using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Dms;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Masters
{
	public class ApprovalService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IApprovalRepository repository) : BaseService(accessor, languageRepository)
	{
		public async Task<object> GetAll(RequestFilter request)
		{
			await ValidateInputRequestAsync(request);

			//var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			//return ObjectFlatter.Flatten(result);
			var totalRecords = await repository.CountAsync();
			var records = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = records
			};

		}

		public async Task<Approvals> Get(int Id)
		{
			await ValidateInputAsync(Id);
			await repository.LogTransaction($"Get Approval by Id {Id}", Domain.Attributes.UserAction.Read);
			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<Approvals>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);

			await repository.LogTransaction($"Get Approval page {page} limit {limit}", Domain.Attributes.UserAction.Read);

			return await repository.GetAsync(page, limit);
		}

		//public async Task<Approvals> Insert(RequestActionApproval request)
		//{
		//	await ValidateInputRequestAsync(request);

		//	await CheckIfExist(request.Id);

		//	var entity = request.CopyProperties<Approvals>();

		//	await repository.LogTransactionAndAuditTrail($"Insert new Approval", Domain.Attributes.UserAction.Insert, entity);

		//	return await repository.InsertAsync(entity);
		//}

		//public async Task<Approvals> UpdateDocument(RequestActionApproval request)
		//{
		//	await ValidateInputRequestAsync(request);

		//	await CheckIfExist(request.Id);

		//	var entity = request.CopyProperties<Approvals>();

		//	await repository.LogTransactionAndAuditTrail($"UpdateDocument existing Request Approval with id {entity.Id}", Domain.Attributes.UserAction.UpdateDocument, entity);

		//	return await repository.UpdateAsync(entity);
		//}

		private async Task CheckIfExist(int id)
		{
			var any = await repository.AnyAsync(x => x.Id == id);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Id {id}");
			}
		}

        public async Task<object> GetMyApprovalTask(RequestPagination request)
        {
            var result = await repository.GetMyApprovalTask(request.Page, request.Limit);
			return new ResponsePagination
			{
				TotalRecords = result.PageCount,
				TotalPages = GetTotalPages(result.PageCount, request.Limit),
				Data = result.ResponseApprovalTasks
			};
			//return ObjectFlatter.Flatten(result);
        }

        public async Task<object> GetMyApprovalRequest(RequestPagination request)
        {
            var result = await repository.GetMyApprovalRequest(request.Page, request.Limit);
			return new ResponsePagination
			{
				TotalRecords = result.PageCount,
				TotalPages = GetTotalPages(result.PageCount, request.Limit),
				Data = result.ResponseApprovalRequests
			};
			//return ObjectFlatter.Flatten(result);
        }

		public async Task<ResponseCheckApproval> GetInfoCheckForApprovalPageView(int approvalId)
		{
			await ValidateInputAsync(approvalId);
			await CheckIfExist(approvalId);

			return await repository.GetInfoCheckForApprovalPageView(approvalId);
		}


		//public async Task<int> Approve(int ApprovalId, RequestApprovalActivities activity)
		public async Task<int> Approve(int ApprovalId, RequestApprovalActivities activity)
		{
			await ValidateInputAsync(ApprovalId);
			await ValidateInputRequestAsync(activity);

			var entities = activity.CopyProperties<ApprovalActivities>();
			entities.RelatedDocumentID = activity.RelatedDocumentID;

			return await repository.ApprovalAction(ApprovalId, entities);
		}

		public async Task<int> Reject(int ApprovalId, RequestApprovalActivities activity)
		{
			await ValidateInputAsync(ApprovalId);
			await ValidateInputRequestAsync(activity);

			if (string.IsNullOrEmpty(activity.Reason))
			{
				var message = await GetMessage(LangCodes.InputEmpty);
				throw new ApiException(message);
			}

			var entities = activity.CopyProperties<ApprovalActivities>();
			return await repository.ApprovalAction(ApprovalId, entities);
		}
	}
}
