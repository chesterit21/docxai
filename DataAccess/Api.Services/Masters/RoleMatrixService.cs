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
    public class RoleMatrixService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IRoleMatrixRepository repository, IMenuRepository menuRepository, IRoleRepository roleRepository) : BaseService(accessor, languageRepository)
    {
        public async Task<object> GetAll(ReqestFilter request)
        {
            await ValidateInputRequestAsync(request);
            var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            return ObjectFlatter.Flatten(result);
        }

        public async Task<object> Get(int roleId)
        {
            await ValidateInputAsync(roleId);
            if (!await roleRepository.CheckIfExist([roleId]))
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. Role ID {roleId}");
            }

            await repository.LogTransaction($"Get role matrix by role ID {roleId}", Domain.Attributes.UserAction.Read);

            var result = await repository.GetAsync(roleId);
            var menus = await languageRepository.GetAsync(x => x.Type == "Menu");
            if (menus == null || menus.Count == 0)
                return result;

            foreach (var item in result)
            {
                var lang = menus.FirstOrDefault(x => x.Code == item.MenuId);
                if (lang == null)
                    continue;

                item.Id = lang.Id;
                item.En = lang.En;
            }

            return result;
        }

        public async Task UpsertDelete(RequestRoleMatrix request)
        {
            await ValidateInputRequestAsync(request);

            var hasMenuDuplicate = request.Matrices.GroupBy(x => x.MenuId).Any(g => g.Count() > 1);
            if (hasMenuDuplicate)
            {
                var message = await GetMessage(LangCodes.Duplicate);
                throw new ApiException($"{message}. @Role ID");
            }

            var menuValid = await menuRepository.CheckIfExist(request.Matrices.Select(x => x.MenuId).ToList());
            if (!menuValid)
            {
                var message = await GetMessage(LangCodes.InputInvalid);
                throw new ApiException($"{message}. @Menu ID not found");
            }

            var entities = new List<RoleMatrix>();
            foreach (var a in request.Matrices)
            {
                entities.Add(new RoleMatrix
                {
                    IsDelete = a.IsDelete,
                    RoleId = request.RoleId,
                    MenuId = a.MenuId,
                    IsInsert = a.IsInsert,
                    IsUpdate = a.IsUpdate,
                    IsRead = a.IsRead
                });
            }

            await repository.LogTransactionAndAuditTrail($"Upsert new role matrix", Domain.Attributes.UserAction.Insert, [.. entities]);

            await repository.DeleteInsert(entities);
        }

    }
}