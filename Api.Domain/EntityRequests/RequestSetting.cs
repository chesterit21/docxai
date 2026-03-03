using System.ComponentModel;
using System.Text.Json.Serialization;
using static Api.Domain.EntityRequests.RequestSetting;

namespace Api.Domain.EntityRequests
{
	public class RequestSetting
	{
		public ConnectionStringProperty ConnectionString { get; set; }
		public LoginData Login { get; set; }
		public ApplicationData ApplicationInfoData { get; set; }
		public ActiveDirectoryData ActiveDirectory { get; set; }
		public EmailData MailServer { get; set; }
		public MaintenanceData Maintenance { get; set; }
		public DataGridData DataGrid { get; set; }
		public FileOptions FileOption { get; set; }

		//public WordEditorParameter WordEj2EditorParameter { get; set; }
		public Waha WAHA { get; set; }
		//[JsonIgnore]
		public LicenseDMS License { get; set; }

		public class MaintenanceData
		{
			public int PurgingPeriodInMonth { get; set; }
		}

		public class ApplicationData
		{
			public string ApplicationUrl { get; set; }
			public string AppUploadFolder { get; set; }
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
			//public override string ToString()
			//{
			//	return $"Host={Host};Port={Port};Username={Username};Password={Password};Timeout={Timeout};CommandTimeout={CommandTimeout};";

			//}
		}

		public class FileOptions
		{
			public string FileExt { get; set; }
			public long MaxSize { get; set; }
		}

		public class SqlConnectionString
		{
			public string SqlServer { get; set; }
			public string PostgreSql { get; set; }
			public string Redis { get; set; }
		}

		public class WordEditorParameter
		{
			public string SpellcheckDictionaryPath { get; set; }
			public string SpellcheckJsonFilename { get; set; }
		}

		public class Waha
		{
			public string ApiKey { get; set; }
			public string ServerUrl { get; set; }
		}

		public class LicenseDMS
		{
			public string AppsLicense { get; set; }
			public int UserCount { get; set; }
		}
	}

	public class RequestSaSetting
	{
		public LoginSA Login { get; set; }
		public class LoginSA
		{
			public string SuperAdminUser { get; set; }
			public string SuperAdminPassword { get; set; }
		}
	}	

	public class RequestDbSetting
	{
		public ConnectionStringDB ConnectionString { get; set; }
		
	}

	public class ConnectionStringDB
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string Database { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public int Timeout { get; set; }
		public int CommandTimeout { get; set; }

		// Override the default ToString() method
		public override string ToString()
		{
			return $"Host={Host};Port={Port};Username={Username};Password={Password};Database={Database};Timeout={Timeout};CommandTimeout={CommandTimeout};SSL Mode=Prefer";
		}
	}
}
