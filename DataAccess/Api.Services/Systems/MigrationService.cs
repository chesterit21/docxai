using Api.DataAccess;
using Api.DataAccess.Models.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace Api.Services.Systems
{
	public class MigrationService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IConfiguration _configuration;

		public MigrationService(IServiceProvider serviceProvider, IConfiguration configuration)
		{
			_serviceProvider = serviceProvider;
			_configuration = configuration;
		}

		// run migrations for the configured DataContext (uses DI configuration)
		public async Task ApplyMigrationsAsync()
		{
			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DataContext>();

			try
			{
				await db.Database.MigrateAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				// use proper logging in real code
				Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
				throw;
			}
		}

		// Optional: run migrations using explicit connection string (if you must)
		public async Task ApplyMigrationsAsync(string userDefinedConnectionString)
		{
			// 1) ensure the MigrationJobs table exists so we can log job rows prior to applying EF migrations
			await EnsureMigrationJobTableExistsAsync(userDefinedConnectionString).ConfigureAwait(false);

			// 2) build options for your actual DataContext (not DbContext)
			var options = new DbContextOptionsBuilder<DataContext>()
				.UseNpgsql(userDefinedConnectionString, o => o.CommandTimeout(600))// 10 minutes
				.Options;

			// DataContext requires IHttpContextAccessor in constructor; provide a simple one for migration runs
			var httpAccessor = new HttpContextAccessor();
			await using var db = new DataContext(options, httpAccessor);
			try
			{
				await db.Database.MigrateAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new Exception($"EF Migration failed: {ex.Message}", ex);
			}

			//await using var db = new DbContext(options);
			//await db.Database.MigrateAsync().ConfigureAwait(false);
		}

		public async Task PrepareTargetDatabaseAsync(string userConn, Guid jobId)
		{
			await EnsureMigrationJobTableExistsAsync(userConn);
			var options = new DbContextOptionsBuilder<DataContext>()
				.UseNpgsql(userConn).Options;

			string ConnectionStringHash = (userConn.Length > 50) ? userConn.Substring(0, 50) : userConn;

			await using var db = new DataContext(options, new HttpContextAccessor());
			db.MigrationJobs.Add(new MigrationJob
			{
				Id = jobId,
				Status = MigrationJobStatus.Pending,
				InsertedAt = DateTime.Now,
				ConnectionStringHash = ConnectionStringHash,
				Message = "Database initialized. Waiting for migrations..."
			});
			await db.SaveChangesAsync();
		}

		// Idempotent: creates the MigrationJobs table if it's not present. Safe to call repeatedly.
		private async Task EnsureMigrationJobTableExistsAsync(string connString)
		{
			var builder = new NpgsqlConnectionStringBuilder(connString);
			string targetDatabase = builder.Database;
			if (string.IsNullOrWhiteSpace(targetDatabase))
				throw new Exception("The connection string must contain a 'Database' name.");
			var adminBuilder = new NpgsqlConnectionStringBuilder(connString) { Database = "postgres" };
			await using (var adminConn = new NpgsqlConnection(adminBuilder.ConnectionString))
			{
				try
				{
					await adminConn.OpenAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					string mssg = ex.Message;
				}

				var checkDbCmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{targetDatabase}'", adminConn);
				var dbExists = await checkDbCmd.ExecuteScalarAsync().ConfigureAwait(false);

				if (dbExists == null)
				{
					// Note: Database names should be quoted in case they have uppercase or special chars
					var createDbCmd = new NpgsqlCommand($"CREATE DATABASE \"{targetDatabase}\"", adminConn);
					await createDbCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
				}
			}

			await using (var conn = new NpgsqlConnection(connString))
			{
				await conn.OpenAsync().ConfigureAwait(false);
				try
				{
					var checkTableCmd = new NpgsqlCommand("SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = 'MigrationJobs');", conn);
					var existsResult = await checkTableCmd.ExecuteScalarAsync().ConfigureAwait(false);
					bool tableExists = existsResult is bool b && b;

					if (!tableExists)
					{
						var createSql = @"
						CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";
						CREATE TABLE IF NOT EXISTS public.""MigrationJobs"" (
							""Id"" uuid PRIMARY KEY,
							""ConnectionStringHash"" text,
							""Status"" integer NOT NULL,
							""Message"" text,
							""InsertedAt"" timestamp NOT NULL,
							""StartedAt"" timestamp,
							""FinishedAt"" timestamp
						);";

						var createCmd = new NpgsqlCommand(createSql, conn);
						await createCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
					}
				}
				catch (Exception xe)
				{
				}
			}
		}

		public async Task ExecuteMigrationTaskAsync(string userConn, Guid jobId, CancellationToken ct)
		{
			var options = new DbContextOptionsBuilder<DataContext>()
				.UseNpgsql(userConn).Options;

			await using var db = new DataContext(options, new HttpContextAccessor());

			try
			{
				// 1. Mark as Running
				var job = await db.MigrationJobs.FindAsync(jobId);
				if (job != null)
				{
					job.Status = MigrationJobStatus.Running;
					job.StartedAt = DateTime.Now;
					db.MigrationJobs.Update(job);
					await db.SaveChangesAsync(ct);
				}

				// 2. Run EF Migrations
				await db.Database.MigrateAsync(ct);

				// 3. Run Seeder
				await Seeder.SeedItAsync(db); // Pass the context we already built

				// 4. Success
				job.Status = MigrationJobStatus.Succeeded;
				job.FinishedAt = DateTime.Now;
				job.Message = "All migrations and seeding completed.";
				db.MigrationJobs.Update(job);
				await db.SaveChangesAsync(ct);
			}
			catch (Exception ex)
			{
				// 5. Failure
				var job = await db.MigrationJobs.FindAsync(jobId);
				if (job != null)
				{
					job.Status = MigrationJobStatus.Failed;
					job.Message = ex.Message;
					job.FinishedAt = DateTime.Now;
					await db.SaveChangesAsync();
				}
			}
		}
	}
}
