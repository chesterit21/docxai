using Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.DataAccess
{
    public static class DI
    {
        public static void RegisterDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            //string sqlServerConnectionString = configuration.GetValue<string>("SqlConnectionString:SqlServer");
            //if (!string.IsNullOrEmpty(sqlServerConnectionString))
            //{
            //    if (sqlServerConnectionString.Contains("password", StringComparison.OrdinalIgnoreCase))
            //    {
            //        var splits = sqlServerConnectionString.Split(';');
            //        var pswd = splits.FirstOrDefault(x => x.Contains("password", StringComparison.OrdinalIgnoreCase));
            //        var plainPassword = pswd.Split('=')[1];
            //        var encptPassword = Encryption.Decrypt(plainPassword);

            //        sqlServerConnectionString = sqlServerConnectionString.Replace(plainPassword, encptPassword);
            //    }

            //    services
            //        .AddDbContext<DataContext>(builder => builder
            //        .UseSqlServer(sqlServerConnectionString, sqlOptions =>
            //        {
            //            sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
            //            sqlOptions.EnableRetryOnFailure(2, TimeSpan.FromSeconds(3), null);
            //        }));
            //}

            var postgreServerConnectionString = configuration.GetValue<string>("SqlConnectionString:PostgreSql");
            if (!string.IsNullOrEmpty(postgreServerConnectionString))
            {
                if (postgreServerConnectionString.Contains("password", StringComparison.OrdinalIgnoreCase))
                {
                    var splits = postgreServerConnectionString.Split(';');
                    var pswd = splits.FirstOrDefault(x => x.Contains("password", StringComparison.OrdinalIgnoreCase));
                    var plainPassword = pswd.Split('=')[1];
                    var encptPassword = Encryption.Decrypt(plainPassword);

                    postgreServerConnectionString = postgreServerConnectionString.Replace(plainPassword, encptPassword);
                }

                services
                    .AddDbContext<DataContext>(builder => builder
                    .UseNpgsql(postgreServerConnectionString, sqlOptions =>
                    {
                        sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
                        sqlOptions.EnableRetryOnFailure(2, TimeSpan.FromSeconds(3), null);
                    }));
            }

            //services.AddSingleton<RedisCache>();
        }
    }
}
