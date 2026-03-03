namespace Docubase.api.Controllers.Dms
{
	using Api.Domain.EntityRequests.Dms;
	using Api.Services.Dms;
	using Api.Services.Masters;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using ProtoBuf.Meta;
	using Syncfusion.EJ2.Spreadsheet;
	using System.ComponentModel;

	/// <summary>
	/// Defines the <see cref="Ej2SpreedsheetController" />
	/// </summary>
	[DisplayName("Ej2 Spreedsheet")]
	[Route("[controller]")]
	//[Menu("MnDocument")]
	[ApiController]
	[AllowAnonymous]
	//public class Ej2SpreedsheetController(Ej2SpreedsheetService service) : ControllerBase
	public class Ej2SpreedsheetController(DocumentFilesService filesService) : ControllerBase
	{
		/// <summary>
		/// The Open
		/// </summary>
		/// <param name="openRequest">The openRequest<see cref="IFormCollection"/></param>
		/// <returns>The <see cref="IActionResult"/></returns>
		[HttpPost("Open")]
		public IActionResult Open([FromForm] IFormCollection openRequest)
		{
			OpenRequest open = new OpenRequest();
			if (openRequest.Files.Count != 0)
			{
				open.File = openRequest.Files[0];
				if (openRequest.ContainsKey("IsManualCalculationEnabled") && bool.TryParse(openRequest["IsManualCalculationEnabled"].ToString(), out bool flag))
				{
					open.IsManualCalculationEnabled = flag;
				}
			}
			open.Password = openRequest["Password"];
			if (openRequest["SheetIndex"].Count != 0)
			{
				open.SheetIndex = int.Parse(openRequest["SheetIndex"].ToString());
			}
			open.SheetPassword = openRequest["SheetPassword"];
			return Content(Workbook.Open(open));
		}

		/// <summary>
		/// The Draft
		/// </summary>
		/// <param name="saveSettings">The saveSettings<see cref="SaveSettings"/></param>
		/// <returns>The <see cref="IActionResult"/></returns>
		[HttpPost("Save")]
		public IActionResult Save([FromForm] SaveSpreadsheeRequestDTO saveSettings)
		{
			FileStreamResult excelStream = Workbook.Save(saveSettings);

			//// Example: Saving to a local folder
			//string fileName = "Updated_Document.xlsx";
			//string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", fileName);

			//using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			//{
			//	excelStream.CopyTo(file);
			//}
			var existingFileInfo = filesService.Get(saveSettings.DocumentFileId).GetAwaiter().GetResult();
			RequestDocumentExcelFile request = new RequestDocumentExcelFile()
			{
				Id = saveSettings.DocumentFileId,
				IsNewVersion = saveSettings.IsSaveAsNewVersion,
				streamOfTheFile = excelStream.FileStream,
				FileContentType = saveSettings.FileContentType.ToString()
			};

			try
			{
				var result = filesService.UpdateFileForExcelAsync(request).GetAwaiter().GetResult();

				return Ok(new
				{
					success = true,
					message = "Document updated successfully",
					documentId = request.Id,
					versionCreated = request.IsNewVersion
				});
			}
			catch (Exception ex)
			{
				return BadRequest(new { success = false, message = ex.Message });
			}
			//return Workbook.Save(saveSettings);
		}
	}

	public class SaveSpreadsheeRequestDTO : SaveSettings
	{
		public int DocumentFileId { get; set; }
		public bool IsSaveAsNewVersion { get; set; }
	}
}
