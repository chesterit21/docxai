using Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using System;
using static Api.Extensions.AppSettings;

namespace Api.DataAccess
{
	public static class DI
	{
		public static void RegisterDataAccess(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<DataContext>((serviceProvider, builder) =>
			{
				var currentConfig = serviceProvider.GetRequiredService<IConfiguration>();
				var settings = currentConfig.GetSection("ConnectionString").Get<ConnectionStringProperty>();

				if (settings == null || string.IsNullOrEmpty(settings.Host))
				{
					//throw new InvalidOperationException("Database Host is not configured yet.");
				}

				settings.Password = string.IsNullOrEmpty(settings.Password)
					? ""
					: settings.Password.Decrypt();

				var dynamicConnectionString = settings.ToString();
				builder.UseNpgsql(dynamicConnectionString, sqlOptions =>
				{
					sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
					sqlOptions.EnableRetryOnFailure();
				});
			}, ServiceLifetime.Scoped);

			//var postgreServerConnectionString = "";
			////var setting = AppSettings.Read();
			////var connstring = setting.ConnectionString;
			////connstring.Password = setting.ConnectionString.Password.Decrypt();
			////postgreServerConnectionString = connstring.ToString();
			//var settings = configuration.GetSection("ConnectionString").Get<ConnectionStringProperty>();
			//settings.Password = string.IsNullOrEmpty(settings.Password)
			//	? ""
			//	: settings.Password.Decrypt();
			//postgreServerConnectionString = settings.ToString();
			////if (!string.IsNullOrEmpty(postgreServerConnectionString))
			////{

			//services
			//		.AddDbContext<DataContext>(builder => builder
			//		.UseNpgsql(postgreServerConnectionString, sqlOptions =>
			//		{
			//			sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
			//			sqlOptions.EnableRetryOnFailure();
			//			//sqlOptions.EnableRetryOnFailure(2, TimeSpan.FromSeconds(3), null);
			//		}), ServiceLifetime.Scoped);
			////}
			////else
			////{
			////throw new InvalidOperationException("Database connection string is missing from appsettings.json.");
			////}

			////services.AddSingleton<RedisCache>();
		}
	}
}
