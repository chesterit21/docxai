using Api.Domain;
using Api.Domain.Attributes;
using Api.Services.Dms;
using Api.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("## Dropdown")]
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
	}
}
