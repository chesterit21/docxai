namespace Docubase.api.Controllers.Masters
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Domain.EntityRequests.Authentications;
	using Api.Domain.EntityRequests.Dms;
	using Api.Domain.EntityRequests.Masters;
	using Api.Services.Masters;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using System.ComponentModel;
    using System.Net;


    [DisplayName("Group")]
    [Menu("MnUserGroup")]
    [Route("[controller]")]
    [ApiController]
    public class GroupController(GroupService service) : ControllerBase
    {
		[UserAction(UserAction.Read)]
		[HttpGet("filter")]
        public async Task<IActionResult> GetFilter([FromQuery] RequestGroupList filter)
        {
            var result = await service.GetAll(filter, true);
            return ResultFactory.Create(result);
        }

		[UserAction(UserAction.Read)]
		[HttpGet("get-byid")]
		public async Task<IActionResult> GetSingle(int id)
		{
			var result = await service.Get(id);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-group-members")]
		public async Task<IActionResult> GetWithMember(int id)
		{
			var result = await service.GetWithMember(id);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Insert)]
		[HttpPost]
		public async Task<IActionResult> Post(RequestGroupCreate request)
		{
			var result = await service.Insert(request);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Update)]
		[HttpPut]
		public async Task<IActionResult> Put(RequestGroupUpdateUser request)
		{
			var result = await service.UpdateUserGroupMember(request);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpPut("delete")]
		public async Task<IActionResult> Delete(int groupId)
		{
			var result = await service.SoftDelete(groupId);
			//var result = await service.DeleteWithChildren(ids);
			return ResultFactory.Create(result);
		}

		/**
		 * Recyclebin, to shelter attribute deleted (IsActive == false)
		 * this can be restore, undeleted or delete permanent
		 **/

		[UserAction(UserAction.Update)]
		[HttpPut("undelete")]
		public async Task<IActionResult> UnDelete(int grpId)
		{
			var result = await service.SoftUndelete(grpId);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("recyclebin")]
		public async Task<IActionResult> GetDeleted([FromQuery] RequestGroupList filter)
		{
			var result = await service.GetAll(filter, false);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("hard-delete")]
		public async Task<IActionResult> HardDelete(int id)
		{
			var result = await service.HardDelete(id);
			return ResultFactory.Create();
		}

		[HttpDelete("empty-recyclebin")]
		public async Task<IActionResult> EmptyRecyclebin()
		{
			var result = await service.EmptyRecyclebin();
			return ResultFactory.Create(result);
		}
	}
}
