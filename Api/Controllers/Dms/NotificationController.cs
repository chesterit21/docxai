using Microsoft.AspNetCore.Mvc;
using Api.Domain.Attributes;
using System.ComponentModel;
using Api.Services.Masters;
using Api.Domain;
using Api.Domain.EntityRequests.Dms;
using System.Net;
using Api.DataAccess.Models.Dms;
using ProtoBuf.Meta;
using MailKit.Search;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Notification")]
	[Menu("MnDocument")]
	[ApiController]
	[Route("[controller]")]
	public class NotificationController(NotificationService service) : ControllerBase
	{
		[UserAction(UserAction.Read)]
		[HttpGet("search")]
		public async Task<IActionResult> NotificationList([FromQuery] RequestNotification filter)
		{
			var result = await service.NotificationList(filter);
			return ResultFactory.Create(result);
		}

		//// POST api/<TopController>
		//[HttpPost]
		//public void Post([FromBody] string value)
		//{
		//}

		//// PUT api/<TopController>/5
		//[HttpPut("{id}")]
		//public void Put(int id, [FromBody] string value)
		//{
		//}

		//// DELETE api/<TopController>/5
		//[HttpDelete("{id}")]
		//public void Delete(int id)
		//{
		//}
	}
}
