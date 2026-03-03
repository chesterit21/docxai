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
	public class ApprovalFlowsService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IApprovalFlowsRepository repository) : BaseService(accessor, languageRepository)
	{
		public async Task<object> GetAll(RequestFilter request)
		{
			await ValidateInputRequestAsync(request);

			var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			return ObjectFlatter.Flatten(result);
		}

		public async Task<ApprovalFlows> Get(int Id)
		{
			await ValidateInputAsync(Id);

			await repository.LogTransaction($"Get ApprovalcFlows by Id {Id}", Domain.Attributes.UserAction.Read);

			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<ApprovalFlows>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);

			await repository.LogTransaction($"Get ApprovalFlows page {page} limit {limit}", Domain.Attributes.UserAction.Read);

			return await repository.GetAsync(page, limit);
		}

		public async Task<List<ApprovalFlows>> Upsert(List<RequestApprovalFlows> request)
		{
			await ValidateInputRequestAsync(request);

			var entities = request.CopyProperties<List<ApprovalFlows>>();
			entities = entities.DistinctBy(x => x.Id).ToList();

			return await repository.UpsertManyAsync(entities);
		}

		public async Task<ApprovalFlows> Insert(RequestApprovalFlows request)
		{
			await ValidateInputRequestAsync(request);

			await CheckIfExist(request.Id);

			var entity = request.CopyProperties<ApprovalFlows>();

			await repository.LogTransactionAndAuditTrail($"Insert new ApprovalFlows", Domain.Attributes.UserAction.Insert, entity);

			return await repository.InsertAsync(entity);
		}

		public async Task<ApprovalFlows> Update(RequestApprovalFlows request)
		{
			await ValidateInputRequestAsync(request);

			await CheckIfExist(request.Id);

			var entity = request.CopyProperties<ApprovalFlows>();

			await repository.LogTransactionAndAuditTrail($"Update existing Request Approval with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.UpdateAsync(entity);
		}

		public async Task<int> Delete(List<int> ids)
		{
			await ValidateInputAsync(ids);
			return await repository.Delete(ids);
		}

		public async Task<ApprovalFlows> SoftDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);

			var entity = new ApprovalFlows { Id = id };

			await repository.LogTransactionAndAuditTrail($"Soft delete existing ApprovalFlows with id {id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.MarkAsDeletedAsync(entity);
		}

		public async Task<ApprovalFlows> SoftUndelete(int id)
		{
			await ValidateInputAsync(id);

			await CheckIfExist(id);

			var entity = new ApprovalFlows { Id = id };

			await repository.LogTransactionAndAuditTrail($"Soft undelete existing Approval Flows with id {id}", Domain.Attributes.UserAction.Update, entity);

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


		//private async Task CheckIfExist(int id, string name)
		//{
		//    var any = await repository.AnyAsync(x => x.Id == id || x.Name == name);
		//    if (any)
		//    {
		//        var message = await GetMessage(LangCodes.Duplicate);
		//        throw new ApiException($"{message}. ApprovalAction Flows ID {id}, Name {name}");
		//    }
		//}
	}
}
