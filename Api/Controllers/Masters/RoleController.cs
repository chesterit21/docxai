namespace Docubase.api.Controllers.Masters
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Domain.EntityRequests.Authentications;
	using Api.Domain.EntityRequests.Masters;
	using Api.Services.Masters;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using System.ComponentModel;
    using System.Net;


    [DisplayName("Role")]
    [Menu("MnUserGroup")]
    [Route("[controller]")]
    [ApiController]
    public class RoleController(RoleService service, RoleMatrixService roleMatrixSevice) : ControllerBase
    {
		[AllowAnonymous]
		[HttpGet("{page}/{limit}")]
		public async Task<IActionResult> GetGroups(int page, int limit)
		{
			var result = await service.GetAll(page, limit);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("filter")]
        public async Task<IActionResult> GetFilter([FromQuery] RequestFilter filter)
        {
            var result = await service.GetAll(filter);
            return ResultFactory.Create(result);
        }

		[AllowAnonymous]
		[HttpGet("get-byid")]
		public async Task<IActionResult> GetSingle(int id)
		{
			var result = await service.Get(id);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("get-group-members")]
		public async Task<IActionResult> GetWithMember(int id)
		{
			var result = await service.GetWithMember(id);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> Post(RequestRoleCreate request)
		{
			var result = await service.Insert(request);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpPut]
		public async Task<IActionResult> Put(RequestRoleUpdate request)
		{
			var result = await service.UpdateUserRoleMember(request);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpPut("delete")]
        public async Task<IActionResult> Delete(int userId)
        {
            var result = await service.SoftDelete(userId);
            return ResultFactory.Create(result);
        }

        [AllowAnonymous]
        [HttpPost("set-role-matrix")]
        public async Task<IActionResult> Post(RequestRoleMatrix request)
        {
            await roleMatrixSevice.UpsertDelete(request);
            return ResultFactory.Create();
        }
    }
}
