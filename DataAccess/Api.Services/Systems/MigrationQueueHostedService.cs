using Api.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services.Systems
{
	public class MigrationQueueHostedService(IBackgroundTaskQueue _taskQueue, ILogger<MigrationQueueHostedService> _logger,
		IDatabaseGuard _dbGuard) : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("MigrationQueueHostedService started.");
			while (!stoppingToken.IsCancellationRequested)
			{
				if (await _dbGuard.IsServerReachableAsync(stoppingToken))
				{
					_logger.LogInformation("Database is ready. Starting background processing...");
					break; // Exit the "waiting" loop
				}
				_logger.LogWarning("Database not ready or config missing. Retrying in 10 seconds...");
				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var workItem = await _taskQueue.DequeueAsync(stoppingToken).ConfigureAwait(false);
					try
					{
						await workItem(stoppingToken).ConfigureAwait(false);
					}
					catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
					{
						// shutdown requested
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Error occurred executing background migration task.");
					}
				}
				catch (OperationCanceledException)
				{
					// service stopping
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error dequeuing background task.");
					// small delay to avoid fast-fail loop
					await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
				}
			}
			_logger.LogInformation("MigrationQueueHostedService stopping.");
		}
	}
}
