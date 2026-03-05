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
    public class AttributtesService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IAttributtesRepository repository) : BaseService(accessor, languageRepository)
    {
        public async Task<object> GetAll(RequestFilter request)
        {
            await ValidateInputRequestAsync(request);

            var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            return ObjectFlatter.Flatten(result);
        }

        public async Task<Attributtes> Get(int Id)
        {
            await ValidateInputAsync(Id);

            await repository.LogTransaction($"Get Attributtes by Id {Id}", Domain.Attributes.UserAction.Read);

            return await repository.GetSingleAsync(x => x.Id == Id);
        }

        public async Task<List<Attributtes>> GetAll(int page, int limit)
        {
            await ValidateInputAsync([page, limit]);

            await repository.LogTransaction($"Get Attributtes page {page} limit {limit}", Domain.Attributes.UserAction.Read);

            return await repository.GetAsync(page, limit);
        }

        public async Task<List<Attributtes>> Upsert(List<RequestAttributtes> request)
        {
            await ValidateInputRequestAsync(request);

            var entities = request.CopyProperties<List<Attributtes>>();
            entities = entities.DistinctBy(x => x.Id).ToList();

            return await repository.UpsertManyAsync(entities);
        }

        public async Task<Attributtes> Insert(RequestAttributtes request)
        {
            await ValidateInputRequestAsync(request);

            await CheckIfExist(request.Id);

            var entity = request.CopyProperties<Attributtes>();

            await repository.LogTransactionAndAuditTrail($"Insert new Attributtes", Domain.Attributes.UserAction.Insert, entity);

            return await repository.InsertAsync(entity);
        }

        public async Task<Attributtes> Update(RequestAttributtes request)
        {
            await ValidateInputRequestAsync(request);

            await CheckIfExist(request.Id);

            var entity = request.CopyProperties<Attributtes>();

            await repository.LogTransactionAndAuditTrail($"Update existing Request Attributtes with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);

            return await repository.UpdateAsync(entity);
        }

        public async Task<Attributtes> SoftDelete(int id)
        {
            await ValidateInputAsync(id);
            await CheckIfExist(id);

            var entity = new Attributtes { Id = id };

            await repository.LogTransactionAndAuditTrail($"Soft delete existing Attributtes with id {id}", Domain.Attributes.UserAction.Update, entity);

            return await repository.MarkAsDeletedAsync(entity);
        }

        public async Task<Attributtes> SoftUndelete(int id)
        {
            await ValidateInputAsync(id);

            await CheckIfExist(id);

            var entity = new Attributtes { Id = id };

            await repository.LogTransactionAndAuditTrail($"Soft undelete existing Attributtes with id {id}", Domain.Attributes.UserAction.Update, entity);

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
