using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Masters
{
    public class CompanyService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, ICompanyRepository repository) : BaseService(accessor, languageRepository)
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

			//var result = await repository.GetAsync(x => x.IsActive == true, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            //return ObjectFlatter.Flatten(result);
        }

        public async Task<Company> Get(string companyId)
        {
             await ValidateInputAsync(companyId);

            await repository.LogTransaction($"Get company by CompanyId {companyId}", Domain.Attributes.UserAction.Read);

            return await repository.GetSingleAsync(x => x.CompanyId == companyId && x.IsActive == true);
        }

        public async Task<List<Company>> GetAll(int page, int limit)
        {
            await ValidateInputAsync([page, limit]);

            await repository.LogTransaction($"Get company page {page} limit {limit}", Domain.Attributes.UserAction.Read);

            return await repository.GetAsync(page, limit);
        }

        public async Task<List<Company>> Upsert(List<RequestCompany> request)
        {
            await ValidateInputRequestAsync(request);

            var entities = request.CopyProperties<List<Company>>();
            entities = entities.DistinctBy(x => x.CompanyId).ToList();

            return await repository.UpsertManyAsync(entities);
        }

        public async Task<Company> Insert(RequestCompany request)
        {
            await ValidateInputRequestAsync(request);

            await CheckIfExist(request.CompanyId, request.Name);

            var entity = request.CopyProperties<Company>();

            await repository.LogTransactionAndAuditTrail($"Insert new company", Domain.Attributes.UserAction.Insert, entity);

            return await repository.InsertAsync(entity);
        }

        public async Task<Company> Update(RequestCompany request)
        {
            await ValidateInputRequestAsync(request);

            //await CheckIfExist(request.CompanyId, request.Name);

            var entity = request.CopyProperties<Company>();
            
            await repository.LogTransactionAndAuditTrail($"Update existing company with id {entity.CompanyId}", Domain.Attributes.UserAction.Update, entity);
            
            return await repository.UpdateAsync(entity);
        }

        public async Task<int> Delete(List<string> companyIds)
        {
            await ValidateInputAsync(companyIds);
            return await repository.Delete(companyIds);
        }

        public async Task<Company> SoftDelete(string id)
        {
            await ValidateInputAsync(id);
            await CheckIfExist(id);

            var entity = new Company { CompanyId = id };
            
            await repository.LogTransactionAndAuditTrail($"Soft delete existing company with id {id}", Domain.Attributes.UserAction.Update, entity);

            return await repository.MarkAsDeletedAsync(entity);
        }

        public async Task<Company> SoftUndelete(string id)
        {
            await ValidateInputAsync(id);

            await CheckIfExist(id);

            var entity = new Company { CompanyId = id };
            
            await repository.LogTransactionAndAuditTrail($"Soft undelete existing company with id {id}", Domain.Attributes.UserAction.Update, entity);

            return await repository.MarkAsNotDeletedAsync(entity);
        }

        private async Task CheckIfExist(string id)
        {
            var any = await repository.AnyAsync(x => x.CompanyId == id);
            if (!any)
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. Company ID {id}");
            }
        }

        private async Task CheckIfExist(string id, string name)
        {
            var any = await repository.AnyAsync(x => x.CompanyId == id || x.Name == name);
            if (any)
            {
                var message = await GetMessage(LangCodes.Duplicate);
                throw new ApiException($"{message}. Company ID {id}, Name {name}");
            }
        }
    }
}
