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
	public class CategoriesSharedService : BaseService
	{
		private readonly ILanguageRepository languageRepository;
		private readonly ICategoriesSharedRepository repository;
		private readonly ICategoriesSharedPrivillegeRepository repositoryPriv;
		private readonly CategoriesSharedService service;

		public CategoriesSharedService(IHttpContextAccessor accessor, ILanguageRepository languageRepository,
			ICategoriesSharedRepository repository, ICategoriesSharedPrivillegeRepository repositoryPriv) : base(accessor, repository: languageRepository)
		{
			this.languageRepository = languageRepository;
			this.repository = repository;
			this.repositoryPriv = repositoryPriv;
		}

		public async Task<CategoriesShared> SubmitShare(RequestCategoriesShared request)
		{
			await repository.LogTransactionAndAuditTrail($"Insert new Categories Shared", Domain.Attributes.UserAction.Insert);
			return await repository.InsertCategoriesShared(request);
		}
	}
}
