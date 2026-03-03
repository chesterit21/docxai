using Api.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services.Systems
{
	public interface IDatabaseGuard
	{
		// Checks if we can connect to the server/postgres (For the Migration Service)
		Task<bool> IsServerReachableAsync(CancellationToken ct);

		// Checks if the target DB is fully migrated (For all other services)
		Task<bool> IsReadyAsync(CancellationToken ct);
	}

	public class DatabaseGuard(IServiceProvider serviceProvider,
		IOptionsMonitor<AppSettings.ConnectionStringProperty> _dbOptions,
		ILogger<DatabaseGuard> _logger) : IDatabaseGuard
	{
		public async Task<bool> IsReadyAsync(CancellationToken ct)
		{
			var settings = _dbOptions.CurrentValue;
			string pwd = settings.Password;
			if (!string.IsNullOrEmpty(pwd))
			{
				using var scope = serviceProvider.CreateScope();
				var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
				pwd = config["ConnectionString:Password"];
			}			

			// 1. Build connection string locally (prevents corrupting global settings)
			var builder = new NpgsqlConnectionStringBuilder
			{
				Host = settings.Host,
				Port = settings.Port == 0 ? 5432 : settings.Port,
				Username = settings.Username,
				Password = pwd.Decrypt(),
				Database = settings.Database,
				Timeout = settings.Timeout == 0 ? 10 : settings.Timeout, // Shorter timeout for checks
				CommandTimeout = settings.CommandTimeout == 0 ? 10 : settings.CommandTimeout,
				SslMode = SslMode.Prefer
			};

			try
			{
				await using var conn = new NpgsqlConnection(builder.ToString());
				await conn.OpenAsync(ct);

				// 2. Check if the MigrationJobs table exists and status is "Completed" (e.g., Status = 2)
				var sql = @"
                SELECT EXISTS (
                    SELECT FROM pg_tables WHERE tablename = 'MigrationJobs'
                ) AND EXISTS (
                    SELECT 1 FROM ""MigrationJobs"" WHERE ""Status"" = 2 LIMIT 1
                );";

				await using var cmd = new NpgsqlCommand(sql, conn);
				return (bool)(await cmd.ExecuteScalarAsync(ct) ?? false);
			}
			catch
			{
				return false; // DB not created yet or table missing
			}
		}

		public async Task<bool> IsServerReachableAsync(CancellationToken ct)
		{
			var settings = _dbOptions.CurrentValue;

			string pwd = settings.Password;
			if (!string.IsNullOrEmpty(pwd))
			{
				using var scope = serviceProvider.CreateScope();
				var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
				pwd = config["ConnectionString:Password"];
			}

			// Connect to 'postgres' system db to see if the server is alive
			var builder = new NpgsqlConnectionStringBuilder
			{
				Host = settings.Host,
				Port = settings.Port == 0 ? 5432 : settings.Port,
				Username = settings.Username,
				Password = pwd.Decrypt(),
				Database = "postgres",
				Timeout = 5
			};


			try
			{
				await using var conn = new NpgsqlConnection(builder.ToString());
				await conn.OpenAsync(ct);
				return true;
			}
			catch { return false; }
		}
	}
}
