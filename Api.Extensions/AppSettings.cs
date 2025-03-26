using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Api.Extensions
{
    public class AppSettings
    {
        public LoggingData Logging { get; set; }
        public string AllowedHosts { get; set; }
        public AuthenticationData Authentication { get; set; }
        public SqlConnectionStringData SqlConnectionString { get; set; }
        public LoginData Login { get; set; }
        public ActiveDirectoryData ActiveDirectory { get; set; }
        public EmailData Email { get; set; }
        public MaintenanceData Maintenance { get; set; }
        public DataGridData DataGrid { get; set; }

        public class MaintenanceData
        {
            public int PurgingPeriodInMonth { get; set; }
        }

        public class AuthenticationData
        {
            public string SymmetricSecurityKey { get; set; }
            public string APIKey { get; set; }
        }

        public class DataGridData
        {
            public string DateTimeFormat { get; set; }
            public int RowPerPage { get; set; }
        }

        public class EmailData
        {
            public string Smtp { get; set; }
            public int Port { get; set; }
            public string Name { get; set; }
            public string User { get; set; }
            public string Password { get; set; }
        }

        public class LoggingData
        {
            public LogLevel LogLevel { get; set; }
        }

        public class LoginData
        {
            public int Attempts { get; set; }
            public int AllowedReloginAfterMinutes { get; set; }
            public int IdleTimeoutAfterMinutes { get; set; }
            public string SuperAdminUser { get; set; }
            public string SuperAdminPassword { get; set; }
        }

        public class ActiveDirectoryData
        {
            public string Host { get; set; }
            public string Domain { get; set; }
            public int Port { get; set; }
            public string User { get; set; }
            public string Password { get; set; }
        }

        public class LogLevel
        {
            public string Default { get; set; }

            [JsonPropertyName("Microsoft.AspNetCore")]
            public string MicrosoftAspNetCore { get; set; }
        }

        public class SqlConnectionStringData
        {
            public string SqlServer { get; set; }
            public string PostgreSql { get; set; }
            public string Redis { get; set; }
        }

        public static string ReadAsJson()
        {
            return File.ReadAllText("appsettings.json");
        }

        public static AppSettings Read()
        {
            var json = File.ReadAllText("appsettings.json");
            return JsonSerializer.Deserialize<AppSettings>(json);
        }

        public static T Read<T>(string path)
        {
            var element = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText("appsettings.json"));
            var value = element.GetElement(path).ToString();
            if (value == null)
                return default;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static void Write(object value, params object[] path)
        {
            var json = File.ReadAllText("appsettings.json");
            var node = JsonNode.Parse(json);

            var temp = node;
            foreach (var item in path)
            {
                if (item is string propertyName)
                    temp = temp[propertyName];

                if (item is int index)
                    temp = temp[index];
            }

            temp.ReplaceWith(value);

            json = node.ToJsonString();

            File.WriteAllText("appsettings.json", json);
        }

        public static void Write(AppSettings settings)
        {
            var current = Read();
            current.SqlConnectionString = settings.SqlConnectionString;
            current.Login = settings.Login;
            current.ActiveDirectory = settings.ActiveDirectory;
            current.Email = settings.Email;
            current.Maintenance = settings.Maintenance;
            current.DataGrid = settings.DataGrid;

            var json = JsonSerializer.Serialize(current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("appsettings.json", json);
        }
    }
}
