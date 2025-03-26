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
    public class MenuService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IMenuRepository repository, UserMatrixService service) : BaseService(accessor, languageRepository)
    {
        public async Task<object> GetAll(ReqestFilter request)
        {
            await ValidateInputRequestAsync(request);
            var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            return ObjectFlatter.Flatten(result);
        }

        public async Task<Menu> Get(string menuId)
        {
            await ValidateInputAsync(menuId);
            await repository.LogTransaction($"Get menu by id {menuId}", Domain.Attributes.UserAction.Read);
            return await repository.GetSingleAsync(x => x.MenuId == menuId);
        }

        public async Task<List<Menu>> Get(int level)
        {
            await ValidateInputAsync(level);
            await repository.LogTransaction($"Get menu by level {level}", Domain.Attributes.UserAction.Read);
            return await repository.GetAsync(x => x.Level == level);
        }

        public async Task<List<Menu>> Get()
        {
            await repository.LogTransaction($"Get all menus", Domain.Attributes.UserAction.Read);
            return await repository.GetAsync();
        }

        public async Task<List<Menu>> GetNested()
        {
            await repository.LogTransaction($"Get menu as nested json result", Domain.Attributes.UserAction.Read);
            return await repository.GetNestedMenu();
        }

        public async Task<List<Menu>> GetNested(int userId)
        {
            var matrix = await service.GetFinalMatrixMenu(userId);
            var children = matrix.Matrices.CopyProperties<List<Menu>>();
            var parent = await repository.GetAsync(x => x.ParentMenuId == null);

            await repository.LogTransaction($"Get menu as nested json result by user id {userId}", Domain.Attributes.UserAction.Read);
            return repository.CreateNestedMenu(parent, children);
        }

        public async Task<List<Menu>> Upsert(List<RequestMenu> request)
        {
            await ValidateInputRequestAsync(request);
            var entities = request.CopyProperties<List<Menu>>();
            entities = entities.DistinctBy(x => x.MenuId).ToList();
            await repository.LogTransactionAndAuditTrail($"Upsert new and existing Menu", Domain.Attributes.UserAction.Insert, [.. entities]);

            return await repository.UpsertManyAsync(entities);
        }

        public async Task<Menu> Insert(RequestMenu request)
        {
            await ValidateInputRequestAsync(request);
            await CheckIfExistDuplicate(request.MenuId);

            if (!string.IsNullOrWhiteSpace(request.EN) && !string.IsNullOrWhiteSpace(request.ID))
                await languageRepository.UpdateAsync(new DataAccess.Models.Masters.Language { Code = request.MenuId, En = request.EN, Id = request.ID, Type = "Menu" });

            var entity = request.CopyProperties<Menu>();
            await repository.LogTransactionAndAuditTrail($"Insert new menu", Domain.Attributes.UserAction.Insert, entity);
            return await repository.InsertAsync(entity);
        }

        public async Task<Menu> Update(RequestMenu request)
        {
            await ValidateInputRequestAsync(request);
            await CheckIfExistDuplicate(request.MenuId);

            if (!string.IsNullOrWhiteSpace(request.EN) && !string.IsNullOrWhiteSpace(request.ID))
                await languageRepository.UpdateAsync(new DataAccess.Models.Masters.Language { Code = request.MenuId, En = request.EN, Id = request.ID, Type = "Menu" });

            var entity = request.CopyProperties<Menu>();
            await repository.LogTransactionAndAuditTrail($"Update new menu", Domain.Attributes.UserAction.Update, entity);
            return await repository.UpdateAsync(entity);
        }

        //public async Task<List<Menu>> UpsertDelete(List<RequestMenu> request)
        //{
        //    await ValidateInputRequest(request);
        //    var items = request.CopyProperties<List<Menu>>();
        //    return await menuRepository.UpsertDeleteManyAsync(items);
        //}

        public async Task<int> DeleteWithChildren(List<string> ids)
        {
            ids.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            await ValidateInputAsync(ids);

            return await repository.DeleteWithChildren(ids);
        }

        public async Task<Menu> SoftDelete(string id)
        {
            await ValidateInputAsync(id);

            await CheckIfExist(id);

            var entity = new Menu { MenuId = id };
            await repository.LogTransactionAndAuditTrail($"Soft delete existing menu with id {id}", Domain.Attributes.UserAction.Update, entity);

            return await repository.MarkAsDeletedAsync(new Menu { MenuId = id });
        }

        public async Task<Menu> SoftUndelete(string id)
        {
            await ValidateInputAsync(id);

            await CheckIfExist(id);

            var entity = new Menu { MenuId = id };
            await repository.LogTransactionAndAuditTrail($"Soft undelete existing menu with id {id}", Domain.Attributes.UserAction.Update, entity);

            return await repository.MarkAsNotDeletedAsync(new Menu { MenuId = id });
        }

        private async Task CheckIfExist(string id)
        {
            var any = await repository.AnyAsync(x => x.MenuId == id);
            if (!any)
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. Menu ID {id}");
            }
        }

        private async Task CheckIfExistDuplicate(string id)
        {
            var any = await repository.AnyAsync(x => x.MenuId == id);
            if (any)
            {
                var message = await GetMessage(LangCodes.Duplicate);
                throw new ApiException($"{message}. Menu ID {id}");
            }
        }
    }
}
