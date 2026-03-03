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
    [DisplayName("Documents Files")]
    [Route("[controller]")]
    [Menu("MnDocument")]
    [ApiController]
    public class DocumentFilesController(DocumentFilesService service) : ControllerBase
    {
		[UserAction(UserAction.Read)]
		[HttpGet("get-document-files")]
        public async Task<IActionResult> GetDocumentFiles(int documentId)
        {
            var result = await service.GetFilesFromDocument(documentId);
            return ResultFactory.Create(result);
        }

		[UserAction(UserAction.Read)]
		[HttpGet("get-category-files")]
		public async Task<IActionResult> GetCategoryFiles(int categoryId)
		{
			var result = await service.GetFilesFromDocument(categoryId);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Update)]
		[HttpPut]
        public async Task<IActionResult> Put(RequestDocumentFiles request)
        {
            var result = await service.Update(request);
            return ResultFactory.Create(result);
        }

		[UserAction(UserAction.Delete)]
		[HttpPut("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await service.Delete(id);
            return ResultFactory.Create(result);
        }

		[UserAction(UserAction.Update)]
		[HttpPut("undelete")]
		public async Task<IActionResult> UnDelete(int id)
		{
			var result = await service.Undelete(id);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Read)]
		[HttpGet("recyclebin")]
		public async Task<IActionResult> GetDeleted([FromQuery] RequestDocumentFileList filter)
		{
			var result = await service.GetFilesRecyclebin(filter);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("hard-delete")]
		public async Task<IActionResult> HardDelete(int id)
		{
			var result = await service.HardDelete(id);
			return ResultFactory.Create();
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("empty-recyclebin")]
		public async Task<IActionResult> EmptyRecyclebin()
		{
			var result = await service.EmptyRecyclebin();
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("download")]
		public async Task<IActionResult> DownloadWithWatermark(int documentfileId)
		{
			return await service.GetFileWithWatermark(documentfileId);

			//try
			//{
			//	// DocumentFilesHelper.CreateFileStreamResultAsync will call the loader to get metadata
			//	var fileResult = await DocumentFilesHelper.CreateFileStreamResultAsync(id, async (fileId) =>
			//	{
			//		// loader -> fetch metadata from repository
			//		// repository.GetInfo returns ResponseDocumentFiles (adjust if your repo method name differs)
			//		return await _repo.GetInfo(fileId);
			//	}).ConfigureAwait(false);

			//	return fileResult;
			//}
			//catch (FileNotFoundException fnf)
			//{
			//	_logger.LogWarning(fnf, "Download requested but file not found for id {Id}", id);
			//	return NotFound(new { message = "File not found" });
			//}
		}
	}
}
