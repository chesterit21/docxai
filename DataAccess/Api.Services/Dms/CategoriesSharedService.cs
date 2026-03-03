using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Repository.Dms;
using Api.Services.Masters;
using Microsoft.AspNetCore.Http;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Enum;

namespace Api.Services.Dms
{
	public class CategoriesSharedService : BaseService
	{
		private readonly ILanguageRepository languageRepository;
		private readonly ICategoriesSharedRepository repository;
		private readonly CategoriesSharedService service;

		public CategoriesSharedService(IHttpContextAccessor accessor, ILanguageRepository languageRepository,
			ICategoriesSharedRepository repository) : base(accessor, repository: languageRepository)
		{
			this.languageRepository = languageRepository;
			this.repository = repository;
		}

		public async Task<bool> SubmitShare(RequestCreateCategoriesShared request)
		{
			//var entity = request.CopyProperties<CategoriesShared>();
			await ValidateInputRequestAsync(request);
			await repository.LogTransactionAndAuditTrail($"Insert new Categories Shared", Domain.Attributes.UserAction.Insert, new CategoriesShared());
			return await repository.InsertCategoriesShared(request);
		}

		public async Task<List<ResponseCategoryShared>> GetListSharedUsers(int categoryId)
		{
			await ValidateInputAsync(categoryId);
			var documentShared = await repository.GetSharedUsersByCategoryIDAsync(categoryId);
			return documentShared;
		}

		public async Task<int> DeleteSharedCategory(int categoryId)
		{
			await ValidateInputAsync(categoryId);
			int result = await repository.DeleteSharedCategory(categoryId);
			return result;
		}
	}
}
