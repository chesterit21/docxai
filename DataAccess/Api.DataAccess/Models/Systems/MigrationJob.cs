using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.DataAccess.Models.Systems
{
	public enum MigrationJobStatus
	{
		Pending = 0,
		Running = 1,
		Succeeded = 2,
		Failed = 3
	}

	public class MigrationJob
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string ConnectionStringHash { get; set; } = string.Empty; // do not store raw password
		public MigrationJobStatus Status { get; set; } = MigrationJobStatus.Pending;
		public string? Message { get; set; }
		public DateTime InsertedAt { get; set; } = DateTime.Now;
		public DateTime? StartedAt { get; set; }
		public DateTime? FinishedAt { get; set; }
	}
}
