using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Api.Services.Systems
{
	public interface IBackgroundTaskQueue
	{
		ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem);
		Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
	}

	public class BackgroundTaskQueue : IBackgroundTaskQueue
	{
		private readonly Channel<Func<CancellationToken, Task>> _queue;

		public BackgroundTaskQueue(int capacity = 100)
		{
			// bounded channel to avoid unbounded memory growth; adjust capacity as needed
			var options = new BoundedChannelOptions(capacity)
			{
				FullMode = BoundedChannelFullMode.Wait,
				SingleReader = true,
				SingleWriter = false
			};
			_queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
		}

		public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, Task> workItem)
		{
			if (workItem == null) throw new ArgumentNullException(nameof(workItem));
			await _queue.Writer.WriteAsync(workItem).ConfigureAwait(false);
		}

		public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
		{
			var workItem = await _queue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
			return workItem;
		}
	}

}
