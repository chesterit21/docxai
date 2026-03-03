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
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;

namespace Api.Services.Masters
{
	public class MenuService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IUserRepository userRepository, IMenuRepository repository,
		UserMatrixService userMatrixSvc, IUserRoleRepository userRoleRepository, IRoleMatrixRepository roleMatrixRepo) : BaseService(accessor, languageRepository)
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
			var matrix = await userMatrixSvc.GetFinalMatrixMenu(userId);
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
			{
				var any = await languageRepository.AnyAsync(x => x.Code == request.MenuId);
				if (any)
					await languageRepository.UpdateAsync(new DataAccess.Models.Masters.Language { Code = request.MenuId, En = request.EN, Id = request.ID, Type = "Menu" });
				else
					await languageRepository.InsertAsync(new DataAccess.Models.Masters.Language { Code = request.MenuId, En = request.EN, Id = request.ID, Type = "Menu" });
			}

			var entity = request.CopyProperties<Menu>();
			await repository.LogTransactionAndAuditTrail($"Insert new menu", Domain.Attributes.UserAction.Insert, entity);
			return await repository.InsertAsync(entity);
		}

		public async Task<Menu> Update(RequestMenu request)
		{
			await ValidateInputRequestAsync(request);

			if (!string.IsNullOrWhiteSpace(request.EN) && !string.IsNullOrWhiteSpace(request.ID))
			{
				var any = await languageRepository.AnyAsync(x => x.Code == request.MenuId);
				if (any)
					await languageRepository.UpdateAsync(new DataAccess.Models.Masters.Language { Code = request.MenuId, En = request.EN, Id = request.ID, Type = "Menu" });
				else
					await languageRepository.InsertAsync(new DataAccess.Models.Masters.Language { Code = request.MenuId, En = request.EN, Id = request.ID, Type = "Menu" });
			}


			var entity = request.CopyProperties<Menu>();
			await repository.LogTransactionAndAuditTrail($"Update menu", Domain.Attributes.UserAction.Update, entity);
			var fromdb = await repository.GetSingleAsync(x => x.MenuId == entity.MenuId);
			fromdb.CopyPropertiesFrom(entity);
			return await repository.UpdateAsync(fromdb);
		}

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

		public async Task<List<ResponseMenus>> GetMenuForLogedUser()
		{
			var name = accessor?.HttpContext?.User?.Identity?.Name;
			var userId = name == null ? 0 : int.Parse(name);
			var user = await userRepository.GetUser(userId);

			if (user == null)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. User ID {userId}");
			}

			var userRoles = await userRoleRepository.GetRolesByUserId(userId);
			if (userRoles.Count == 0)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. User Role With User ID {userId}");
			}

			var langMenus = await languageRepository.GetAsync(x => x.Type == "Menu");
			var roleMatrices = await roleMatrixRepo.GetAsync(x => userRoles.Contains(x.RoleId));
			var activeMenus = await repository.GetAsync(x => x.IsActive == true);

			var ownedMenus = activeMenus.Where(x => x.IsActive == true)
				.Join(roleMatrices,
					menu => menu.MenuId,
					rolemtx => rolemtx.MenuId,
					(menu, rolemtx) => new ResponseMenus()
					{
						MenuId = menu.MenuId,
						ParentMenuId = menu.ParentMenuId,
						Description = menu.Description,
						Level = menu.Level,
						Icon = menu.Icon,
						Url = menu.Url,
						Id = langMenus.FirstOrDefault(x => x.Code == menu.MenuId).Id,
						En = langMenus.FirstOrDefault(x => x.Code == menu.MenuId).En,
						IsRead = rolemtx.IsRead,
						IsInsert = rolemtx.IsInsert,
						IsDelete = rolemtx.IsDelete,
						IsUpdate = rolemtx.IsUpdate
					}).ToList();

			return ownedMenus;
		}
	}
}
