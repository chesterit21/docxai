namespace Api.Domain.EntityRequests
{
    public class RequestSetting
    {
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

        public class SqlConnectionStringData
        {
            public string SqlServer { get; set; }
            public string PostgreSql { get; set; }
            public string Redis { get; set; }
        }
    }
}
