using Api.DataAccess.Models.Dms;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Enum;
using Api.Domain.Formatters;
using Api.Extensions;
using Api.Repository;
using Api.Repository.Masters;
using BitMiracle.LibTiff.Classic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using SixLabors.ImageSharp;
using SkiaSharp;
using Syncfusion.DocIO.DLS;
using Syncfusion.EJ2.DocumentEditor;
using Syncfusion.EJ2.PdfViewer;
using Syncfusion.EJ2.SpellChecker;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System.IO;
using System.Reflection;
using WDocument = Syncfusion.DocIO.DLS.WordDocument;
using WFormatType = Syncfusion.DocIO.FormatType;
using WordEditor = Syncfusion.EJ2.DocumentEditor;

namespace Api.Services.Dms
{
	public class PDFViewerService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IMemoryCache cache,
		IDocumentFilesRepository documentFileRepository, ICategoryRepository categoryRepository, IDocumentsRepository documentRepository,
		IWatermarksRepository watermarksRepository, IUserRepository userRepository)
			: BaseService(accessor, languageRepository)
	{


		public async Task<string> Load(object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			_ = Int32.TryParse(jsonObject["document"], out var idx);
			string filePath = string.Empty;

			if (jsonObject != null && jsonObject.ContainsKey("document"))
			{
				if (bool.Parse(jsonObject["isFileName"]))
				{
					if (idx > 0)
					{
						var docFile = await documentFileRepository.GetSingleAsync(x => x.Id == idx);

						var document = await documentRepository.GetSingleAsync(x => x.Id == docFile.DocumentID);
						var category = await categoryRepository.GetSingleAsync(x => x.Id == document.CategoryID);
						var user = await userRepository.GetSingleAsync(x => x.UserId == document.InsertedBy);

						filePath = DocumentFilesHelper.GetPhysicalPathForDocumentFile(user.UserId, user.UserName, category.Id, category.CategoryName, docFile.NewDocumentFileName, true);
					}
					else
					{
						//string baseDir = AppDomain.CurrentDomain.BaseDirectory;
						string resource = jsonObject["document"].ToString();
						if (resource.StartsWith("/") || resource.StartsWith("\\"))
						{
							resource = resource.Substring(1);
						}

						var setting = AppSettings.Read();
						string AppUploadFolder = setting.ApplicationInfoData.AppUploadFolder;

						string filepath = resource.Replace("resource", AppUploadFolder).Replace('/', '\\');
						//string filePathFolder = Path.Combine(baseDir, filepath);

						filePath = filepath;
					}
				}
			}

			return filePath;
		}

		public string RenderPdfPages(object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			PdfRenderer pdfviewer = new(cache);
			object jsonResult = pdfviewer.GetPage(jsonObject);
			return JsonConvert.SerializeObject(jsonResult);
		}

		public string RenderAnnotationComments(object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			PdfRenderer pdfviewer = new(cache);
			object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
			return JsonConvert.SerializeObject(jsonResult);
		}

		public string Unload(object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			PdfRenderer pdfviewer = new(cache);
			pdfviewer.ClearCache(jsonObject);
			return "Document cache is cleared";
		}

		public string RenderThumbnailImages(object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			PdfRenderer pdfviewer = new(cache);
			object result = pdfviewer.GetThumbnailImages(jsonObject);
			return JsonConvert.SerializeObject(result);
		}

		public string Bookmarks(object obj)
		{
			var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
			PdfRenderer pdfviewer = new(cache);
			object jsonResult = pdfviewer.GetBookmarks(jsonObject);
			return JsonConvert.SerializeObject(jsonResult);
		}

		public async Task<string> GetWatermark(int id)
		{
			var doc = await documentRepository.GetSingleAsync(x => x.Id == id);

			await documentRepository.LogDocumentInsert(null, id, LogDocumentAction.DownloadFile);
			//if (doc == null) return string.Empty;
			var watermark = await watermarksRepository.GetSingleAsync(x => x.Id == doc.WatermarkID);

			string waterMark = watermark.Text;
			return waterMark;
		}

		public async Task<string> GetDocumentPath(int documentId)
		{
			var document = documentRepository.GetSingleAsync(x => x.Id == documentId).Result;
			var model = await categoryRepository.GetSingleAsync(x => x.Id == document.CategoryID);
			var user = await userRepository.GetSingleAsync(x => x.UserId == document.InsertedBy);

			string uploadFilePath = DocumentFilesHelper.GetPhysicalPathForCategory(user.UserId, user.UserName, model.Id, model.CategoryName, true);
			return uploadFilePath;
		}

		public async Task<ResponseDocumentFiles> GetDocumentFile(int fileId)
		{
			var dfile = documentFileRepository.GetSingleAsync(x => x.Id == fileId).Result;
			//var document = documentRepository.GetSingleAsync(x => x.Id == dfile.DocumentID).Result;
			//var model = await categoryRepository.GetSingleAsync(x => x.Id == document.CategoryID);
			return new ResponseDocumentFiles
			{
				Id = dfile.Id,
				DocumentFileContent = dfile.DocumentFileContent,
				DocumentFileName = dfile.DocumentFileName,
				DocumentFilePath = dfile.DocumentFilePath,
				DocumentFileSize = SizeFormatter.SizeSuffix(dfile.DocumentFileSize, 2),
				NewDocumentFileName = dfile.NewDocumentFileName,
				DocumentID = dfile.DocumentID,
				DocumentType = dfile.DocumentType,

			};

		}

		//public async Task<string> ExportAnnotations(object obj)
		//{
		//	var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.ToString());
		//	PdfRenderer pdfviewer = new(cache);
		//	object jsonResult = pdfviewer.ExportAnnotations(jsonObject);
		//	return JsonConvert.SerializeObject(jsonResult);
		//}
	}
}
