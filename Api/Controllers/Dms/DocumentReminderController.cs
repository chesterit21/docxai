using Api.Domain.Attributes;
using Api.Services.Dms;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Api.Services.Masters;
using Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Docubase.api.Controllers.Dms
{
    [DisplayName("Documents Reminders")]
    [Route("[controller]")]
    [Menu("MnDocument")]
    [ApiController]
    public class DocumentReminderController(DocumentReminderService service) : ControllerBase
    {
		[UserAction(UserAction.Read)]
		[HttpGet("get-document-reminders")]
        public async Task<IActionResult> GetDocumentReminder([FromQuery] RequestDocumentReminderNewList req)
        {
            var result = await service.GetAll(req);
            return ResultFactory.Create(result);
        }

		[UserAction(UserAction.Read)]
		[HttpGet("get-top-document-reminders")]
		public async Task<IActionResult> GetTopDocumentReminder([FromQuery] RequestDocumentReminderInTop req)
		{
			var result = await service.GetDocumentReminderInTopPanel(req);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		[UserAction(UserAction.Read)]
		[HttpGet("get-by-id")]
        public async Task<IActionResult> GetSingle(int id)
        {
            var result = await service.Get(id);
            return ResultFactory.Create(result);
        }

		[UserAction(UserAction.Insert)]
		[HttpPost]
        public async Task<IActionResult> Post(RequestDocumentRemindersNew er)
        {
            var result = await service.Create(er);
            return ResultFactory.Create(result);
        }

		[UserAction(UserAction.Update)]
		[HttpPut]
		public async Task<IActionResult> Update(RequestDocumentRemindersUpdate er)
		{
			var result = await service.Update(er);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpPut("delete")]
		public async Task<IActionResult> Inactivated(int id)
		{
			var result = await service.InActivated(id);
			return ResultFactory.Create(result);
		}
	}
}
