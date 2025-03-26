using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Masters;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Masters
{
    public class UserRoleService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IRoleRepository roleRepository, IUserRoleRepository userRoleRepository) : BaseService(accessor, languageRepository)
    {
        public async Task<object> GetAll(ReqestFilter request)
        {
            await ValidateInputRequestAsync(request);
            var result = await userRoleRepository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            return ObjectFlatter.Flatten(result);
        }

        public async Task<List<UserRole>> GetByUserId(int userId)
        {
            await ValidateInputAsync(userId);
            await userRoleRepository.LogTransaction($"Get role by user id {userId}", Domain.Attributes.UserAction.Read);
            return await userRoleRepository.GetAsync(x => x.UserId == userId);
        }

        public async Task<UserRole> Get(int userId, int roleId)
        {
            await ValidateInputAsync(userId);
            await ValidateInputAsync(roleId);
            await userRoleRepository.LogTransaction($"Get user {userId} role {roleId}", Domain.Attributes.UserAction.Read);
            return await userRoleRepository.GetSingleAsync(x => x.RoleId == roleId && x.UserId == userId);
        }

        public async Task<List<UserRole>> GetAll(int page, int limit)
        {
            await ValidateInputAsync([page, limit]);
            await userRoleRepository.LogTransaction($"Get user role page {page} limit {limit}", Domain.Attributes.UserAction.Read);
            return await userRoleRepository.GetAsync(page, limit);
        }

        public async Task UpsertDelete(RequestUserRole request)
        {
            await ValidateInputRequestAsync(request);

            var hasMenuDuplicate = request.Roles.GroupBy(x => x).Any(g => g.Count() > 1);
            if (hasMenuDuplicate)
            {
                var message = await GetMessage(LangCodes.Duplicate);
                throw new ApiException($"{message}. @Roles");
            }

            var rolesValid = await roleRepository.CheckIfExist(request.Roles.Distinct().ToList());
            if (!rolesValid)
            {
                var message = await GetMessage(LangCodes.InputInvalid);
                throw new ApiException($"{message}. @Roles");
            }

            var entities = new List<UserRole>();
            foreach (var roleId in request.Roles)
            {
                entities.Add(new UserRole
                {
                    RoleId = roleId,
                    UserId = request.UserId
                });
            }

            await userRoleRepository.LogTransactionAndAuditTrail($"Delete and insert new user role", Domain.Attributes.UserAction.Insert, [.. entities]);
            await userRoleRepository.DeleteInsert(request.UserId, entities);
        }

        public async Task<int> Delete(List<RequestUserRoleDelete> request)
        {
            request.RemoveAll(x => x.RoleId == 0);
            await ValidateInputRequestAsync(request);

            var entities = request.Select(x => new UserRole
            {
                UserId = x.UserId,
                RoleId = x.RoleId
            }).ToList();

            await userRoleRepository.LogTransactionAndAuditTrail($"Delete permanent user role", Domain.Attributes.UserAction.Delete, [.. entities]);

            await userRoleRepository.DeleteManyAsync(entities);
            return request.Count;
        }
    }
}
