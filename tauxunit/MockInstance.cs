using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text.Json;
using Api.DataAccess;
using Api.Extensions;
using SixLabors.ImageSharp;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Api.Services.Masters;
using Api.Services.Systems;
using Api.Services.Dms;
using Api.Extensions.Services;

namespace tauxunit
{
    internal class MockInstance
    {
        internal static IHttpContextAccessor GetIHttpContextAccessor()
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
            return mockHttpContextAccessor.Object;
        }

        internal static DataContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "shuba")
            .Options;

            var accessor = GetIHttpContextAccessor();
            return new DataContext(options, accessor);
        }

        internal static DataContext GetSqlDbContext()
        {
            var path = "..\\..\\..\\..\\..\\Docubase\\Api\\appsettings.json";
            var fullPath = Path.GetFullPath(path);
            //bool exist = File.Exists(fullPath);
            JsonElement doc = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(fullPath));
            var postgreServerConnectionString = doc.GetElement("SqlConnectionString.PostgreSql").GetString();
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
            }

            var options = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(postgreServerConnectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds);
                sqlOptions.EnableRetryOnFailure(2, TimeSpan.FromSeconds(3), null);
            })
            .Options;

            var accessor = GetIHttpContextAccessor();
            return new DataContext(options, accessor);
        }

        internal static InstanceContext GetRepositoryInstances(DataContext dbcontext)
        {
            var accessor = GetIHttpContextAccessor();

            Mock<IConfiguration> config = new Mock<IConfiguration>();

            var iCompanyRepository = new CompanyRepository(dbcontext, accessor);
            var iEmailRepository = new EmailRepository(dbcontext, accessor);
            var iMenuRepository = new MenuRepository(dbcontext, accessor);
            var iRoleMatrixRepository = new RoleMatrixRepository(dbcontext, accessor);
            var iRoleRepository = new RoleRepository(dbcontext, accessor);
            var iUserCompanyRepository = new UserCompanyRepository(dbcontext, accessor);
            var iUserMatrixRepository = new UserMatrixRepository(dbcontext, accessor);
            var iUserRepository = new UserRepository(dbcontext, accessor);
            var iUserRoleRepository = new UserRoleRepository(dbcontext, accessor);
            var iLanguageRepository = new LanguageRepository(dbcontext, accessor);
            var iApprovalRepository = new ApprovalRepository(dbcontext, accessor);
            var iGroupREpository = new GroupRepository(dbcontext, accessor, iUserRepository);
            var iUserGroupRepository = new UserGroupRepository(dbcontext, accessor);			
			//c, IUserRoleRepository userRoleRepository, IRoleMatrixRepository roleMatrixRepo
			var userMatrixService = new UserMatrixService(accessor, iUserRepository, iMenuRepository, iUserRoleRepository, iUserMatrixRepository, iRoleMatrixRepository, iLanguageRepository);
            var iCategoriyRepository = new CategoryRepository(dbcontext, accessor);
			var licenseManager = new LicenseManager(config.Object);
			var iMigrationJobRepository = new MigrationJobRepository(dbcontext, accessor);

			return new InstanceContext
            {
                DataContext = dbcontext,
                ICompanyRepository = iCompanyRepository,
                IEmailRepository = iEmailRepository,
                IMenuRepository = iMenuRepository,
                IRoleMatrixRepository = iRoleMatrixRepository,
                IRoleRepository = iRoleRepository,
                IUserCompanyRepository = iUserCompanyRepository,
                IUserMatrixRepository = iUserMatrixRepository,
                IUserRepository = iUserRepository,
                IUserRoleRepository = iUserRoleRepository,
				IApprovalRepository = iApprovalRepository,

				CompanyService = new CompanyService(accessor, iLanguageRepository, iCompanyRepository),
                LanguageService = new LanguageService(accessor, iLanguageRepository),
                //MenuService = new MenuService(accessor, iLanguageRepository, iMenuRepository, userMatrixService),
                EmailService = new EmailService(accessor, iLanguageRepository, iEmailRepository),
                RoleMatrixService = new RoleMatrixService(accessor, iLanguageRepository, iRoleMatrixRepository, iMenuRepository, iRoleRepository),
                RoleService = new RoleService(accessor, iLanguageRepository, iRoleRepository),
                UserCompanyService = new UserCompanyService(accessor, iLanguageRepository, iUserCompanyRepository),
                UserMatrixService = new UserMatrixService(accessor, iUserRepository, iMenuRepository, iUserRoleRepository, iUserMatrixRepository, iRoleMatrixRepository, iLanguageRepository),
                UserRoleService = new UserRoleService(accessor, iLanguageRepository, iRoleRepository, iUserRoleRepository),
                UserService = new UserService(accessor, iLanguageRepository, iCompanyRepository, iUserRepository, iRoleRepository, iUserRoleRepository, iUserCompanyRepository, iGroupREpository, iUserGroupRepository, iEmailRepository, licenseManager),
                SettingService = new SettingService(accessor, config.Object, new LanguageService(accessor, iLanguageRepository), iUserRepository, iMigrationJobRepository),
                CategoryService = new CategoriesService(accessor,iLanguageRepository,iCategoriyRepository, iApprovalRepository)
            };
        }

        internal class InstanceContext
        {
            internal DataContext DataContext { get; set; }
            internal ICompanyRepository ICompanyRepository { get; set; }
            internal IMenuRepository IMenuRepository { get; set; }
            internal IRoleMatrixRepository IRoleMatrixRepository { get; set; }
            internal IRoleRepository IRoleRepository { get; set; }
            internal IUserMatrixRepository IUserMatrixRepository { get; set; }
            internal IUserRepository IUserRepository { get; set; }
            internal IUserRoleRepository IUserRoleRepository { get; set; }
            internal IUserCompanyRepository IUserCompanyRepository { get; set; }
            internal IEmailRepository IEmailRepository { get; set; }
            internal ICategoryRepository ICategoryRepository { get; set; }
            internal IApprovalRepository IApprovalRepository { get; set; }
			internal IGroupRepository IGroupRepository { get; set; }
			internal IUserGroupRepository IUserGroupRepository { get; set; }

			internal CompanyService CompanyService { get; set; }
            internal LanguageService LanguageService { get; set; }
            internal MenuService MenuService { get; set; }
            internal RoleMatrixService RoleMatrixService { get; set; }
            internal RoleService RoleService { get; set; }
            internal UserMatrixService UserMatrixService { get; set; }
            internal UserService UserService { get; set; }
            internal UserCompanyService UserCompanyService { get; set; }
            internal UserRoleService UserRoleService { get; set; }
            internal EmailService EmailService { get; set; }
            internal SettingService SettingService { get; set; }
            internal CategoriesService CategoryService { get; set; }
        }
    }
}
