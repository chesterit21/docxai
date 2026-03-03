using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Api.Extensions
{
    public class AppSettings
    {
        public LoggingData Logging { get; set; }
        public string AllowedHosts { get; set; }
        public ApplicationData ApplicationInfoData { get; set; }
		public AuthenticationData Authentication { get; set; }
        public SqlConnectionStringData SqlConnectionString { get; set; }
        public LoginData Login { get; set; }
        public ActiveDirectoryData ActiveDirectory { get; set; }
        public EmailData MailServer { get; set; }
        public MaintenanceData Maintenance { get; set; }
        public DataGridData DataGrid { get; set; }
		public FileOptions FileOption { get; set; }
		public ConnectionStringProperty ConnectionString { get; set; }
		//public WordEditorParameter WordEj2EditorParameter { get; set; }
		public Waha WAHA { get; set; }
		
		public LicenseDMS License { get; set; }

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

		public class ApplicationData
		{
			public string ApplicationUrl { get; set; }
			public string AppUploadFolder { get; set; }
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

		public class ConnectionStringProperty
		{
			public string Host { get; set; }
			public int Port { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
			public int Timeout { get; set; }
			public int CommandTimeout { get; set; }
            public string Database { get; set; }

			// Override the default ToString() method
			public override string ToString()
			{
				return $"Host={Host};Port={Port};Username={Username};Password={Password};Database={Database};Timeout={Timeout};CommandTimeout={CommandTimeout};SSL Mode=Prefer";
			}
		}
		public class FileOptions
		{
			public string FileExt { get; set; }
			public long MaxSize { get; set; }
		}

		public class WordEditorParameter
		{
			public string SpellcheckDictionaryPath { get; set; }
			public string SpellcheckJsonFilename { get; set; }
		}
		public class Waha
		{
			public string ApiKey { get; set; }
			public string BaseUrl { get; set; }
		}

		public class LicenseDMS
		{
			public string AppsLicense { get; set; }
			public string TotalUserCount { get; set; }
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
			//current.SqlConnectionString = settings.SqlConnectionString;
			current.Login = settings.Login;
            if(settings.ActiveDirectory != null)
                current.ActiveDirectory = settings.ActiveDirectory;
			if (settings.MailServer != null)
				current.MailServer = settings.MailServer;
			if (settings.Maintenance != null)
				current.Maintenance = settings.Maintenance;
			if (settings.DataGrid != null)
				current.DataGrid = settings.DataGrid;
			if (settings.ConnectionString != null)
				current.ConnectionString = settings.ConnectionString;
			if (settings.FileOption != null)
				current.FileOption = settings.FileOption;

			var json = JsonSerializer.Serialize(current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("appsettings.json", json);
        }
    }
}
