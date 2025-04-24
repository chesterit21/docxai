using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Authentications;
using Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Api.Services.Systems
{
    public class SettingService(IHttpContextAccessor accessor, IConfiguration configuration, LanguageService language)
    {
        public async Task CheckToken()
        {
            var headers = accessor.HttpContext.Request.Headers;
            var local = headers["x-lang"].FirstOrDefault()?.ToUpper() ?? "ID";
            var lang = await language.GetLanguage(LangCodes.PassInvalid);
            var message = local == "ID" ? lang.Id : lang.En;

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

        public object GetConfig()
        {
            var setting = AppSettings.Read();
            return new
            {
                setting.DataGrid,
                Login = new
                {
                    setting.Login.IdleTimeoutAfterMinutes
                }
            };
        }

        public RequestSetting Get()
        {
            var setting = AppSettings.Read();
            var response = setting.CopyProperties<RequestSetting>();

            response.Login.SuperAdminPassword = configuration["Login:SuperAdminPassword"]?.Decrypt();
            response.ActiveDirectory.Password = configuration["ActiveDirectory:Password"]?.Decrypt();

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

        public void Save(RequestSetting request)
        {
            if (request == null)
                throw new ApiException("input-empty");

            request.SqlConnectionString.SqlServer = EncryptSqlPassword(request.SqlConnectionString.SqlServer);
            request.SqlConnectionString.PostgreSql = EncryptSqlPassword(request.SqlConnectionString.PostgreSql);

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
        }
    }
}
