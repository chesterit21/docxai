using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Dms;
using Api.Extensions;
using Api.Repository;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Masters
{
	public class GroupService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IGroupRepository repository) 
		: BaseService(accessor, languageRepository)
	{
		public async Task<object> GetAll(RequestGroupList request, bool isActive)
		{
			await ValidateInputRequestAsync(request);
			ResponseGroupPagination result = await repository.GetAsync(request.search, isActive, request.Page, request.Limit);

			var totalRecords = result.TotalREcord;

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = result.Record
			};
			//var result = await repository.GetAsync(x => x.IsActive == true, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			//return ObjectFlatter.Flatten(result);
		}

		public async Task<Group> Get(int GrpId)
		{
			await ValidateInputAsync(GrpId);
			await repository.LogTransaction($"Get group by group ID {GrpId}", Domain.Attributes.UserAction.Read);
			return await repository.GetSingleAsync(x => x.GroupId == GrpId);
		}

		public async Task<object> GetWithMember(int groupId)
		{
			await ValidateInputAsync(groupId);
			await repository.LogTransaction($"Get group with member by group ID : {groupId}", Domain.Attributes.UserAction.Read);
			return await repository.GetByIdAndMembers(groupId);
		}


		//public async Task<List<Group>> GetAll(int page, int limit)
		//{
		//	await ValidateInputAsync([page, limit]);
		//	await repository.LogTransaction($"Get role page {page} limit {limit}", Domain.Attributes.UserAction.Read);
		//	return await repository.GetAsync(page, limit);
		//}

		public async Task<Group> GetByName(string groupName)
		{
			await ValidateInputAsync(groupName);
			await repository.LogTransaction($"Get group by name", Domain.Attributes.UserAction.Read);
			return await repository.GetGroupByName(groupName);
		}

		public async Task<List<Group>> Upsert(List<RequestGroupUpdate> request)
		{
			await ValidateInputRequestAsync(request);

			var entities = request.CopyProperties<List<Group>>();

			if (entities[1].GroupId != 0)
				entities = entities.DistinctBy(x => x.GroupId).ToList();

			return await repository.UpsertManyAsync(entities);
		}

		public async Task<Group> Insert(RequestGroupCreate request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(0, request.GroupName);
			await CheckGroupEveryone(request.GroupName);

			var entity = request.CopyProperties<Group>();
			entity.IsSystem = false;
			entity.IsDeleted = false;
			entity.IsActive = true;
			entity.InsertedAt = DateTime.Now;
			entity.InsertedBy = GetUser();
			await repository.LogTransactionAndAuditTrail($"Insert new role", Domain.Attributes.UserAction.Insert, entity);
			return await repository.InsertAsync(entity);
		}

		public async Task<Group> Update(RequestGroupUpdate request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(request.GroupId, request.GroupName);
			await CheckGroupEveryone(request.GroupName);

			var entity = request.CopyProperties<Group>();
			await repository.LogTransactionAndAuditTrail($"Update existing group with id {entity.GroupId}", Domain.Attributes.UserAction.Update, entity);
			return await repository.UpdateAsync(entity);
		}

		public async Task<bool> UpdateUserGroupMember(RequestGroupUpdateUser request)
		{
			await ValidateInputRequestAsync(request);
			var entity = request.CopyProperties<Group>();
			await repository.LogTransactionAndAuditTrail($"Update existing group with id {entity.GroupId}", Domain.Attributes.UserAction.Update, entity);
			return repository.UpdateGroupMembers(request);
		}

		//public async Task<List<Group>> UpsertDelete(List<RequestRoleCreate> request)
		//{
		//	await ValidateInputRequestAsync(request);
		//	var entities = request.CopyProperties<List<Group>>();
		//	entities = entities.DistinctBy(x => x.RoleId).ToList();
		//	return await repository.UpsertDeleteManyAsync(entities);
		//}

		public async Task<int> DeleteWithChildren(List<int> roleIds)
		{
			roleIds.RemoveAll(x => x == 0);
			await ValidateInputAsync(roleIds);

			return await repository.DeleteWithChildren(roleIds);
		}

		public async Task<Group> SoftDelete(int groupId)
		{
			await ValidateInputAsync(groupId);
			await CheckIfExist(groupId);
			Group grp = await repository.GetSingleAsync(c => c.GroupId == groupId);
			await CheckGroupEveryone(grp.GroupName);

			var entity = new Group { GroupId = groupId };
			await repository.LogTransactionAndAuditTrail($"Soft delete existing group with id {groupId}", Domain.Attributes.UserAction.Update, entity);
			return await repository.MarkAsDeletedAsync(groupId);
			//return await repository.MarkAsDeletedAsync(entity);
		}

		public async Task<Group> SoftUndelete(int groupId)
		{
			await ValidateInputAsync(groupId);
			await CheckIfExist(groupId);

			var entity = new Group { GroupId = groupId };
			await repository.LogTransactionAndAuditTrail($"Soft undelete existing group with id {groupId}", Domain.Attributes.UserAction.Update, entity);

			//return await repository.MarkAsNotDeletedAsync(entity);
			return await repository.MarkAsUnDeletedAsync(groupId);
		}

		public async Task<int> HardDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);
			var entity = await repository.GetSingleAsync(x => x.GroupId == id);
			await repository.LogTransactionAndAuditTrail($"Hard Delete Group (IsDelete = true) with Group ID {id}", Domain.Attributes.UserAction.Delete, entity);
			//check again if usr has been used in, group user, user company, user role
			return await repository.DeleteAsync(id);
		}

		private async Task CheckIfExist(int groupId)
		{
			var any = await repository.AnyAsync(x => x.GroupId == groupId);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Group ID {groupId}");
			}
		}

		private async Task CheckIfExist(int id, string name)
		{
			var any = await repository.AnyAsync(x => x.GroupId == id || x.GroupName == name);
			if (any)
			{
				var message = await GetMessage(LangCodes.Duplicate);
				throw new ApiException($"{message}. Group ID {id}, Group Name {name}");
			}
		}

		private async Task CheckGroupEveryone(string name)
		{
			var any = await repository.GetSingleAsync(x => x.GroupName.Trim().ToLower() == name.Trim().ToLower());
			if (any != null)
			{
				if (any.GroupName.Trim().ToLower() == "everyone")
				{
					var message = await GetMessage(LangCodes.GroupEveryone);
					throw new ApiException($"{message}. Group Name {name}");
				}
			}
		}

		public async Task<bool> EmptyRecyclebin()
		{
			await repository.LogTransaction($"Empty recyclebin of group", Domain.Attributes.UserAction.Delete);
			return await repository.EmptyRecyclebin(20);
		}

		//public async Task<Group> HardDelete(int id)
		//{
		//	await ValidateInputAsync(id);
		//	await CheckIfExist(id);
		//	//var entity = new Attributes { Id = id };
		//	var entity = await repository.GetSingleAsync(x => x.GroupId == id);
		//	await repository.LogTransactionAndAuditTrail($"Hard Delete Group with Group Id {id}", Domain.Attributes.UserAction.Delete, entity);
		//	return await repository.DeleteAsync(entity);
		//}
	}
}