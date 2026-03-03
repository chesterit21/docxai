using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses.Dms;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.Index.HPRtree;

namespace Api.Services.Masters
{
	public class DocumentSharedService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IDocumentSharedRepository repository) : BaseService(accessor, languageRepository)
	{
		public async Task<object> GetAll(RequestFilter request)
		{
			await ValidateInputRequestAsync(request);

			var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			return ObjectFlatter.Flatten(result);
		}

		public async Task<DocumentShared> Get(int Id)
		{
			await ValidateInputAsync(Id);

			await repository.LogTransaction($"Get Document Shared by Id {Id}", Domain.Attributes.UserAction.Read);

			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<DocumentShared>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);

			await repository.LogTransaction($"Get Document Shared page {page} limit {limit}", Domain.Attributes.UserAction.Read);

			return await repository.GetAsync(page, limit);
		}


		public async Task<DocumentShared> Insert(RequestDocumentShared request)
		{
			await ValidateInputRequestAsync(request);

			await CheckIfExist(request.DocumentID);

			var entity = request.CopyProperties<DocumentShared>();

			await repository.LogTransactionAndAuditTrail($"Insert new Document Shared", Domain.Attributes.UserAction.Insert, entity);

			return await repository.InsertAsync(entity);
		}

		public async Task<DocumentShared> Update(RequestDocumentShared request)
		{
			await ValidateInputRequestAsync(request);

			await CheckIfExist(request.DocumentID);

			var entity = request.CopyProperties<DocumentShared>();

			await repository.LogTransactionAndAuditTrail($"Update existing Request Document Shared with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.UpdateAsync(entity);
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

        public async Task<int> InsertDocumentShare(RequestDocumentShared request)
        {
            await ValidateInputRequestAsync(request);
            await CheckIfExist(request.DocumentID);
            //await repository.LogTransactionAndAuditTrail($"Insert new Document Shared", Domain.Attributes.UserAction.Insert, entity);
            return await repository.UpsertDocumentSharePrivillege(request);
        }

		public async Task<List<ResponseUserPrivillege>> GetListSharedByDocId(int Documentid)
		{
			await ValidateInputAsync(Documentid);
			//await repository.LogTransactionAndAuditTrail($"Insert new Document Shared", Domain.Attributes.UserAction.Insert, entity);
			var documentShared = await repository.GetSharedUsersByDocumentIDAsync(Documentid);
			return documentShared.SharedUser;
		}
	}
}
