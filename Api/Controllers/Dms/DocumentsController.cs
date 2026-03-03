using Api.Domain.Attributes;
using Api.Services.Dms;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Api.Services.Masters;
using Api.Domain;
using Microsoft.AspNetCore.Authorization;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityRequests;
using System.Net;
using Api.DataAccess.Models.Dms;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Documents")]
	[Route("[controller]")]
	[Menu("MnDocument")]
	[ApiController]
	public class DocumentsController(DocumentsService service) : ControllerBase
	{
		//[AllowAnonymous]
		[UserAction(UserAction.Insert)]
		[HttpPost("add-initial-document")]
		public async Task<IActionResult> AddInitDocument(RequestDocumentAddUpload request)
		{
			if (request.DocFile == null || request.DocFile.Length == 0)
			{
				return BadRequest("No file uploaded.");
			}
			var result = await service.InsertInitAddWithUploadDocument(request);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		[UserAction(UserAction.Insert)]
		[HttpPost("upload-initial-document/{catid?}")]
		public async Task<IActionResult> UploadInitDocument(string catid, List<IFormFile> docs)
		{
			var result = await service.UploadInitDocument(catid, docs);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-document-dtl")]
		//using to get screen workflow popup too
		public async Task<IActionResult> GetDocumentDetail([FromQuery]int id, [FromQuery] string mode)
		{
			var result = await service.GetDocumentDetail(id, mode);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Update)]
		[HttpPost("upload-new-version")]
		public async Task<IActionResult> UploadNewVersion([FromForm] RequestDocumentUpload uploadreq)
		{

			if (uploadreq.fileUpload
				== null || uploadreq.fileUpload.Length == 0)
			{
				return BadRequest("No file uploaded.");
			}

			int id = uploadreq.DocumentID;
			var file = uploadreq.fileUpload;
			bool isMainDocumentFile = uploadreq.IsMainDocumentFile;

			var docfile = await service.UploadNewVersionofFile(id, file, isMainDocumentFile);
			return ResultFactory.Create(docfile);
		}

		[UserAction(UserAction.Update)]
		[HttpPost("set-main-file-document")]
		public async Task<IActionResult> SetMainFileDocument(RequestDocumentFilesUpdateMainDocument dcf)
		{
			DocumentFiles model = await service.SetMainFileDocument(dcf);
			return ResultFactory.Create(model);
		}

		//[UserAction(UserAction.UpdateDocument)]
		//[HttpPost("set-reminder-document")]
		//public async Task<IActionResult> SetReminderDocument([FromForm] RequestDocumentReminder docRemind)
		//{
		//	int documentId = docRemind.DocumentIDs;
		//	short? reminderDay = docRemind.DayOfReminder;
		//	DateTime? datetimeReminder = docRemind.DateofReminder;
		//	await service.SetReminderDocument(documentId, reminderDay, datetimeReminder);
		//	return ResultFactory.Create();
		//}

		[UserAction(UserAction.Insert)]
		[HttpPost("add-document-shared")]
		public async Task<IActionResult> InsertDocumentShare(RequestDocumentShared request)
		{
			int documentId = request.DocumentID;
			await service.InsertDocumentShare(request);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Read)]
		[HttpGet("list-document-category")]
		public async Task<IActionResult> ListDocumentCategory([FromQuery] RequestDocumentCategory filter)
		{
			var result = await service.ListCategoryDocument(filter);
			return ResultFactory.Create(result);
		}

		//[UserAction(UserAction.Insert)]
		//[HttpPost("InsertWithDetail")]
		//public async Task<IActionResult> Post(RequestDocuments request)
		//{
		//	var result = await service.Insert(request);
		//	return ResultFactory.Create(result);
		//}

		[UserAction(UserAction.Insert)]
		[HttpPost("add-document-attribute")]
		public async Task<int> InsertDocumentsAttribute(RequestDocumentAttributes request)
		{
			int documentId = request.DocumentID;
			return await service.InsertDocumentAttribute(request);
		}

		[UserAction(UserAction.Read)]
		[HttpPost("get-shared-users")]
		public async Task<IActionResult> GetListSharedUsers(int DocumentId)
		{
			var result = await service.GetListSharedUsers(DocumentId);
			return ResultFactory.Create(result);
		}

		//[UserAction(UserAction.Insert)]
		//[HttpPost]
		//public async Task<IActionResult> Post(RequestDocuments request)
		//{
		//	var result = await service.InsertWithDetail(request);
		//	return ResultFactory.Create(result);
		//}

		[UserAction(UserAction.Update)]
		[HttpPut]
		public async Task<IActionResult> Put(RequestUpdateDocuments request)
		{
			var result = await service.UpdateDocument(request);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Insert)]
		[HttpPut("un-favorite")]
		public async Task<IActionResult> Unfavorite([FromQuery] int documentId)
		{
			var res = await service.UnFavorite(documentId);
			return ResultFactory.Create("OK", HttpStatusCode.OK);
		}

		[UserAction(UserAction.Update)]
		[HttpPut("add-favorite")]
		public async Task<IActionResult> AddFavorite([FromQuery] int documentId)
		{
			var res = await service.AddFavorite(documentId);
			return ResultFactory.Create("OK", HttpStatusCode.OK);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-document-logs")]
		public async Task<IActionResult> GetDocumentLog([FromQuery] RequestDocumentLog filter)
		{
			var result = await service.GetDocumentLogs(filter);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-highlight-document")]
		public async Task<IActionResult> GetHighlightDocument([FromQuery] RequestHighlightDocument filter)
		{
			var result = await service.GetHighlightDocument(filter);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-recent-document")]
		public async Task<IActionResult> GetRecentDocument([FromQuery] RequestRecentDocument filter)
		{
			var result = await service.GetRecentDocument(filter);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpPut("delete")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await service.SoftDelete(id);
			return ResultFactory.Create(result);
		}

		//
		[UserAction(UserAction.Delete)]
		[HttpPut("undelete")]
		public async Task<IActionResult> UnDelete(int id)
		{
			var result = await service.SoftUndelete(id);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("recyclebin")]
		public async Task<IActionResult> GetDeleted([FromQuery] RequestDocument filter)
		{
			var result = await service.ListDocumentDeleted(filter);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("empty-recyclebin")]
		public async Task<IActionResult> EmptyRecyclebin()
		{
			var result = await service.EmptyRecyclebin();
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
		[HttpPost("add-workflow")]
		public async Task<IActionResult> AddWorkFlow(RequestWorkflow workflow)
		{
			var result = await service.AddWorkFlow(workflow);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("delete-workflow")]
		public async Task<IActionResult> DeleteWorkflow([FromQuery]int documentId)
		{
			var result = await service.DeleteWorkFlow(documentId);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Delete)]
		[HttpPost("delete-workflow-user")]
		public async Task<IActionResult> DeleteWorkflowUser([FromBody] RequestDeleteWorkflowUser request)
		{
			var result = await service.DeleteWorkFlowUser(request);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Insert)]
		[HttpPost("send-email")]
		public async Task<IActionResult> SendMail(RequestEmailDocument request)
		{
			var result = await service.SendMailDirect(request);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Insert)]
		[HttpPost("to-dump")]
		public async Task<IActionResult> ToDump()
		{
			var result = await service.CreateTempData();
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
