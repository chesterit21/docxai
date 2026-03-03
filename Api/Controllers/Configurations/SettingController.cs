namespace Docubase.api.Controllers.Configurations
{
	using Api.Domain;
	using Api.Domain.Attributes;
	using Api.Domain.EntityRequests;
	using Api.Domain.EntityRequests.Authentications;
	using Api.Extensions;
	using Api.Services;
	using Api.Services.Systems;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using System.ComponentModel;
	using System.Text.RegularExpressions;

	//[AllowAnonymous]
	[DisplayName("Configurations - Application Setting")]
	[Menu("MnSetting")]
	[Route("[controller]")]
	[ApiController]
	public class SettingController(IHttpContextAccessor accessor, IConfiguration configuration, LanguageService language,
		SettingService settingService, IServiceProvider serviceProvider, IBackgroundTaskQueue taskQueue) : ControllerBase
	{

		//to check from FE if not exist show form setting superuseradmin and password
		[AllowAnonymous]
		[UserAction(UserAction.Read)]
		[HttpGet("get-login-sa")]
		public IActionResult GetSettingWithoutLogin()
		{
			var setting = settingService.GetConfigsuperadmin();
			return ResultFactory.Create(setting);
		}

		[AllowAnonymous]
		[UserAction(UserAction.Insert)]
		[HttpPost("set-login-sa")]
		public async Task<IActionResult> SaveSaSettingAsync([FromBody] RequestSaSetting request)
		{
			//await settingService.CheckToken();
			await settingService.SaveSaSetting(request);
			return ResultFactory.Create("Settings saved", System.Net.HttpStatusCode.OK);
		}

		[AllowAnonymous]
		[HttpPost("login-sa")]
		public async Task<IActionResult> Login([FromBody] RequestUserLogin request)
		{
			var login = await settingService.Login(request);
			return ResultFactory.Create(login);
		}

		[AllowAnonymous]
		[UserAction(UserAction.Read)]
		[HttpGet("config-db")]
		public async Task<IActionResult> GetConfigDB()
		{
			await settingService.CheckToken();
			var setting = settingService.GetConfigConnectionString();
			return ResultFactory.Create(setting);
		}

		[AllowAnonymous]
		[UserAction(UserAction.Insert)]
		[HttpPost("set-config-db")]
		public async Task<IActionResult> SaveConfigDBAsync([FromBody] RequestDbSetting request)
		{
			await settingService.CheckToken();
			var settings = request.ConnectionString;

			// Sanitize Database Name (No spaces, no special chars)
			if (!Regex.IsMatch(settings.Database, @"^[a-zA-Z0-9_]+$"))
			{
				return ResultFactory.Create("Database name contains invalid characters or spaces.", System.Net.HttpStatusCode.BadRequest);
			}

			// Sanitize username (No spaces, no special chars)
			if (!Regex.IsMatch(settings.Username, @"^[a-zA-Z0-9_]+$"))
			{
				return ResultFactory.Create("Username contains invalid characters or spaces.", System.Net.HttpStatusCode.BadRequest);
			}

			// Sanitize Password (No spaces)
			if (settings.Password.Contains(" "))
			{
				return ResultFactory.Create("Password cannot contain spaces.", System.Net.HttpStatusCode.BadRequest);
			}

			// Test Connection
			var connString = $"Host={settings.Host};Port={settings.Port};Username={settings.Username};Password={settings.Password};Timeout={(settings.Timeout == 0 ? 30 : settings.Timeout)};CommandTimeout={(settings.CommandTimeout == 0 ? 30 : settings.CommandTimeout)};SSL Mode=Prefer";

			try
			{
				using var conn = new Npgsql.NpgsqlConnection(connString);
				await conn.OpenAsync();

				// Note: Writing to appsettings.json at runtime requires physical file access
				await settingService.SaveDBSetting(request);
				

				request.ConnectionString.Password = settings.Password;
				request.ConnectionString.Timeout = settings.Timeout == 0 ? 30 : settings.Timeout;
				request.ConnectionString.CommandTimeout = settings.CommandTimeout == 0 ? 60 : settings.CommandTimeout;
				string? userConn = request.ConnectionString.ToString();

				//_dbProvider.UpdateConnectionString(userConn);
				//for poll job status using endpoint setting/migration-status/{jobId}

				var jobId = Guid.NewGuid();
				// create a scope per job to resolve services and DataContext
				using var scope = serviceProvider.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<Api.DataAccess.DataContext>();
				var migrationService = scope.ServiceProvider.GetRequiredService<Api.Services.Systems.MigrationService>();
				await migrationService.PrepareTargetDatabaseAsync(userConn, jobId);

				await taskQueue.QueueBackgroundWorkItemAsync(async ct =>
				{
					await migrationService.ExecuteMigrationTaskAsync(userConn, jobId, ct);
				});

				return Ok(new { JobId = jobId, Message = "Database prepared. Migration started." });

				//// enqueue background work
				//await taskQueue.QueueBackgroundWorkItemAsync(async ct =>
				//{
					
				//	try
				//	{
				//		// do the migration (use the method that accepts a conn string)
				//		await migrationService.ApplyMigrationsAsync(userConn).ConfigureAwait(false);

				//		// create job in DB (use a scope to get DataContext)
				//		db.MigrationJobs.Add(job);
				//		await db.SaveChangesAsync();

				//		// mark running
				//		var jobRecord = await db.MigrationJobs.FindAsync(new object[] { job.Id }, ct);
				//		if (jobRecord != null)
				//		{
				//			jobRecord.Status = Api.DataAccess.Models.Systems.MigrationJobStatus.Running;
				//			jobRecord.StartedAt = DateTime.Now;
				//			await db.SaveChangesAsync(ct);
				//		}

				//		// run seeder only once (guard by flag file)
				//		string flagFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed1.flag");
				//		if (!System.IO.File.Exists(flagFilePath))
				//		{
				//			try
				//			{
				//				// Seeder creates its own scope internally; await to capture failures
				//				await Seeder.SeedItAsync(scope.ServiceProvider).ConfigureAwait(false);

				//				// write flag so seeding doesn't repeat
				//				System.IO.File.WriteAllText(flagFilePath, $"Seeded on {DateTime.Now:O}");
				//			}
				//			catch (Exception seedEx)
				//			{
				//				// record seeder failure into job message but do not rethrow — mark job as Failed below
				//				jobRecord = await db.MigrationJobs.FindAsync(new object[] { job.Id }, ct);
				//				if (jobRecord != null)
				//				{
				//					jobRecord.Message = (jobRecord.Message ?? string.Empty) + $" Seeder error: {seedEx.Message}";
				//					await db.SaveChangesAsync(ct);
				//				}
				//				throw; // bubble to outer catch to mark job Failed
				//			}
				//		}

				//		// mark succeeded
				//		if (jobRecord != null)
				//		{
				//			jobRecord.Status = Api.DataAccess.Models.Systems.MigrationJobStatus.Succeeded;
				//			jobRecord.FinishedAt = DateTime.Now;
				//			jobRecord.Message = "Migrations applied successfully";
				//			await db.SaveChangesAsync(ct);
				//		}
				//	}
				//	catch (Exception ex)
				//	{
				//		// update job as failed
				//		try
				//		{
				//			var jobRecord = await db.MigrationJobs.FindAsync(new object[] { job.Id }, ct);
				//			if (jobRecord != null)
				//			{
				//				jobRecord.Status = Api.DataAccess.Models.Systems.MigrationJobStatus.Failed;
				//				jobRecord.FinishedAt = DateTime.Now;
				//				jobRecord.Message = ex.Message.Length > 2000 ? ex.Message.Substring(0, 2000) : ex.Message;
				//				await db.SaveChangesAsync(ct);
				//			}
				//		}
				//		catch
				//		{
				//			// best-effort update; swallowing here avoids killing the worker
				//		}
				//	}
				//});

				//// return job id to caller so they can poll
				//return ResultFactory.Create($"Connection successful. Settings saved. Migrations queued. Job Id : {job.Id}", System.Net.HttpStatusCode.Accepted);			

				////return ResultFactory.Create(" Settings saved.", System.Net.HttpStatusCode.OK);
			}
			catch (Exception ex)
			{
				return ResultFactory.Create("Connection failed, please make sure the configuration property has right value.", System.Net.HttpStatusCode.InternalServerError);
			}
		}

		[UserAction(UserAction.Read)]
		[HttpGet]
		public async Task<IActionResult> GetSettingAsync()
		{
			//await settingService.CheckToken();
			var setting = settingService.Get();
			return ResultFactory.Create(setting);
		}

		[UserAction(UserAction.Insert)]
		[HttpPost]
		public async Task<IActionResult> SaveSettingAsync([FromBody] RequestSetting request)
		{
			//await settingService.CheckToken();
			await settingService.Save(request);

			///* extract connection string from request as you already do */
			//string? userConn = request.ConnectionString.ToString();

			//if (!string.IsNullOrWhiteSpace(userConn))
			//{

			//	// create job record immediately and persist
			//	var job = new Api.DataAccess.Models.Systems.MigrationJob
			//	{
			//		// store masked or hashed connection info; avoid raw credentials in DB
			//		ConnectionStringHash = (userConn.Length > 50) ? userConn.Substring(0, 50) : userConn
			//	};

			//	// create job in DB (use a scope to get DataContext)
			//	using (var scope = serviceProvider.CreateScope())
			//	{
			//		var db = scope.ServiceProvider.GetRequiredService<Api.DataAccess.DataContext>();
			//		db.MigrationJobs.Add(job);
			//		await db.SaveChangesAsync();
			//	}


			//	// enqueue background work
			//	await taskQueue.QueueBackgroundWorkItemAsync(async ct =>
			//	{
			//		// create a scope per job to resolve services and DataContext
			//		using var scope = serviceProvider.CreateScope();
			//		var db = scope.ServiceProvider.GetRequiredService<Api.DataAccess.DataContext>();
			//		var migrationService = scope.ServiceProvider.GetRequiredService<Api.Services.Systems.MigrationService>();
			//		try
			//		{
			//			// mark running
			//			var jobRecord = await db.MigrationJobs.FindAsync(new object[] { job.Id }, ct);
			//			if (jobRecord != null)
			//			{
			//				jobRecord.Status = Api.DataAccess.Models.Systems.MigrationJobStatus.Running;
			//				jobRecord.StartedAt = DateTime.Now;
			//				await db.SaveChangesAsync(ct);
			//			}

			//			// do the migration (use the method that accepts a conn string)
			//			await migrationService.ApplyMigrationsAsync(userConn).ConfigureAwait(false);

			//			// run seeder only once (guard by flag file)
			//			string flagFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed.flag");
			//			if (!System.IO.File.Exists(flagFilePath))
			//			{
			//				try
			//				{
			//					// Seeder creates its own scope internally; await to capture failures
			//					await Seeder.SeedItAsync(scope.ServiceProvider).ConfigureAwait(false);

			//					// write flag so seeding doesn't repeat
			//					System.IO.File.WriteAllText(flagFilePath, $"Seeded on {DateTime.Now:O}");
			//				}
			//				catch (Exception seedEx)
			//				{
			//					// record seeder failure into job message but do not rethrow — mark job as Failed below
			//					jobRecord = await db.MigrationJobs.FindAsync(new object[] { job.Id }, ct);
			//					if (jobRecord != null)
			//					{
			//						jobRecord.Message = (jobRecord.Message ?? string.Empty) + $" Seeder error: {seedEx.Message}";
			//						await db.SaveChangesAsync(ct);
			//					}
			//					throw; // bubble to outer catch to mark job Failed
			//				}
			//			}

			//			// mark succeeded
			//			if (jobRecord != null)
			//			{
			//				jobRecord.Status = Api.DataAccess.Models.Systems.MigrationJobStatus.Succeeded;
			//				jobRecord.FinishedAt = DateTime.Now;
			//				jobRecord.Message = "Migrations applied successfully";
			//				await db.SaveChangesAsync(ct);
			//			}
			//		}
			//		catch (Exception ex)
			//		{
			//			// update job as failed
			//			try
			//			{
			//				var jobRecord = await db.MigrationJobs.FindAsync(new object[] { job.Id }, ct);
			//				if (jobRecord != null)
			//				{
			//					jobRecord.Status = Api.DataAccess.Models.Systems.MigrationJobStatus.Failed;
			//					jobRecord.FinishedAt = DateTime.Now;
			//					jobRecord.Message = ex.Message.Length > 2000 ? ex.Message.Substring(0, 2000) : ex.Message;
			//					await db.SaveChangesAsync(ct);
			//				}
			//			}
			//			catch
			//			{
			//				// best-effort update; swallowing here avoids killing the worker
			//			}
			//		}
			//	});

			//	// return job id to caller so they can poll
			//	return ResultFactory.Create($"Settings saved. Migrations queued. Job Id : {job.Id}", System.Net.HttpStatusCode.Accepted);
			//}

			return ResultFactory.Create("Settings saved", System.Net.HttpStatusCode.OK);



			//// Attempt to run migrations using the user-provided connection string (if present).
			//// 1) Safely obtain a connection string from the request (try common locations).
			//string? userConn = null;
			//if (request?.ConnectionString != null)
			//{
			//	// Some payloads may include a nested object named SqlConnectionString with PostgreSql
			//	var sqlConn = request.ConnectionString as dynamic;
			//	if (string.IsNullOrWhiteSpace(userConn) && !string.IsNullOrWhiteSpace(request.ConnectionString.Host))
			//	{
			//		// You may want to include Database, Port and other options as needed.
			//		userConn = $"Host={request.ConnectionString.Host};Username={request.ConnectionString.Username};Password={request.ConnectionString.Password}";
			//	}
			//}

			//if (!string.IsNullOrWhiteSpace(userConn))
			//{
			//	try
			//	{
			//		// WARNING: Running migrations can take time -- consider running this in a background job
			//		// or queuing it instead of blocking the HTTP request in production.
			//		await migrationService.ApplyMigrationsAsync(userConn);

			//		string flagFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "seed.flag");
			//		if (!System.IO.File.Exists(flagFilePath))
			//		{
			//			// do not block the request, run seeder in background
			//			_ = Task.Run(() => Seeder.SeedItAsync(HttpContext.RequestServices));
			//			System.IO.File.WriteAllText(flagFilePath, "Seeded on " + DateTime.Now);
			//		}
			//	}
			//	catch (Exception ex)
			//	{
			//		// swallow/log the exception depending on your logging strategy.
			//		// Here we include it in the response metadata but still return success for settings save.
			//		// Replace with proper logger in production.
			//		string strMsg = $"Settings saved, but migration failed. error = {ex.Message}";
			//		return ResultFactory.Create(strMsg, System.Net.HttpStatusCode.Accepted);
			//	}
			//}

			//return ResultFactory.Create("Settings saved", System.Net.HttpStatusCode.OK);
		}

		[AllowAnonymous]
		[HttpGet("migration-status/{jobId:guid}")]
		public async Task<IActionResult> GetMigrationStatusAsync(Guid jobId)
		{
			//using var scope = serviceProvider.CreateScope();
			//var db = scope.ServiceProvider.GetRequiredService<Api.DataAccess.DataContext>();
			//var job = await db.MigrationJobs.FindAsync(jobId);
			var job = await settingService.CheckStatus(jobId);
			if (job == null) return ResultFactory.Create("Job not found", System.Net.HttpStatusCode.NotFound);
			return ResultFactory.Create(new
			{
				job.Id,
				job.Status,
				job.Message,
				job.InsertedAt,
				job.StartedAt,
				job.FinishedAt
			});
		}

		//[HttpPost("setup-database")]
		//public async Task<IActionResult> SetupDatabase([FromBody] DbSettings settings)
		//{
		//	var connStr = BuildConnectionString(settings);

		//	await EnsureDatabaseExistsAsync(connStr); // ✅ Create DB if needed

		//	var options = new DbContextOptionsBuilder<YourDbContext>()
		//		.UseNpgsql(connStr)
		//		.Options;

		//	using var context = new YourDbContext(options);
		//	context.Database.Migrate(); // ✅ Apply migrations

		//	return Ok("Database created and migrated.");
		//}

	}
}
