using Api.DataAccess.Models.Masters;
using Api.Domain.EntityRequests;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Masters
{
    public class UserCompanyService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IUserCompanyRepository repository) : BaseService(accessor, languageRepository)
    {
        public async Task<object> GetAll(RequestFilter request)
        {
            await ValidateInputRequestAsync(request);
            var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            return ObjectFlatter.Flatten(result);
        }

        public async Task<List<string>> GetCompanyIdsByUserId(int userId)
        {
            await ValidateInputAsync(userId);
            return await repository.GetCompanyIdsByUserId(userId);
        }

        public async Task<List<Company>> GetCompaniesByUserId(int userId)
        {
            await ValidateInputAsync(userId);
            await repository.LogTransaction($"Get company by user ID {userId}", Domain.Attributes.UserAction.Read);
            return await repository.GetCompaniesByUserId(userId);
        }

        public async Task<int> DeleteInsert(int userId, List<UserCompany> entities)
        {
            await ValidateInputRequestAsync(entities);
            await repository.LogTransactionAndAuditTrail($"Delete and insert new user company", Domain.Attributes.UserAction.Insert, [.. entities]);

            return await repository.DeleteInsert(userId, entities);
        }
    }
}
