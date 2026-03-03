using Api.Domain;
using Api.Domain.Attributes;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Services.Dms;
using Api.Services.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Shared Workspace")]
	[Menu("MnSharedWorkspace")]
	[Route("[controller]")]
	[ApiController]
	public class SharedWorkspaceController(SharedWorkspaceService service) : ControllerBase
	{
		//[AllowAnonymous]
		//[HttpGet("mysharedfile{page}/{limit}")]
		//public async Task<IActionResult> GetMySharedFile(int page, int limit)
		//{
		//	var result = await service.GetAllMySharedFiles(page, limit);
		//	return ResultFactory.Create(result);
		//}

		[AllowAnonymous]
		[HttpGet("mysharedfile")]
		public async Task<IActionResult> GetFilterMySharedFile([FromQuery] RequestWorkspace filter)
		{
			var result = await service.GetMySharedFiles(filter);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		//[HttpGet("sharedtome{page}/{limit}")]
		//public async Task<IActionResult> GetSharedToMe(int page, int limit)
		//{
		//	var result = await service.GetAllMySharedFiles(page, limit);
		//	return ResultFactory.Create(result);
		//}

		[AllowAnonymous]
		[HttpGet("sharedtome")]
		public async Task<IActionResult> GetFIlterGetSharedToMe([FromQuery] RequestWorkspace request)
		{
			var result = await service.GetSharedToMe(request);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpDelete("delete-category-shared-tome")]
		public async Task<IActionResult> DeleteCategoryShareToMe([FromQuery] int categoryId)
		{
			var result = await service.DeleteCategoryShareToMe(categoryId);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpDelete("delete-document-shared-tome")]
		public async Task<IActionResult> DeleteDocumentShareToMe([FromQuery] int documentId)
		{
			var result = await service.DeleteDocumentShareToMe(documentId);
			return ResultFactory.Create(result);
		}
	}
}
