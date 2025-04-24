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
	[DisplayName("## Categories Shared")]
	[Menu("CategoriesShared")]
	[Route("[controller]")]
	[ApiController]
	public class CategoriesShared(CategoriesSharedService service) : ControllerBase
	{
		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> SubmitShare(RequestCategoriesShared request)
		{
			var result = await service.SubmitShare(request);
			return ResultFactory.Create(result);
		}
	}
}
