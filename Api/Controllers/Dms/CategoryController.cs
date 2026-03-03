using Api.Domain.Attributes;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.Miscellaneous;
using Api.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.ComponentModel;
using Api.Services.Dms;
using Microsoft.AspNetCore.Authorization;
using Api.Domain.EntityRequests.Dms;

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Categories")]
	[Menu("MnDocument")]
	[Route("[controller]")]
	[ApiController]
	public class CategoryController(CategoriesService service) : ControllerBase
	{
		[UserAction(UserAction.Read)]
		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] RequestCategoryList filter)
		{
			var result = await service.GetAll(filter, true);//2nd parmeter is column IActive, true means active only
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("recyclebin")]
		public async Task<IActionResult> GetDeleted([FromQuery] RequestCategoryList filter)
		{
			var result = await service.GetAll(filter, false);//2nd parmeter is column IActive, true means active only
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("empty-recyclebin")]
		public async Task<IActionResult> EmptyRecyclebin()
		{
			var result = await service.EmptyRecyclebin();
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-by-id")]
		public async Task<IActionResult> GetSingle(int id)
		{
			var result = await service.Get(id);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Insert)]
		[HttpPost]
		public async Task<IActionResult> Post(RequestCategory request)
		{
			var result = await service.Create(request);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Update)]
		[HttpPut]
		public async Task<IActionResult> Put(RequestCategory request)
		{
			var result = await service.Update(request);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		[UserAction(UserAction.Update)]
		[HttpPut("delete")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await service.SoftDelete(id);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		[UserAction(UserAction.Update)]
		[HttpPut("undelete")]
		public async Task<IActionResult> UnDelete(int id)
		{
			var result = await service.SoftUnDelete(id);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("hard-delete")]
		public async Task<IActionResult> HardDelete(int id)
		{
			var result = await service.HardDelete(id);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Insert)]
		[HttpPut("un-favorite")]
		public async Task<IActionResult> Unfavorite([FromQuery] int categoryId)
		{
			var res = await service.UnFavorite(categoryId);
			return ResultFactory.Create("OK", HttpStatusCode.OK);
		}

		[UserAction(UserAction.Update)]
		[HttpPut("add-favorite")]
		public async Task<IActionResult> AddFavorite([FromQuery] int categoryId)
		{
			var res = await service.AddFavorite(categoryId);
			return ResultFactory.Create("OK", HttpStatusCode.OK);
		}

		[UserAction(UserAction.Update)]
		[HttpPost("add-workflow")]
		public async Task<IActionResult> AddWorkFlow(RequestCategoryWorkflow workflow)
		{
			var result = await service.AddWorkFlow(workflow);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-workflow-users")]
		public async Task<IActionResult> GetListworkflowUsers([FromQuery]int categoryId)
		{
			var result = await service.GetApprovalFlow(categoryId);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-workflow")]
		public async Task<IActionResult> GetWorkflow([FromQuery] int categoryId)
		{
			var result = await service.GetWorkFlow(categoryId);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("delete-workflow")]
		public async Task<IActionResult> DeleteWorkflow([FromQuery] int categoryId)
		{
			var result = await service.DeleteWorkFlow(categoryId);
			return ResultFactory.Create();
		}

		//[UserAction(UserAction.Update)]
		//[HttpPut("undelete")]
		//public async Task<IActionResult> UnDelete(int id)
		//{
		//	var result = await service.SoftUnDelete(id);
		//	return ResultFactory.Create();
		//}

		//[UserAction(UserAction.Read)]
		//[HttpGet("recyclebin")]
		//public async Task<IActionResult> GetDeleted([FromQuery] RequestAttrList filter)
		//{
		//	var result = await service.GetAll(filter, false);
		//	return ResultFactory.Create(result);
		//}

		//[UserAction(UserAction.Delete)]
		//[HttpDelete("hard-delete")]
		//public async Task<IActionResult> HardDelete(int id)
		//{
		//	var result = await service.HardDelete(id);
		//	return ResultFactory.Create();
		//}

	}

}
