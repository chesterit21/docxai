using Api.DataAccess.Models.Dms;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Dms;
using Api.Extensions;
using Api.Repository;
using Api.Repository.Masters;
using BitMiracle.LibTiff.Classic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SkiaSharp;
using Syncfusion.EJ2.DocumentEditor;
using Syncfusion.EJ2.SpellChecker;
using System.Reflection;
using WDocument = Syncfusion.DocIO.DLS.WordDocument;
using WFormatType = Syncfusion.DocIO.FormatType;
using BitMiracle.LibTiff.Classic;
using Syncfusion.EJ2.Spreadsheet;

namespace Api.Services.Dms
{
	public class Ej2SpreedsheetService(IHttpContextAccessor accessor, ILanguageRepository languageRepository,
		IDocumentFilesRepository documentFileRepository)
			: BaseService(accessor, languageRepository)
	{
		public OpenRequest Open(IFormCollection openRequest)
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
			return open;
		}

		public FileStreamResult Save([FromForm] SaveSettings saveSettings)
		{
			return Workbook.Save(saveSettings);
		}
	}
}
