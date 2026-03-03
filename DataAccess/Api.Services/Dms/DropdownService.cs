using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Services.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Dms
{
    public class DropdownService : BaseService
    {
        private readonly ILanguageRepository languageRepository;
        private readonly IDropdownRepository repository;
        private readonly IUserRepository repositoryUser;
        private readonly IRoleRepository repositoryRole;
        private readonly ICompanyRepository repositoryCompany;
        private readonly IWatermarksRepository repositoryWatermarks;
		private readonly IGroupRepository repositoryGroup;
		private readonly IDocumentsRepository documentsRepo;
		private readonly DropdownService service;
		private readonly IAttributesRepository attributesRepo;
		private readonly IAttributeCollectionsRepository attributeCollectionsRepo;
		private readonly ICategoryRepository categoryRepository;

		public DropdownService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, 
            IDropdownRepository repository, IUserRepository repositoryUser,
            IRoleRepository repositoryRole, ICompanyRepository repositoryCompany, IWatermarksRepository repositoryWatermarks,
			IGroupRepository repositoryGroup, IDocumentsRepository documentsRepo, IAttributesRepository attributesRepo,
			IAttributeCollectionsRepository attributeCollectionsRepo, ICategoryRepository categoryRepository) : base(accessor, repository: languageRepository)
        {
            this.languageRepository = languageRepository;
            this.repository = repository;
            this.repositoryUser = repositoryUser;
            this.repositoryRole = repositoryRole;
            this.repositoryCompany = repositoryCompany;
            this.repositoryWatermarks = repositoryWatermarks;
			this.repositoryGroup = repositoryGroup;
			this.documentsRepo = documentsRepo;
			this.attributesRepo = attributesRepo;
			this.attributeCollectionsRepo = attributeCollectionsRepo;
			this.categoryRepository = categoryRepository;
		}

        public async Task<List<Dropdown>> GetDropdownSharedPrivillege()
        {
            //await repository.LogTransactionAndAuditTrail($"Get Dropdown Shared Privillege", Domain.Attributes.UserAction.Read);
            return repository.GetDropdownSharedPrivillege();
        }

		public async Task<List<DropdownUsers>> GetDropdownListUsers()
		{
			//await repository.LogTransactionAndAuditTrail($"Get Dropdown List Users", Domain.Attributes.UserAction.Read);
			return await repositoryUser.GetDropdownUsers();
		}

		public async Task<List<DropdownTextValue>> GetDropdownListRoles()
		{
			//await repository.LogTransactionAndAuditTrail($"Get Dropdown List Role", Domain.Attributes.UserAction.Read);
			return await repositoryRole.GetDropdownRoles();
		}

		public async Task<List<DropdownTextValueString>> GetDropdownListCompany()
		{
			//await repository.LogTransactionAndAuditTrail($"Get Dropdown List Company", Domain.Attributes.UserAction.Read);
			return await repositoryCompany.GetDropdownCompany();
		}

		public async Task<List<DropdownTextValue>> GetDropdownListWatermarks()
		{
			//await repository.LogTransactionAndAuditTrail($"Get Dropdown List Watermarks", Domain.Attributes.UserAction.Read);
			return await repositoryWatermarks.GetDropdownWatermarks();
		}

		public async Task<List<DropdownTextValue>> GetDropdownListGroup()
		{
			//await repositoryGroup.LogTransactionAndAuditTrail($"Get Dropdown List Group", Domain.Attributes.UserAction.Read);
			return await repositoryGroup.GetDropdownGroups();
		}		
		
		public async Task<List<DropdownTextValue>> GetDropdownDocument(RequestDocumentsDropdown search)
		{
			//await repositoryGroup.LogTransactionAndAuditTrail($"Get Dropdown List Documents", Domain.Attributes.UserAction.Read);
			return await documentsRepo.GetDropdownDocument(search.DocumentTitle, search.excludedIds);
		}

		public async Task<List<DropdownUsersAndGroups>> GetDropdownListUsersAndGroups()
		{
			return await repositoryUser.GetDropdownUsersAndGroups();
		}

		public async Task<object> GetDropdownAttributes(RequestAttributesDropdown request)
		{
			return await attributesRepo.GetDropdownAttribute(request.search);
		}

		public async Task<object> GetDropdownAttributeCollection(RequestAttributeCollectionsDropdown request)
		{
			return await attributeCollectionsRepo.GetDropdownAttributeCollection(request.search);
		}

		public async Task<List<DropdownTextValue>> GetDropdownCategories(RequestCategoryDropdown request)
		{
			//await repositoryGroup.LogTransactionAndAuditTrail($"Get Dropdown List Group", Domain.Attributes.UserAction.Read);
			string param = string.Empty;
			if(request.search != null)
				param = request.search;

			return await categoryRepository.GetDropdownCategories(param);
		}
	}
}
