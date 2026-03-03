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
using Api.Extensions;
using Api.Domain.EntityResponses.Dms;

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Watermarks")]
	[Menu("MnWatermark")]
	[Route("[controller]")]
	[ApiController]
	public class WatermarksController(WatermarkService service) : ControllerBase
	{
		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] RequestWatermarkList filter)
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
		[HttpPost]
		public async Task<IActionResult> Post(RequestAddWatermarks request)
		{
			var result = await service.Insert(request);
			var result2 = result.CopyProperties<ResponseWatermark>();
			return ResultFactory.Create(result2);
		}

		[AllowAnonymous]
		[HttpPut]
		public async Task<IActionResult> Put(RequestWatermarks request)
		{
			var result = await service.Update(request);
			var result2 = result.CopyProperties<ResponseWatermark>();
			return ResultFactory.Create(result2);
		}

		[AllowAnonymous]
		[HttpPut("delete")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await service.Delete(id);
			//var result = await service.DeleteWithChildren(ids);
			return ResultFactory.Create(result);
		}
	}
}
