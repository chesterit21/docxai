using Api.DataAccess;
using Api.Extensions;
using Api.Services.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Docubase.api.Middleware
{
	public class SetupModeMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IConfiguration _configuration;
		private readonly IDatabaseGuard _dbGuard;
		private readonly ILogger<SetupModeMiddleware> _logger;

		public SetupModeMiddleware(RequestDelegate next, IConfiguration configuration, IDatabaseGuard dbGuard, ILogger<SetupModeMiddleware> logger)
		{
			_next = next;
			_configuration = configuration;
			_dbGuard = dbGuard;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context, IOptionsSnapshot<AppSettings.ConnectionStringProperty> dbOptions,
			IOptionsSnapshot<AppSettings.LoginData> saLogin, DataContext db)
		{
			var path = context.Request.Path.Value?.ToLower();

			//Scenario,Logic,API Response
			//No DB + No Admin, Allow POST /api/setup/admin,200 OK(Set your admin)
			//No DB + Admin Exists, Require Admin Auth,401 Unauthorized
			//DB Exists,Normal Operation,200 OK(Standard API)


			// 1. Define "Safe" endpoints that don't need a DB
			//setting/get-login-sa
			//setting/set-login-sa
			//setting/config-db
			//setting/set-config-db

			bool isSetupEndpoint = path.Contains("/setting");


			if (string.IsNullOrEmpty(saLogin.Value.SuperAdminUser))
			{
				// No DB + No Admin, Allow POST /api/setting/set-login-sa, 200 OK (Set your admin)
				if (path == "/setting/set-login-sa" && context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
				{
					await _next(context);
					return;
				}
				//else if (string.IsNullOrEmpty(dbOptions.Value.Host) && !isSetupEndpoint)
				//else
				//{
				//	context.Response.StatusCode = 412; // Precondition Failed
				//	context.Response.ContentType = "application/json";
				//	await context.Response.WriteAsync(JsonSerializer.Serialize(new
				//	{
				//		status = "SetupRequired",
				//		message = "Please complete Superadmin Account at setting/set-login-sa endpoint, then database setup at setting/set-config-db endpoint."
				//	}));
				//	return;
				//}
			}
			else
			{
				// No DB + Admin Exists, Require Admin Auth, 401 Unauthorized
				if (string.IsNullOrEmpty(dbOptions.Value.Host) && !isSetupEndpoint)
				{
					context.Response.StatusCode = 401; // Unauthorized
					//context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
					context.Response.ContentType = "application/json";
					await context.Response.WriteAsync(JsonSerializer.Serialize(new
					{
						status = "Unauthorized",
						message = "Database is not configured. Please login as Super Admin and complete database setup at setting/set-config-db endpoint."
					}));
					return;
				}
				else
				{
					//var isDbReady = await IsDatabaseReadyAsync(dbOptions);
					var isDbReady = await _dbGuard.IsReadyAsync(context.RequestAborted);
					if (!isDbReady && !isSetupEndpoint)
					{
						_logger.LogWarning("Blocking request to {Path}: Database/Migration not ready.", path);

						context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
						context.Response.ContentType = "application/json";
						await context.Response.WriteAsJsonAsync(new
						{
							error = "System Initializing",
							message = "The database is currently being prepared. Please try again in a few moments or visit /settings."
						});
					}
					//else
					//{
					//	////var isDbReady = await _dbGuard.IsServerReachableAsync(context.RequestAborted);
					//	await _next(context);
					//}
				}
			}

			await _next(context);
		}

		private async Task<bool> IsDatabaseReadyAsync(IOptionsSnapshot<AppSettings.ConnectionStringProperty> dbOptions)
		{	
			string _connectionString = dbOptions.Value.ToString();
			if (string.IsNullOrEmpty(_connectionString)) return false;

			try
			{				
				using var conn = new Npgsql.NpgsqlConnection(_connectionString);
				await conn.OpenAsync();
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
