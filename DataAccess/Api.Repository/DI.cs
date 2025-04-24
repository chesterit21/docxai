using Api.DataAccess;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Repository
{
    public static class DI
    {
        public static void RegisterRepository(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterDataAccess(configuration);

            //REPOSITORIES
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IRoleMatrixRepository, RoleMatrixRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserMatrixRepository, UserMatrixRepository>();

            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUserCompanyRepository, UserCompanyRepository>();

            services.AddScoped<IApplicationLogRepository, ApplicationLogRepository>();
            services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();
            services.AddScoped<ITransactionLogRepository, TransactionLogRepository>();
            //services.AddScoped<IHistoryAuditTrailRepository, HistoryAuditTrailRepository>();
            //services.AddScoped<IHistoryTransactionLogRepository, HistoryTransactionLogRepository>();

            services.AddScoped<IEmailRepository, EmailRepository>();
            //services.AddScoped<IHistoryEmailRepository, HistoryEmailRepository>();

            //services.AddScoped<Api.Repository.DatabaseLogger>();
        }
    }
}
