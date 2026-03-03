using Api.Repository;
using Api.Repository.Masters;
using Api.Services.Dms;
using Api.Services.Masters;
using Api.Services.Systems;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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

			// register Waha HttpClient and hosted service
			services.AddHttpClient("Waha", client =>
			{
				var baseUrl = configuration.GetValue<string>("Waha:BaseUrl") ?? configuration.GetValue<string>("Waha:BaseUrl");
				if (!string.IsNullOrEmpty(baseUrl))
					client.BaseAddress = new Uri(baseUrl);
				var apiKey = configuration.GetValue<string>("Waha:ApiKey");
				if (!string.IsNullOrEmpty(apiKey))
					client.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", apiKey);
			});


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

			#region DMS
			services.AddScoped<AttributesService>();
			services.AddScoped<DropdownService>();
			services.AddScoped<CategoriesService>();
			services.AddScoped<CategoriesSharedService>();
			services.AddScoped<AttributeCollectionsService>();
			services.AddScoped<SharedWorkspaceService>();


			services.AddScoped<ApprovalService>();
			services.AddScoped<ApprovalFlowsService>();
			services.AddScoped<ApprovalActivityService>();
			services.AddScoped<DocumentAttributesService>();
			services.AddScoped<DocumentFilesService>();
			services.AddScoped<DocumentSharedPrivillegeService>();
			services.AddScoped<DocumentSharedService>();
			services.AddScoped<DocumentsService>();
			services.AddScoped<WatermarkService>();
            services.AddScoped<DocumentReminderService>();
            services.AddScoped<FavoriteService>();
			services.AddScoped<GroupService>(); 
			services.AddScoped<NotificationService>();
			services.AddSingleton<ITokenBlacklistService, InMemoryTokenBlacklistService>();
			services.AddScoped<PDFViewerService>();
			services.AddScoped<DocumentProcessService>();
			services.AddScoped<DocumentEditorService>();
			services.AddScoped<Ej2SpreedsheetService>();
			services.AddScoped<DashboardService>();
			#endregion

			//services.AddSingleton<DynamicDbProvider>();

			services.AddHostedService<EmailBackgroundWorker>();
			services.AddHostedService<PurgingBackgroundWorker>();
			services.AddHostedService<NotificationBackgroundWorker>();
			//services.AddHostedService<ReminderbackgroundWorker>();
			
			services.AddSingleton<IDatabaseGuard, DatabaseGuard>();
			services.AddHostedService<MigrationQueueHostedService>();
			services.AddTransient<MigrationService>();
			services.AddSingleton<IBackgroundTaskQueue, Api.Services.Systems.BackgroundTaskQueue>();
			
			//services.AddHostedService<MigrationQueueHostedService>();
			//services.AddSemanticKernelDependencies(configuration);

		}

		private static IServiceCollection AddSemanticKernelDependencies(this IServiceCollection services, IConfiguration configuration)
		{
			// 1. Dapatkan konfigurasi LLM dari appsettings.json
			var provider = configuration.GetValue<string>("LLM:Provider");

			// 2. Buat builder untuk Kernel
			var builder = Kernel.CreateBuilder();

			// 3. Tambahkan konektor sesuai provider
			if (provider?.ToLower() == "azure")
			{
				var azureConfig = configuration.GetSection("LLM:AzureOpenAI");
				configuration.GetValue<string>("SqlConnectionString:PostgreSql");
				builder.AddAzureOpenAITextEmbeddingGeneration(
					deploymentName: azureConfig.GetValue<string>("DeploymentName")!,
					endpoint: azureConfig.GetValue<string>("Endpoint")!,
					apiKey: azureConfig.GetValue<string>("ApiKey")!
				);
			}
			else
			{
				var openAIApiKey = configuration.GetValue<string>("LLM:OpenAIApiKey");
				if (string.IsNullOrEmpty(openAIApiKey))
				{
					throw new InvalidOperationException("API Key OpenAI tidak ditemukan.");
				}
				builder.AddOpenAITextEmbeddingGeneration(
					modelId: "text-embedding-ada-002",
					apiKey: openAIApiKey
				);
			}

			// 4. Bangun Kernel dan daftarkan sebagai Singleton
			// Ini adalah langkah kunci untuk membuat instance IKernel
			var kernel = builder.Build();
			services.AddSingleton<Kernel>(kernel);

			return services;
		}
	}
}
