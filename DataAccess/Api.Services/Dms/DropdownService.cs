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
        private readonly DropdownService service;

        public DropdownService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, 
            IDropdownRepository repository, IUserRepository repositoryUser) : base(accessor, repository: languageRepository)
        {
            this.languageRepository = languageRepository;
            this.repository = repository;
            this.repositoryUser = repositoryUser;
        }

        public async Task<List<Dropdown>> GetDropdownSharedPrivillege()
        {
            await repository.LogTransactionAndAuditTrail($"Get Dropdown Dropdown Shared Privillege", Domain.Attributes.UserAction.Read);
            return repository.GetDropdownSharedPrivillege();
        }

		public async Task<List<DropdownUsers>> GetDropdownListUsers()
		{
			await repository.LogTransactionAndAuditTrail($"Get Dropdown List Users", Domain.Attributes.UserAction.Read);
			return await repositoryUser.GetDropdownUsers();
		}
	}
}
