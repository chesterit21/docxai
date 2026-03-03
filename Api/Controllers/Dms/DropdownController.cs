using Api.Domain;
using Api.Domain.Attributes;
using Api.Domain.EntityRequests.Dms;
using Api.Services.Dms;
using Api.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Dropdown")]
	[Menu("Dropdown")]
	[Route("[controller]")]
	[ApiController]
	public class DropdownController(DropdownService service) : ControllerBase
	{

		[AllowAnonymous]
		[HttpGet("ddlsharepriv")]
		public async Task<IActionResult> GetDdlSharedPrivillege()
		{
			var result = await service.GetDropdownSharedPrivillege();
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("ddlusers")]
		public async Task<IActionResult> GetDdlUsers()
		{
			var result = await service.GetDropdownListUsers();
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("ddlroles")]
		public async Task<IActionResult> GetDdlRoles()
		{
			var result = await service.GetDropdownListRoles();
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("ddlcompany")]
		public async Task<IActionResult> GetDdlCompany()
		{
			var result = await service.GetDropdownListCompany();
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("ddlwatermarks")]
		public async Task<IActionResult> GetDdlWatermarks()
		{
			var result = await service.GetDropdownListWatermarks();
			return ResultFactory.Create(result);
		}


		[AllowAnonymous]
		[HttpGet("ddlgroups")]
		public async Task<IActionResult> GetDdlGroup()
		{
			var result = await service.GetDropdownListGroup();
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("ddldocuments")]
		public async Task<IActionResult> GetDdlDocuments([FromQuery]RequestDocumentsDropdown search)
		{
			var result = await service.GetDropdownDocument(search);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("ddlusersandgroups")]
		public async Task<IActionResult> GetDdlUsersAndGroups()
		{
			var result = await service.GetDropdownListUsersAndGroups();
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("ddlattributes")]
		public async Task<IActionResult> GetDdlAttributes()
		{
			var result = await service.GetDropdownListUsersAndGroups();
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpGet("ddlCategories")]
		public async Task<IActionResult> GetDdlCategories([FromQuery] RequestCategoryDropdown search)
		{
			var result = await service.GetDropdownCategories(search);
			return ResultFactory.Create(result);
		}
		//public async Task<List<DropdownTextValue>> GetDropdownCategories(RequestCategoryDropdown request)
	}
}
