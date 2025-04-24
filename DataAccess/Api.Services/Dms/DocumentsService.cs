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
	public class DocumentsService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IDocumentsRepository repository) : BaseService(accessor, languageRepository)
	{
		public async Task<object> GetAll(ReqestFilter request)
		{
			await ValidateInputRequestAsync(request);

			var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			return ObjectFlatter.Flatten(result);
		}

		public async Task<Documents> Get(int Id)
		{
			await ValidateInputAsync(Id);

			await repository.LogTransaction($"Get Documents by Id {Id}", Domain.Attributes.UserAction.Read);

			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<Documents>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);

			await repository.LogTransaction($"Get Documents page {page} limit {limit}", Domain.Attributes.UserAction.Read);

			return await repository.GetAsync(page, limit);
		}

		public async Task<List<Documents>> Upsert(List<RequestDocuments> request)
		{
			await ValidateInputRequestAsync(request);

			var entities = request.CopyProperties<List<Documents>>();
			entities = entities.DistinctBy(x => x.Id).ToList();

			return await repository.UpsertManyAsync(entities);
		}

		public async Task<Documents> Insert(RequestDocuments request)
		{
			await ValidateInputRequestAsync(request);

			await CheckIfExist(request.Id);

			var entity = request.CopyProperties<Documents>();

			await repository.LogTransactionAndAuditTrail($"Insert new Documents", Domain.Attributes.UserAction.Insert, entity);

			return await repository.InsertAsync(entity);
		}

		public async Task<Documents> Update(RequestDocuments request)
		{
			await ValidateInputRequestAsync(request);

			await CheckIfExist(request.Id);

			var entity = request.CopyProperties<Documents>();

			await repository.LogTransactionAndAuditTrail($"Update existing Request Documents with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.UpdateAsync(entity);
		}

		public async Task<Documents> SoftDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);

			var entity = new Documents { Id = id };

			await repository.LogTransactionAndAuditTrail($"Soft delete existing Documents with id {id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.MarkAsDeletedAsync(entity);
		}

		public async Task<Documents> SoftUndelete(int id)
		{
			await ValidateInputAsync(id);

			await CheckIfExist(id);

			var entity = new Documents { Id = id };

			await repository.LogTransactionAndAuditTrail($"Soft undelete existing Documents with id {id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.MarkAsNotDeletedAsync(entity);
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
