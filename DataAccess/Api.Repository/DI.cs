using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.Repository.Masters;
using Api.Repository.Dms;
using Api.Repository.Systems;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            //DMS
            services.AddScoped<IDropdownRepository, DropdownRepository>();
            services.AddScoped<ICategoriesSharedRepository, CategoriesSharedRepository>();
            services.AddScoped<IAttributesRepository, AttributesRepository>();
            services.AddScoped<IAttributeCollectionsRepository, AttributeCollectionsRepository>();
            services.AddScoped<ISharedWorkspaceRepository, SharedWorkspaceRepository>();
            services.AddScoped<IWatermarksRepository, WatermarksRepository>();
            services.AddScoped<IApprovalFlowsRepository, ApprovalFlowsRepository>();
            services.AddScoped<IApprovalRepository, ApprovalRepository>();
            services.AddScoped<IApprovalActivityRepository, ApprovalActivityRepository>();
            //services.AddScoped<ICategoriesSharedPrivillegeRepository, CategoriesSharedPrivillegeRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IDocumentAttributesRepository, DocumentAttributesRepository>();
            services.AddScoped<IDocumentFilesRepository, DocumentFilesRepository>();
            services.AddScoped<IDocumentSharedPrivillegeRepository, DocumentSharedPrivillegeRepository>();
            services.AddScoped<IDocumentSharedRepository, DocumentSharedRepository>();
            services.AddScoped<IDocumentsRepository, DocumentsRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ISharedWorkspaceRepository, SharedWorkspaceRepository>();
            services.AddScoped<IDocumentReminderRepository, DocumentReminderRepository>();
            services.AddScoped<IDocumentFavoriteRepository, DocumentFavoriteRepository>();
            services.AddScoped<ICategoriesFavoriteRepository, CategoriesFavoriteRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IUserGroupRepository, UserGroupRepository>();
            services.AddScoped<IDocumentsItemlistRepository, DocumentsItemlistRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IMigrationJobRepository, MigrationJobRepository>();

            services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
            services.AddScoped<IDocumentTypeAttributesRepository, DocumentTypeAttributesRepository>();
            services.AddScoped<IAttributeSynonymsRepository, AttributeSynonymsRepository>();
            services.AddScoped<IAgentPollingTaskDocumentRepository, AgentPollingTaskDocumentRepository>();
            services.AddScoped<IDocumentExtractedEntitiesRepository, DocumentExtractedEntitiesRepository>();
            services.AddScoped<IAiModelRepository, AiModelRepository>();

        }
    }
}
