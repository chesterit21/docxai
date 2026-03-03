using Api.DataAccess.Models.Dms;
using Api.Domain;
using Api.Domain.Attributes;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Api.Repository.Masters;
using Api.Services.Dms;
using BitMiracle.LibTiff.Classic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SkiaSharp;
using Syncfusion.DocIO.DLS;
using Syncfusion.Drawing;
using Syncfusion.EJ2.PdfViewer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json.Nodes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Docubase.api.Controllers.Dms
{
    [DisplayName("PDF Viewer")]
    [Route("[controller]")]    
    [ApiController]
	[AllowAnonymous]
    public class PdfViewerController(PDFViewerService service, DocumentProcessService documentProcessService, IMemoryCache _cache) : ControllerBase
    {
		//private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

		[HttpPost("Load")]
		public async Task<IActionResult> Load([FromBody] object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			bool isFileName = jsonObject.ContainsKey("isFileName") && bool.Parse(jsonObject["isFileName"]);
			var pdfRenderer = new PdfRenderer(_cache, 30); // 30 minutes sliding expiration
			MemoryStream stream = null;
			if (isFileName)
			{
				// Get the actual file path from your service using the documentKey
				string filepath = await service.Load(obj);
				if (!System.IO.File.Exists(filepath))
					return NotFound("File not found");

				byte[] bytes = System.IO.File.ReadAllBytes(filepath);
				stream = new MemoryStream(bytes);

				// Read entire file into MemoryStream
				//using (var fileStream = System.IO.File.OpenRead(filepath))
				//{
				//	stream = new MemoryStream();
				//	await fileStream.CopyToAsync(stream);
				//	stream.Position = 0; // Reset stream position
				//}
			}
			else
			{
				// If document is base64 or other format, handle accordingly
				// For now, return bad request
				return BadRequest("Only file name loading is supported");
			}
			// Pass the stream and the jsonObject (which contains the cache key 'document')
			//var jsonResult = pdfRenderer.Load(stream, jsonObject);
			var jsonResult = pdfRenderer.Load(stream, jsonObject);
			var objx = JsonConvert.SerializeObject(jsonResult);
			//return Content(objx, "application/json");
			return Content(objx);

			//var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			//_ = Int32.TryParse(jsonObject["document"], out var idx);

			//var pdfviewer = new PdfRenderer(_cache, 30);
			//var stream = new MemoryStream();

			//if (jsonObject != null && jsonObject.ContainsKey("document"))
			//{
			//	string filepath = await service.Load(obj);

			//	if (bool.Parse(jsonObject["isFileName"]))
			//	{
			//		using Stream fileStream = System.IO.File.OpenRead(filepath);
			//		stream = new MemoryStream(fileStream.ReadByte());
			//	}
			//}


			//var jsonResult = pdfviewer.Load(stream, jsonObject);
			//var objx = JsonConvert.SerializeObject(jsonResult);
			//return Content(objx.ToString());
		}

		[HttpPost("RenderPdfPages")]
		public IActionResult RenderPdfPages([FromBody] object obj)
		{
			string result = service.RenderPdfPages(obj);
			return Content(result);
		}

		[HttpPost("RenderAnnotationComments")]
		public IActionResult RenderAnnotationComments([FromBody] object obj)
		{
			string result = service.RenderAnnotationComments(obj);
			return Content(result);
		}

		[HttpPost("Unload")]
		public IActionResult Unload([FromBody] object obj)
		{
			string result = service.Unload(obj);
			return Content(result);
		}

		[HttpPost("RenderThumbnailImages")]
		public IActionResult RenderThumbnailImages([FromBody] object obj)
		{
			string result = service.RenderThumbnailImages(obj);
			return Content(result);
		}

		[HttpPost("Bookmarks")]
		public IActionResult Bookmarks([FromBody] object obj)
		{
			string result = service.Bookmarks(obj);
			return Content(result);
		}

		[HttpPost("Download")]
		public async Task<IActionResult> Download([FromBody] object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			if (!jsonObject.ContainsKey("documentId")) return BadRequest(500);

			_ = int.TryParse(jsonObject["documentId"], out var idx);
			string watermark = await service.GetWatermark(idx);

			PdfRenderer pdfviewer = new(_cache);
			string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
			string base64String = documentBase.Split(new[] { "data:application/pdf;base64," }, StringSplitOptions.None)[1];
			if (string.IsNullOrEmpty(base64String)) return BadRequest(500);

			if (!string.IsNullOrEmpty(watermark))
			{
				byte[] bytes = Convert.FromBase64String(base64String);
				await using var streamsss = new MemoryStream(bytes);
				PdfLoadedDocument loadedDocument = new(streamsss);
				foreach (PdfPageBase i in loadedDocument.Pages)
				{
					PdfGraphics graphics = i.Graphics;
					PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 72);
					PdfGraphicsState state = graphics.Save();
					graphics.SetTransparency(0.25f);
					graphics.RotateTransform(-40);
					graphics.DrawString(watermark, font, PdfPens.Red, PdfBrushes.Red, new PointF(-150, 450));
				}
				MemoryStream stream = new();
				loadedDocument.Save(stream);
				stream.Position = 0;
				loadedDocument.Close(true);
				documentBase = Convert.ToBase64String(stream.ToArray());
				documentBase = "data:application/pdf;base64," + documentBase;
				await stream.DisposeAsync();
			}

			return Content(documentBase);
		}

		[HttpPost("PrintImages")]
		public IActionResult PrintImages([FromBody] object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			using PdfRenderer pdfviewer = new(_cache);
			object pageImage = pdfviewer.GetPrintImage(jsonObject);
			return Content(JsonConvert.SerializeObject(pageImage));
		}

		[HttpPost("ExportAnnotations")]
		public IActionResult ExportAnnotations([FromBody] object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			PdfRenderer pdfviewer = new(_cache);
			string result = pdfviewer.ExportAnnotation(jsonObject);
			return Content(result, "text/plain", System.Text.Encoding.UTF8);
		}

		[HttpPost("ImportAnnotations")]
		public IActionResult ImportAnnotations([FromBody] object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			PdfRenderer pdfviewer = new(_cache);
			string jsonResult;
			if (jsonObject != null && jsonObject.ContainsKey("fileName"))
			{
				//string documentPath = GetDocumentPath(jsonObject["fileName"]);
				string documentPath = "";//belum dilengkapi
				if (!string.IsNullOrEmpty(documentPath))
				{
					jsonResult = System.IO.File.ReadAllText(documentPath);
				}
				else
				{
					return Content(jsonObject["document"] + " is not found", "text/plain", System.Text.Encoding.UTF8);
				}
			}
			else
			{
				string extension = Path.GetExtension(jsonObject?["importedData"]);
				object JsonResult;
				if (extension != ".xfdf")
				{
					JsonResult = pdfviewer.ImportAnnotation(jsonObject);
					return Content(JsonConvert.SerializeObject(JsonResult), "text/plain", System.Text.Encoding.UTF8);
				}

				//string documentPath = service.GetDocumentPath((int)jsonObject?["importedData"]);
				string documentPath = "";//belum dilengkapi
				if (!string.IsNullOrEmpty(documentPath))
				{
					byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
					jsonObject["importedData"] = Convert.ToBase64String(bytes);
					JsonResult = pdfviewer.ImportAnnotation(jsonObject);
					return Content(JsonConvert.SerializeObject(JsonResult), "text/plain", System.Text.Encoding.UTF8);
				}

				return Content(jsonObject?["document"] + " is not found");
			}
			return Content(jsonResult, "text/plain", System.Text.Encoding.UTF8);
		}

		[HttpPost("RenderPdfTexts")]
		public IActionResult RenderPdfTexts([FromBody] Dictionary<string, string> jsonObject)
		{
			//var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			//PdfRenderer pdfviewer = new(_cache);
			//object result = pdfviewer.GetDocumentText(jsonObject);
			//return Content(JsonConvert.SerializeObject(result));

			PdfRenderer pdfviewer = new PdfRenderer(_cache);
			object jsonResult = pdfviewer.GetDocumentText(jsonObject);
			return Content(JsonConvert.SerializeObject(jsonResult));
		}

		[HttpPost("ExportFormFields")]
		public IActionResult ExportFormFields([FromBody] object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			PdfRenderer pdfviewer = new(_cache);
			string jsonResult = pdfviewer.ExportFormFields(jsonObject);
			return Content(jsonResult, "text/plain", System.Text.Encoding.UTF8);
		}

		[HttpPost("ImportFormFields")]
		public async Task<IActionResult> ImportFormFields([FromBody] object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			PdfRenderer pdfviewer = new(_cache);
			//jsonObject["data"] = GetDocumentPath(jsonObject["data"]);
			_ = int.TryParse(jsonObject["data"], out var docId);
			jsonObject["data"] = await service.GetDocumentPath(docId);
			object jsonResult = pdfviewer.ImportFormFields(jsonObject);
			return Content(JsonConvert.SerializeObject(jsonResult));
		}

		//private string GetDocumentPath(string document)
		//{
		//	string documentPath = string.Empty;
		//	if (!System.IO.File.Exists(document))
		//	{
		//		string basePath = _webHostEnvironment.ContentRootPath;
		//		var dataPath = basePath + @"/Data/";
		//		if (System.IO.File.Exists(dataPath + document))
		//			documentPath = dataPath + document;
		//	}
		//	else
		//	{
		//		documentPath = document;
		//	}
		//	return documentPath;
		//}

		[HttpPost("Save/Download")]
		public async Task<object> Save([FromBody] object obj)
		{
			try
			{
				var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
				if (!jsonObject.ContainsKey("documentId")) return BadRequest(500);

				PdfRenderer pdfviewer = new(_cache);
				//return BadRequest(500);
				string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
				string base64String = documentBase.Split(new[] { "data:application/pdf;base64," }, StringSplitOptions.None)[1];
				if (string.IsNullOrEmpty(base64String)) return BadRequest(500);

				_ = Int32.TryParse(jsonObject["documentId"], out var idx);
				var file = await service.GetDocumentFile(idx);
				if (file == null) return NotFound();
				string pathfilebycategory = await service.GetDocumentPath(file.DocumentID);


				var directoryName = pathfilebycategory;
				var dNow = DateTime.Now.ToString("ddMMyyyyHHmm");
				var fName = $"{Path.GetFileNameWithoutExtension(file.NewDocumentFileName)}-{dNow}{Path.GetExtension(file.NewDocumentFileName)}";
				bool isGood = true;
				if (directoryName != null)
				{
					var fPath = Path.Combine(directoryName, fName);
					byte[] fff = Convert.FromBase64String(base64String);
					var isSave = await documentProcessService.CreateFiles(fff, null, null, null,
						fPath, null, null, new CancellationToken()).ConfigureAwait(false);
					if (!isSave) return BadRequest(500);
					//var newDoc = new DocumentFile
					//{
					//	DocFileName = fName,
					//	DocPath = fPath,
					//	DocSize = fff.Length,
					//	DocumentId = docId.DocumentId,
					//	IsDeleted = false,
					//	IsBulkUpload = false,
					//	DocType = docId.DocType,
					//	CreateBy = User.Identity?.Name,
					//	CreateDate = DateTime.Now
					//};
					//await _context.DocumentFiles.AddAsync(newDoc).ConfigureAwait(false);

					//var needApprove = await _context.DocumentCategories.Include(d => d.Document).FirstOrDefaultAsync(r => r.Category.NeedApproval && r.DocumentId == docId.DocumentId).ConfigureAwait(false);
					//if (needApprove != null)
					//{
					//	needApprove.Document.IsApproved = false;
					//	_context.Documents.Update(needApprove.Document);
					//	var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
					//	isGood = await ApprovalAsync(docId.DocumentId ?? Guid.Empty, userId, needApprove.CategoryId, 1, $"Edit file: {newDoc.DocFileName.TrimEnd('.')}").ConfigureAwait(false);
					//}
				}
				pdfviewer.ClearCache(jsonObject);
				if (isGood)
					return Ok();

				return BadRequest("1");
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
