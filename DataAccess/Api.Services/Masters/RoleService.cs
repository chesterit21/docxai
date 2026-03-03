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
	public class RoleService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IRoleRepository repository) : BaseService(accessor, languageRepository)
	{
		public async Task<object> GetAll(RequestFilter request)
		{
			await ValidateInputRequestAsync(request);
			var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			return ObjectFlatter.Flatten(result);
		}

		public async Task<Role> Get(int roleId)
		{
			await ValidateInputAsync(roleId);
			await repository.LogTransaction($"Get role by role ID {roleId}", Domain.Attributes.UserAction.Read);
			return await repository.GetSingleAsync(x => x.RoleId == roleId);
		}

		public async Task<object> GetWithMember(int roleId)
		{
			await ValidateInputAsync(roleId);
			await repository.LogTransaction($"Get role with member by role ID {roleId}", Domain.Attributes.UserAction.Read);
			return await repository.GetByIdAndMembers(roleId);
		}


		public async Task<List<Role>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);
			await repository.LogTransaction($"Get role page {page} limit {limit}", Domain.Attributes.UserAction.Read);
			return await repository.GetAsync(page, limit);
		}

		public async Task<Role> GetByName(string rolename)
		{
			await ValidateInputAsync(rolename);
			await repository.LogTransaction($"Get role by name", Domain.Attributes.UserAction.Read);
			return await repository.GetByName(rolename);
		}

		public async Task<List<Role>> Upsert(List<RequestRoleUpdate> request)
		{
			await ValidateInputRequestAsync(request);

			var entities = request.CopyProperties<List<Role>>();

			if (entities[1].RoleId != 0)
				entities = entities.DistinctBy(x => x.RoleId).ToList();

			return await repository.UpsertManyAsync(entities);
		}

		public async Task<Role> Insert(RequestRoleCreate request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(0, request.Name);

			var entity = request.CopyProperties<Role>();
			await repository.LogTransactionAndAuditTrail($"Insert new role", Domain.Attributes.UserAction.Insert, entity);
			return await repository.InsertAsync(entity);
		}

		public async Task<Role> Update(RequestRoleUpdate request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(request.RoleId, request.Name);

			var entity = request.CopyProperties<Role>();
			await repository.LogTransactionAndAuditTrail($"Update existing role with id {entity.RoleId}", Domain.Attributes.UserAction.Update, entity);
			return await repository.UpdateAsync(entity);
		}

		public async Task<bool> UpdateUserRoleMember(RequestRoleUpdate request)
		{
			await ValidateInputRequestAsync(request);
			//await CheckIfExist(request.RoleId, request.Name);

			var entity = request.CopyProperties<Role>();
			await repository.LogTransactionAndAuditTrail($"Update existing role with id {entity.RoleId}", Domain.Attributes.UserAction.Update, entity);
			return repository.UpdateRoleMembers(request);
		}

		public async Task<List<Role>> UpsertDelete(List<RequestRoleCreate> request)
		{
			await ValidateInputRequestAsync(request);
			var entities = request.CopyProperties<List<Role>>();
			entities = entities.DistinctBy(x => x.RoleId).ToList();
			return await repository.UpsertDeleteManyAsync(entities);
		}

		public async Task<int> DeleteWithChildren(List<int> roleIds)
		{
			roleIds.RemoveAll(x => x == 0);
			await ValidateInputAsync(roleIds);

			return await repository.DeleteWithChildren(roleIds);
		}

		public async Task<Role> SoftDelete(int roleId)
		{
			await ValidateInputAsync(roleId);

			await CheckIfExist(roleId);

			var entity = new Role { RoleId = roleId };
			await repository.LogTransactionAndAuditTrail($"Soft delete existing role with id {roleId}", Domain.Attributes.UserAction.Update, entity);

			return await repository.MarkAsDeletedAsync(entity);
		}

		public async Task<Role> SoftUndelete(int roleId)
		{
			await ValidateInputAsync(roleId);
			await CheckIfExist(roleId);

			var entity = new Role { RoleId = roleId };
			await repository.LogTransactionAndAuditTrail($"Soft undelete existing role with id {roleId}", Domain.Attributes.UserAction.Update, entity);

			return await repository.MarkAsNotDeletedAsync(entity);
		}

		private async Task CheckIfExist(int roleId)
		{
			var any = await repository.AnyAsync(x => x.RoleId == roleId);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Role ID {roleId}");
			}
		}

		private async Task CheckIfExist(int id, string name)
		{
			var any = await repository.AnyAsync(x => x.RoleId == id || x.Name == name);
			if (any)
			{
				var message = await GetMessage(LangCodes.Duplicate);
				throw new ApiException($"{message}. Role ID {id}, name {name}");
			}
		}
	}
}