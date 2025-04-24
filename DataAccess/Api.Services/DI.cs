using Api.Repository;
using Api.Services.Masters;
using Api.Services.Systems;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Api.Services
{
    public static class DI
    {
        public static void RegisterApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            #region OTHER PROJECTS
            services.RegisterRepository(configuration);
            //services.RegisterCqrs(configuration);
            //services.RegisterUoW(configuration);
            #endregion

            services.AddScoped<CompanyService>();
            services.AddScoped<MenuService>();
            services.AddScoped<UserMatrixService>();
            services.AddScoped<UserCompanyService>();
            services.AddScoped<UserService>();
            services.AddScoped<EmailService>();
            services.AddScoped<FileUploadService>();
            services.AddScoped<LanguageService>();
            services.AddScoped<SettingService>();
            services.AddScoped<RoleService>();
            services.AddScoped<RoleMatrixService>();
            services.AddScoped<UserRoleService>();


            services.AddScoped<LogApplicationService>();
            services.AddScoped<LogTransactionService>();
            services.AddScoped<LogAuditTrailService>();

            //services.AddHostedService<EmailBackgroundWorker>();
            //services.AddHostedService<PurgingBackgroundWorker>();
            
        }
    }
}
