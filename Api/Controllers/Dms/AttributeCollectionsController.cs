using Api.Domain.Attributes;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.Miscellaneous;
using Api.Domain;
using Api.Services.Masters;
using Api.Services.Systems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.ComponentModel;
using Api.Services.Dms;
using Microsoft.AspNetCore.Authorization;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityRequests.Dms;

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Attribute Collections")]
	[Menu("MnAttributes")]
	[Route("[controller]")]
	[ApiController]
	public class AttributeCollectionsController(AttributeCollectionsService service) : ControllerBase
	{
		//[UserAction(UserAction.Read)]
		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] RequestAttrCollectionList filter)
		{
			var result = await service.GetListAsync(filter, true);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		[UserAction(UserAction.Read)]
		[HttpGet("get-by-id")]
		public async Task<IActionResult> GetSingle(Guid id)
		{
			var result = await service.Get(id);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		//[UserAction(UserAction.Read)]
		[AllowAnonymous]
		[HttpGet("get-list-attribute")]
		public async Task<IActionResult> GetListAttribute()
		{
			var result = await service.GetList();
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		[UserAction(UserAction.Insert)]
		[HttpPost]
		public async Task<IActionResult> Post(RequestAttributeCollections request)
		{
			var result = await service.Insert(request);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		[UserAction(UserAction.Update)]
		[HttpPut]
		public async Task<IActionResult> Put(RequestUpdateAttributeCollections request)
		{
			var result = await service.Update(request);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpPut("delete")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await service.SoftDelete(id);
			return ResultFactory.Create();
		}

		/**
		 * Recyclebin, to shelter attribute deleted (IsActive == false)
		 * this can be restore, undeleted or delete permanent
		 **/


		[UserAction(UserAction.Update)]
		[HttpPut("undelete")]
		public async Task<IActionResult> UnDelete(Guid id)
		{
			var result = await service.SoftUndelete(id);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Read)]
		[HttpGet("recyclebin")]
		public async Task<IActionResult> GetDeleted([FromQuery] RequestAttrCollectionList filter)
		{
			var result = await service.GetListAsync(filter, false);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("hard-delete")]
		public async Task<IActionResult> HardDelete(Guid id)
		{
			var result = await service.Harddelete(id);
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
