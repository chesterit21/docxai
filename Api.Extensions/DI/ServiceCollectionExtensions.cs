using Microsoft.Extensions.DependencyInjection;
using Api.Extensions.Services;

namespace Api.Extensions.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLicenseManagement(this IServiceCollection services)
        {
            services.AddSingleton<ILicenseManager, LicenseManager>();
            return services;
        }
    }
}