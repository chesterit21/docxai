using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Authentications;
using Api.Extensions;
using Api.Extensions.Services;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Api.Services.Systems
{
	public class SettingService(IHttpContextAccessor accessor, IConfiguration configuration, 
	LanguageService language, IUserRepository userRepo, IMigrationJobRepository migrationJobRepo, ILicenseManager licenseManager)
	{
		public async Task CheckToken()
		{
			var headers = accessor.HttpContext.Request.Headers;
			var local = headers["x-lang"].FirstOrDefault()?.ToUpper() ?? "ID";
			//var lang = await language.GetLanguage(LangCodes.PassInvalid);
			//var message = local == "ID" ? lang.Id : lang.En;
			//var lang = await language.GetLanguage(LangCodes.PassInvalid);
			var message = local == "ID" ? "Token admin tidak ada atau tidak valid" : "Admin token misssing or incorrect";

			if (!headers.TryGetValue("x-admin-token", out var token))
				throw new ApiException(message, 401);


			var decrypted = token.ToString().Decrypt();
			if (decrypted == null)
				throw new ApiException(message, 401);

			var splits = decrypted.Split(':');
			var usr = splits.ElementAtOrDefault(0);
			var pwd = splits.ElementAtOrDefault(1);

			var username = configuration["Login:SuperAdminUser"];
			var password = configuration["Login:SuperAdminPassword"].Decrypt();

			if (username != usr || password != pwd)
				throw new ApiException(message, 401);
		}

		public async Task<object> Login(RequestUserLogin request)
		{
			var username = configuration["Login:SuperAdminUser"];
			var password = configuration["Login:SuperAdminPassword"].Decrypt();

			if (request.UserName == username && request.Password == password)
			{
				var result = Encryption.Encrypt($"{username}:{password}");
				var obj = new
				{
					adminToken = result,
					usage = "Put x-admin-token as HTTP header"
				};

				return obj;
			}
			else
			{
				var local = accessor.HttpContext.Request.Headers["x-lang"].FirstOrDefault()?.ToUpper() ?? "ID";
				var lang = await language.GetLanguage(LangCodes.PassInvalid);
				var message = local == "ID" ? lang.Id : lang.En;

				throw new ApiException(message, 401);
			}
		}

		//public object GetConfig()
		//{
		//    var setting = AppSettings.Read();
		//    return new
		//    {
		//        setting.DataGrid,
		//        Login = new
		//        {
		//            setting.Login.IdleTimeoutAfterMinutes
		//        }
		//    };
		//}

		public object GetConfigConnectionString()
		{
			var setting = AppSettings.Read();
			return new
			{
				setting.ConnectionString
			};
		}

		public object GetConfigsuperadmin()
		{
			var setting = AppSettings.Read();
			return new
			{
				Login = new
				{
					setting.Login.SuperAdminUser,
					setting.Login.SuperAdminPassword
				}
			};
		}

		public RequestSetting Get()
		{
			var setting = AppSettings.Read();
			var response = setting.CopyProperties<RequestSetting>();

			LicenseInfo infolcs = licenseManager.GetLicenseInfo();
			response.License.UserCount = infolcs.MaxUserCount;
			response.Login.SuperAdminPassword = configuration["Login:SuperAdminPassword"]?.Decrypt();
			response.ActiveDirectory.Password = configuration["ActiveDirectory:Password"]?.Decrypt();
			response.ConnectionString.Password = configuration["ConnectionString:Password"]?.Decrypt();

			return response;
		}

		private string EncryptSqlPassword(string connectionString)
		{
			if (!string.IsNullOrWhiteSpace(connectionString) && connectionString.Contains("password", StringComparison.OrdinalIgnoreCase))
			{
				var splits = connectionString.Split(';');
				var pswd = splits.FirstOrDefault(x => x.Contains("password", StringComparison.OrdinalIgnoreCase));
				var plainPassword = pswd.Split('=')[1].Trim();

				if (plainPassword.Decrypt() == null)
				{
					var encptPassword = Encryption.Encrypt(plainPassword);
					return connectionString.Replace(plainPassword, encptPassword);
				}
			}

			return connectionString;
		}

		public async Task Save(RequestSetting request)
		{
			//string ext = configuration.GetValue<string>("FileOptions:FileExt");
			if (request == null)
				throw new ApiException("input-empty");

			// validate ConnectionString object and required properties
			if (request.ConnectionString == null
				|| string.IsNullOrWhiteSpace(request.ConnectionString.Host)
				|| (request.ConnectionString.Port == 0)
				|| string.IsNullOrWhiteSpace(request.ConnectionString.Username)
				|| string.IsNullOrWhiteSpace(request.ConnectionString.Password))
			{
				// return a consistent API error used elsewhere in the project
				throw new ApiException("input-empty in one of propery connection string");
			}

			request.ConnectionString.Password = request.ConnectionString.Password.Encrypt();

			var setting = request.CopyProperties<AppSettings>();

			if (setting.Login == null)
				setting.Login = new AppSettings.LoginData();

			if (setting.ActiveDirectory == null)
				setting.ActiveDirectory = new AppSettings.ActiveDirectoryData();

			if (string.IsNullOrWhiteSpace(setting.Login.SuperAdminUser))
			{
				setting.Login.SuperAdminUser = "superadmin";
				setting.Login.SuperAdminPassword = "superadmin".Encrypt();
			}
			else
			{
				setting.Login.SuperAdminPassword = setting.Login.SuperAdminPassword.Encrypt();
			}

			if (string.IsNullOrWhiteSpace(setting.ActiveDirectory.Password))
			{
				setting.ActiveDirectory.User = "aduser";
				setting.ActiveDirectory.Password = "P@ssw0rd".Encrypt();
			}
			else
			{
				setting.ActiveDirectory.Password = setting.ActiveDirectory.Password.Encrypt();
			}

			AppSettings.Write(setting);

			//to make sure connection string was true configure
			//using var scope = app.Services.CreateScope();
			//var dbContext = scope.ServiceProvider.GetRequiredService<YourDbContext>();

			//// Create the database if it doesn't exist
			//dbContext.Database.EnsureCreated();

			//List<User> users = await userRepo.GetUsers(1, 10);
			//if (users == null)
			//throw new ApiException("Configuration for database was not right, please revise configuration");
		}

		public static async Task EnsureDatabaseExistsAsync(string connectionString)
		{
			var builder = new NpgsqlConnectionStringBuilder(connectionString);
			var databaseName = builder.Database;

			builder.Database = "postgres"; // Connect to default db
			using var conn = new NpgsqlConnection(builder.ConnectionString);
			await conn.OpenAsync();

			using var cmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'", conn);
			var exists = await cmd.ExecuteScalarAsync();

			if (exists == null)
			{
				using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", conn);
				await createCmd.ExecuteNonQueryAsync();
			}
		}

		public async Task SaveSaSetting(RequestSaSetting request)
		{
			if (request == null)
				throw new ApiException("input-empty");

			// validate ConnectionString object and required properties
			if (request.Login == null
				|| string.IsNullOrWhiteSpace(request.Login.SuperAdminUser)
				|| string.IsNullOrWhiteSpace(request.Login.SuperAdminPassword))
			{
				throw new ApiException("input-empty in one of propery login super admin");
			}

			request.Login.SuperAdminPassword = request.Login.SuperAdminPassword.Encrypt();

			//var setting = request.CopyProperties<AppSettings>();
			var setting = AppSettings.Read();

			if (setting.Login != null)
				setting.Login = new AppSettings.LoginData()
				{
					Attempts = setting.Login.Attempts,
					AllowedReloginAfterMinutes = setting.Login.AllowedReloginAfterMinutes,
					IdleTimeoutAfterMinutes = setting.Login.IdleTimeoutAfterMinutes,
					SuperAdminUser = request.Login.SuperAdminUser,
					SuperAdminPassword = request.Login.SuperAdminPassword
				};

			if (setting.ActiveDirectory == null)
				setting.ActiveDirectory = new AppSettings.ActiveDirectoryData();

			AppSettings.Write(setting);
		}

		public async Task SaveDBSetting(RequestDbSetting request)
		{
			if (request == null)
				throw new ApiException("input-empty");

			// validate ConnectionString object and required properties
			if (request.ConnectionString == null
				|| string.IsNullOrWhiteSpace(request.ConnectionString.Host)
				|| (request.ConnectionString.Port == 0)
				|| string.IsNullOrWhiteSpace(request.ConnectionString.Username)
				|| string.IsNullOrWhiteSpace(request.ConnectionString.Password)
				|| string.IsNullOrWhiteSpace(request.ConnectionString.Database))
			{
				// return a consistent API error used elsewhere in the project
				throw new ApiException("input-empty in one of propery connection string");
			}

			//var setting = request.CopyProperties<AppSettings>();
			var setting = AppSettings.Read();

			if (setting.ConnectionString != null)
				setting.ConnectionString = new AppSettings.ConnectionStringProperty()
				{
					Username = request.ConnectionString.Username,
					Password = request.ConnectionString.Password.Encrypt(),
					Database = request.ConnectionString.Database,
					Host = request.ConnectionString.Host,
					Port = request.ConnectionString.Port,
					Timeout = request.ConnectionString.Timeout == 0 ? 30 : request.ConnectionString.Timeout,
					CommandTimeout = request.ConnectionString.CommandTimeout == 0 ? 60 : request.ConnectionString.CommandTimeout
				};

			AppSettings.Write(setting);
		}

		public async Task<MigrationJob> CheckStatus(Guid jobId)
		{
			return await migrationJobRepo.CheckStatus(jobId);
			//var job = await migrationJobRepo.GetById(jobId);
			//if (job == null)
			//	throw new ApiException("migration-job-not-found");
			//return job;
		}

	}
}
