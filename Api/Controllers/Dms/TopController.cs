using Microsoft.AspNetCore.Mvc;
using Api.Domain.Attributes;
using System.ComponentModel;
using Api.Services.Dms;
using Api.Domain;
using Api.Domain.EntityRequests.Dms;
using System.Net;
using Api.DataAccess.Models.Dms;
using ProtoBuf.Meta;
using MailKit.Search;
using Api.Domain.EntityRequests;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Top Feature")]
	[Menu("MnDocument")]
	[ApiController]
	[Route("[controller]")]
	public class TopController(DocumentsService service) : ControllerBase
	{
		[UserAction(UserAction.Read)]
		[HttpGet("search")]
		public async Task<IActionResult> search([FromQuery] RequestTopSearch filter)
		{
			var result = await service.SearchListDocument(filter);
			return ResultFactory.Create(result);
		}
		
		[UserAction(UserAction.Read)]
		[HttpGet("chat-ai-search")]
		public async Task<IActionResult> ChatAiSearch([FromQuery] RequestTopSearch filter)
		{
			var result = await service.SearchListDocument(filter);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("document-details-ai/{id}")]
		public async Task<IActionResult> GetDocumentDetailsAi(int id)
		{
			var result = await service.GetDocumentDetailsAi(id);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("advsearch")]
		public async Task<IActionResult> AdvSearch([FromQuery] RequestAdvSearch filter)
		{
			var result = await service.AdvSearchDocument(filter);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("itemlist")]
		public async Task<IActionResult> ItemList([FromQuery] RequestPagination filter)
		{
			var result = await service.ItemList(filter);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Insert)]
		[HttpPost("add-itemlist")]
		public async Task<IActionResult> AddItemList(RequestItemList ids)
		{
			var mid = ids.DocumentIds.ToList();
			var result = await service.AddItemList(mid);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Insert)]
		[HttpPost("delete-itemlist")]
		public async Task<IActionResult> DelteItemList(RequestItemList ids)
		{
			var mid = ids.DocumentIds.ToList();
			var result = await service.DeleteItemList(mid);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Delete)]
		[HttpPost("empty-itemlist")]
		public async Task<IActionResult> EmptyItemList()
		{
			var result = await service.EmptyItemList();
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Insert)]
		[HttpPost("send-email")]
		public async Task<IActionResult> SendMail(RequestEmailItemListDocument request)
		{
			var result = await service.SendMailMultiDirect(request);
			return ResultFactory.Create();
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
