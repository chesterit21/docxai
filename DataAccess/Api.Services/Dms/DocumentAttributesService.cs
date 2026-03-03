using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Masters
{
	public class DocumentAttributesService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IDocumentAttributesRepository repository) : BaseService(accessor, languageRepository)
	{
		public async Task<object> GetAll(RequestFilter request)
		{
			await ValidateInputRequestAsync(request);

			var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			return ObjectFlatter.Flatten(result);
		}

		public async Task<DocumentAttributes> Get(int Id)
		{
			await ValidateInputAsync(Id);

			await repository.LogTransaction($"Get Document Attributes by Id {Id}", Domain.Attributes.UserAction.Read);

			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<DocumentAttributes>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);

			await repository.LogTransaction($"Get Document Attributes page {page} limit {limit}", Domain.Attributes.UserAction.Read);

			return await repository.GetAsync(page, limit);
		}

		public async Task<List<DocumentAttributes>> Upsert(List<RequestDocumentAttributes> request)
		{
			await ValidateInputRequestAsync(request);

			var entities = request.CopyProperties<List<DocumentAttributes>>();
			entities = entities.DistinctBy(x => x.Id).ToList();

			return await repository.UpsertManyAsync(entities);
		}

		public async Task<DocumentAttributes> Insert(RequestDocumentAttributes request)
		{
			await ValidateInputRequestAsync(request);

			var entity = request.CopyProperties<DocumentAttributes>();

			await repository.LogTransactionAndAuditTrail($"Insert new Document Attributes", Domain.Attributes.UserAction.Insert, entity);

			return await repository.InsertAsync(entity);
		}

		public async Task<DocumentAttributes> Update(RequestUpdateDocumentAttributes request)
		{
			await ValidateInputRequestAsync(request);

			await CheckIfExist(request.Id);

			var entity = request.CopyProperties<DocumentAttributes>();

			await repository.LogTransactionAndAuditTrail($"Update existing Request Document Attributes with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.UpdateAsync(entity);
		}

		public async Task<int> SaveDocumentAttribute(RequestUpdateDocumentAttributes request)
		{
			await ValidateInputRequestAsync(request);

			//await CheckIfExist(request.Id);

			var entity = request.CopyProperties<DocumentAttributes>();

			await repository.LogTransactionAndAuditTrail($"Update existing Request Document Attributes with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.SaveDocumentAttribute(entity);
		}

		private async Task CheckIfExist(int id)
		{
			var any = await repository.AnyAsync(x => x.Id == id);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Id {id}");
			}
		}
	}
}
