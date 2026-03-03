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
	[DisplayName("Shared Categories")]
	[Menu("MnDocument")]
	[Route("[controller]")]
	[ApiController]
	public class CategoriesSharedController(CategoriesSharedService service) : ControllerBase
	{
		[UserAction(UserAction.Insert)]
		[HttpPost("add-shared")]
		public async Task<IActionResult> SubmitShare(RequestCreateCategoriesShared request)
		{
			var result = await service.SubmitShare(request);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-shared-users")]
		public async Task<IActionResult> GetListSharedUsers([FromQuery]int categoryId)
		{
			var result = await service.GetListSharedUsers(categoryId);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("delete-shared-category")]
		public async Task<IActionResult> DeleteWorkflow([FromQuery] int categoryId)
		{
			var result = await service.DeleteSharedCategory(categoryId);
			return ResultFactory.Create();
		}

	}
}
