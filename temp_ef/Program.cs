using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = "Host=203.175.125.188;Port=5432;Database=dms;Username=postgres;Password=2f7b891595d07e3ae51f87c931aba508;Timeout=30;CommandTimeout=60;SSL Mode=Prefer;";

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        using var cmd1 = new NpgsqlCommand(@"
            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                ""MigrationId"" character varying(150) NOT NULL,
                ""ProductVersion"" character varying(32) NOT NULL,
                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
            );", conn);
        cmd1.ExecuteNonQuery();

        using var cmd2 = new NpgsqlCommand(@"
            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"") 
            VALUES ('20250421064354_init', '10.0.3') 
            ON CONFLICT (""MigrationId"") DO NOTHING;", conn);
        cmd2.ExecuteNonQuery();

        Console.WriteLine("Migration record inserted successfully!");
    }
}
