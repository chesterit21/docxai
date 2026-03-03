using Api.Domain.Attributes;
using Api.Services.Dms;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Api.Services.Masters;
using Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Api.Domain.EntityRequests.Dms;
using Api.DataAccess.Models.Dms;
using System.Diagnostics;
using Api.Domain.EntityRequests;
using Api.Domain.EntityResponses.Dms;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Docubase.api.Controllers.Dms
{
    [DisplayName("Approval")]
    [Route("[controller]")]
    [Menu("MnApproval")]
    [ApiController]
    public class ApprovalController(ApprovalService service) : ControllerBase
    {
		[UserAction(UserAction.Read)]
		[HttpGet("get-approval-task")]
        //using to get screen workflow popup too
        public async Task<IActionResult> GetMyApprovalTask([FromQuery]RequestPagination request)
        {
            var result = await service.GetMyApprovalTask(request);
            return ResultFactory.Create(result);
        }

		[UserAction(UserAction.Read)]
		[HttpGet("get-approval-request")]
        public async Task<IActionResult> GetMyApprovalRequest([FromQuery] RequestPagination request)
        {

            var result = await service.GetMyApprovalRequest(request);
            return ResultFactory.Create(result);
        }

		[UserAction(UserAction.Update)]
		[HttpGet("check-approval")]
		public async Task<IActionResult> GetInfoCheckForApprovalPageView([FromQuery]int approvalId)
		{
			var result = await service.GetInfoCheckForApprovalPageView(approvalId);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Update)]
		[HttpPost("approve")]
		public async Task<IActionResult> Approve(RequestActionApproval aproval)
		{
			//aproval
			RequestApprovalActivities act = new RequestApprovalActivities() 
			{
				//act.ApprovalActivityName = "approve",
				Reason = aproval.Reason,
				RelatedDocumentID = aproval.RelatedDocumentId,
				ApprovalActivityName = "Approve",
			};
			var result = await service.Approve(aproval.Id, act);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Update)]
		[HttpPost("reject")]
		//public async Task<IActionResult> Reject(int ApprovalId, RequestApprovalActivities activity)
		public async Task<IActionResult> Reject(RequestActionApproval reject)
		{
			RequestApprovalActivities act = new RequestApprovalActivities()
			{
				Reason = reject.Reason,
				RelatedDocumentID = reject.RelatedDocumentId,
				ApprovalActivityName = "Reject",

			};
			var result = await service.Reject(reject.Id, act);
			return ResultFactory.Create(result);
		}

		//// POST api/<DocumentsController>
		//[HttpPost]
		//public void Post([FromBody] string value)
		//{
		//}

		//// PUT api/<DocumentsController>/5
		//[HttpPut("{id}")]
		//public void Put(int id, [FromBody] string value)
		//{
		//}

		//// DELETE api/<DocumentsController>/5
		//[HttpDelete("{id}")]
		//public void Delete(int id)
		//{
		//}
	}
}
