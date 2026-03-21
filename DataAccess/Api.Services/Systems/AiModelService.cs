using Api.DataAccess.Models.Systems;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Systems;
using Api.Domain.EntityResponses;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Systems
{
    public class AiModelService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IAiModelRepository repository) : BaseService(accessor, languageRepository)
    {
        public async Task<object> GetAll(RequestFilter request)
        {
            await ValidateInputRequestAsync(request);

            var totalRecords = await repository.CountAsync(x => x.IsActive == true);
            var records = await repository.GetAsync(x => x.IsActive == true, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);

            return new ResponsePagination
            {
                TotalRecords = totalRecords,
                TotalPages = GetTotalPages(totalRecords, request.Limit),
                Data = records
            };
        }

        public async Task<AiModel> Get(Guid id)
        {
            await ValidateInputAsync(id);

            await repository.LogTransaction($"Get AI model by Id {id}", Domain.Attributes.UserAction.Read);

            return await repository.GetSingleAsync(x => x.Id == id && x.IsActive == true);
        }

        public async Task<AiModel> Insert(RequestAiModel request)
        {
            await ValidateInputRequestAsync(request);

            await CheckIfExist(request.ModelName);

            var entity = request.CopyProperties<AiModel>();

            await repository.LogTransactionAndAuditTrail($"Insert new AI model", Domain.Attributes.UserAction.Insert, entity);

            return await repository.InsertAsync(entity);
        }

        public async Task<AiModel> Update(RequestAiModel request)
        {
            await ValidateInputRequestAsync(request);

            var entity = request.CopyProperties<AiModel>();
            
            await repository.LogTransactionAndAuditTrail($"Update existing AI model with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);
            
            return await repository.UpdateAsync(entity);
        }

        public async Task<AiModel> SoftDelete(Guid id)
        {
            await ValidateInputAsync(id);
            await CheckIfExist(id);

            var entity = new AiModel { Id = id };
            
            await repository.LogTransactionAndAuditTrail($"Soft delete existing AI model with id {id}", Domain.Attributes.UserAction.Update, entity);

            return await repository.MarkAsDeletedAsync(entity);
        }

        private async Task CheckIfExist(Guid id)
        {
            var any = await repository.AnyAsync(x => x.Id == id);
            if (!any)
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. AI Model ID {id}");
            }
        }

        private async Task CheckIfExist(string modelName)
        {
            var any = await repository.AnyAsync(x => x.ModelName == modelName && x.IsActive);
            if (any)
            {
                var message = await GetMessage(LangCodes.Duplicate);
                throw new ApiException($"{message}. AI Model Name {modelName}");
            }
        }
    }
}
