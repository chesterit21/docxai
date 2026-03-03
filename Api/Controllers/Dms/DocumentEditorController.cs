using Api.DataAccess.Models.Dms;
using Api.Domain;
using Api.Domain.Attributes;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Api.Services.Dms;
using BitMiracle.LibTiff.Classic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.SpellChecker;
using System.ComponentModel;
using System.Diagnostics;
using static Api.Services.Dms.DocumentEditorService;
//using static Api.Services.Dms.DocumentEditorService;
//using Syncfusion.EJ2.DocumentEditor;
//using WDocument = Syncfusion.DocIO.DLS.WordDocument;
//using WFormatType = Syncfusion.DocIO.FormatType;
//using Syncfusion.EJ2.SpellChecker;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Document Editor")]
	[Route("[controller]")]
	//[Menu("MnDocument")]
	[ApiController]
	[AllowAnonymous]
	public class DocumentEditorController(DocumentEditorService service) : ControllerBase
	{

		[AcceptVerbs("Post")]
		[HttpPost]
		//[EnableCors("AllowAllOrigins")]
		//[EnableCors("cors")]
		[Route("Import")]
		public string Import(IFormCollection data)
		{
			return service.Import(data);
		}

		[AcceptVerbs("Post")]
		[HttpPost]
		//[EnableCors("AllowAllOrigins")]
		//[EnableCors("cors")]
		[Route("SpellCheck")]
		public string SpellCheck([FromBody] SpellCheckJsonData spellChecker)
		{
			try
			{
				SpellChecker spellCheck = new SpellChecker();
				spellCheck.GetSuggestions(spellChecker.LanguageID, spellChecker.TexttoCheck, spellChecker.CheckSpelling, spellChecker.CheckSuggestion, spellChecker.AddWord);
				return Newtonsoft.Json.JsonConvert.SerializeObject(spellCheck);
			}
			catch
			{
				return "{\"SpellCollection\":[],\"HasSpellingError\":false,\"Suggestions\":null}";
			}
		}
		public class SpellCheckJsonData
		{
			public int LanguageID { get; set; }
			public string TexttoCheck { get; set; }
			public bool CheckSpelling { get; set; }
			public bool CheckSuggestion { get; set; }
			public bool AddWord { get; set; }

		}

		public class UploadDocument
		{
			public string DocumentName { get; set; }
		}

		[AcceptVerbs("Post")]
		[HttpPost]
		//[EnableCors("AllowAllOrigins")]
		//[EnableCors("cors")]
		[Route("SystemClipboard")]
		public string SystemClipboard([FromBody] CustomParameter param)
		{
			return service.SystemClipboard(param);
		}

		[AcceptVerbs("Post")]
		[HttpPost]
		//[EnableCors("AllowAllOrigins")]
		//[EnableCors("cors")]
		[Route("LoadDefault")]
		public string LoadDefault()
		{
			return service.LoadDefault();
		}

		[AcceptVerbs("Post")]
		[HttpPost]
		//[EnableCors("AllowAllOrigins")]
		//[EnableCors("cors")]
		[Route("LoadDocument")]
		public async Task<string> LoadDocument([FromForm] int documentFileId)
		{
			return await service.LoadDocument(documentFileId);
		}

		[AcceptVerbs("Post")]
		[HttpPost]
		//[EnableCors("AllowAllOrigins")]
		//[EnableCors("cors")]
		[Route("Save")]
		public void Save([FromBody] SaveParameter data)
		{
			service.Save(data);
		}

		[AcceptVerbs("Post")]
		[HttpPost]
		//[EnableCors("AllowAllOrigins")]
		//[EnableCors("cors")]
		[Route("ExportSFDT")]
		public FileStreamResult ExportSFDT([FromBody] SaveParameter data)
		{
			return service.ExportSFDT(data);
		}

		[AcceptVerbs("Post")]
		[HttpPost]
		//[EnableCors("AllowAllOrigins")]
		//[EnableCors("cors")]
		[Route("Export")]
		public FileStreamResult Export(IFormCollection data)
		{
			return service.Export(data);
		}
	}
}
