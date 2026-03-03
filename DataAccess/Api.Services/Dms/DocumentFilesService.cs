using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Enum;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Services.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Syncfusion.DocIO.DLS;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;

namespace Api.Services.Masters
{
	public class DocumentFilesService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IDocumentFilesRepository repository, IDocumentsRepository documentsRepository) : BaseService(accessor, languageRepository)
	{
		public async Task<object> GetAll(RequestFilter request)
		{
			await ValidateInputRequestAsync(request);

			var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			return ObjectFlatter.Flatten(result);
		}

		public async Task<DocumentFiles> Get(int Id)
		{
			await ValidateInputAsync(Id);

			await repository.LogTransaction($"Get Document File by Id {Id}", Domain.Attributes.UserAction.Read);
			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<DocumentFiles>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);
			await repository.LogTransaction($"Get Document File page {page} limit {limit}", Domain.Attributes.UserAction.Read);
			return await repository.GetAsync(page, limit);
		}

		public async Task<List<DocumentFiles>> Upsert(List<RequestDocumentFiles> request)
		{
			await ValidateInputRequestAsync(request);

			var entities = request.CopyProperties<List<DocumentFiles>>();
			entities = entities.DistinctBy(x => x.Id).ToList();
			return await repository.UpsertManyAsync(entities);
		}

		public async Task<DocumentFiles> Insert(RequestDocumentFiles request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(request.Id);

			var entity = request.CopyProperties<DocumentFiles>();
			await repository.LogTransactionAndAuditTrail($"Insert new Document File", Domain.Attributes.UserAction.Insert, entity);
			return await repository.InsertAsync(entity);
		}

		public async Task<int> Update(RequestDocumentFiles request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(request.Id);
			var entity = request.CopyProperties<DocumentFiles>();

			await repository.LogTransactionAndAuditTrail($"Update Document File with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);
			//return await repository.UpdateAsync(entity);
			return await repository.UpdateAsync(request);
		}

		public async Task<int> UpdateFileForExcelAsync(RequestDocumentExcelFile request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(request.Id);
			//var entity = await repository.GetSingleAsync(x=> x.Id == request.Id);
			//request.CopyProperties<DocumentFiles>();

			//await repository.LogTransactionAndAuditTrail($"Update Document File with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);
			return await repository.UpdateFileForExcelAsync(request);
		}


		public async Task<bool> Delete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id, true);

			var entity = await repository.GetSingleAsync(x => x.Id == id);

			await repository.LogTransactionAndAuditTrail($"Delete existing Document File with id {id}", Domain.Attributes.UserAction.Update, entity);
			return await repository.MarkAsDeletedAsync(id);
		}

		public async Task<int> HardDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id, false);

			var entity = await repository.GetSingleAsync(x => x.Id == id);
			await repository.LogTransactionAndAuditTrail($"Hard Delete Document File with id {id}", Domain.Attributes.UserAction.Delete, entity);
			return await repository.DeleteAsync(id);
		}

		//public async Task<DocumentFiles> SoftDelete(int id)
		//{
		//	await ValidateInputAsync(id);
		//	await CheckIfExist(id);

		//	var entity = new DocumentFiles { Id = id };
		//	await repository.LogTransactionAndAuditTrail($"Soft delete existing Document File with id {id}", Domain.Attributes.UserAction.UpdateDocument, entity);
		//	return await repository.MarkAsDeletedAsync(entity);
		//}

		public async Task<bool> Undelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id, false);

			var entity = new DocumentFiles { Id = id };
			await repository.LogTransactionAndAuditTrail($"Undelete existing Document File with id {id}", Domain.Attributes.UserAction.Update, entity);
			return await repository.MarkAsUnDeletedAsync(id);
		}

		public async Task<bool> EmptyRecyclebin()
		{
			await repository.LogTransaction($"Empty recyclebin of document files", Domain.Attributes.UserAction.Delete);
			return await repository.EmptyRecyclebin(20);
		}

		private async Task CheckIfExist(int id, bool isactive = true)
		{
			var any = await repository.AnyAsync(x => x.Id == id && x.IsActive == isactive);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Id {id}");
			}
		}

		public async Task<List<ResponseDocumentFiles>> GetFilesFromDocument(int documentId)
		{
			await ValidateInputAsync(documentId);
			return await repository.GetFileFromDocumentAsync(documentId);
			//return ObjectFlatter.Flatten(result);
		}

		public async Task<List<ResponseDocumentFiles>> GetFilesFromCategory(int categoryId)
		{
			await ValidateInputAsync(categoryId);
			return await repository.GetFileFromCategorytAsync(categoryId);
			//return ObjectFlatter.Flatten(result);
		}

		public async Task<object> GetFilesRecyclebin(RequestDocumentFileList filter)
		{
			await ValidateInputRequestAsync(filter);

			ResponseDocumentFilesPagination response = await repository.GetDocumentFileRecyclebinAsync(filter.search, filter.Page, filter.Limit);
			var totalRecords = response.TotalRecord;

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, response.Limit),
				Data = response.Record
			};
		}

		public async Task<FileStreamResult> GetFileWithWatermark(int documentFileId)
		{
			await ValidateInputAsync(documentFileId);
			await CheckIfExist(documentFileId);
			ResponseDocumentFiles model = await repository.GetInfo(documentFileId);

			//var filePath = DocumentFilesHelper.GetPhysicalPathForDocumentFile(GetUser(), UserName, model.CategoryID, model.CategoryName, model.NewDocumentFileName);
			var filePath = DocumentFilesHelper.GetPhysicalPathForDocumentFile(model.OwnerUserID, model.OwnerUserName, model.CategoryID, model.CategoryName, model.NewDocumentFileName);

			MemoryStream memoryStream;

			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				memoryStream = new MemoryStream();
				fs.CopyTo(memoryStream);
			}

			memoryStream.Position = 0; // Reset position before passing

			var provider = new FileExtensionContentTypeProvider();
			if (!provider.TryGetContentType(filePath, out string contentType))
			{
				contentType = "application/octet-stream"; // Default fallback
			}

			FileStreamResult fileStreamResult = null;
			//fileStreamResult = FileDownloadProcessor.CreateFileStreamResult(filePath, model.DocumentFileName);

			if (contentType == "application/pdf")
			{
				fileStreamResult = GetFileFromPdf(memoryStream, model.WaterMark, "watermark", model.DocumentFileName);
			}
			else if (contentType.StartsWith("image"))
			{
				fileStreamResult = GetFileFromImg(memoryStream, model.WaterMark, "watermark", model.DocumentFileName);
			}
			else if (contentType == "application/octet-stream")
			{
				fileStreamResult = new FileStreamResult(memoryStream, contentType) { FileDownloadName = Path.GetFileNameWithoutExtension(model.DocumentFileName) };
			}
			else
			{
				//fileStreamResult = DocumentFilesHelper.CreateFileStreamResult(GetUser(), UserName, model.CategoryID, model.CategoryName, model.NewDocumentFileName);
				fileStreamResult = DocumentFilesHelper.CreateFileStreamResult(model.OwnerUserID, model.OwnerUserName, model.CategoryID, model.CategoryName, model.NewDocumentFileName);
			}

			await repository.LogTransactionAndAuditTrail($"Download File ID {model.Id}", Domain.Attributes.UserAction.Update, new DocumentFiles { Id = model.Id, DocumentFileName = model.DocumentFileName, NewDocumentFileName = model.NewDocumentFileName });
			await documentsRepository.LogDocumentInsert(null, model.DocumentID, LogDocumentAction.DownloadFile);
			return fileStreamResult;
		}

		//watermark
		public static SixLabors.ImageSharp.Image<Rgba32> AddWatermark(Stream imageStream, string watermarkText, float opacity = 0.3f, float fontSize = 24)
		{
			// Load the image from stream
			SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageStream);

			// Load system font collection or custom font if desired
			var fontCollection = new SixLabors.Fonts.FontCollection();
			// You can load a custom font file here if needed:
			// FontFamily fontFamily = fontCollection.Add("path/to/font.ttf");
			// For demo, use a system installed font
			SixLabors.Fonts.FontFamily fontFamily = SixLabors.Fonts.SystemFonts.Families.FirstOrDefault(); // Gets the first available system font

			SixLabors.Fonts.Font font = fontFamily.CreateFont(fontSize);

			// Measure the size of the text to be drawn
			var textOptions = new RichTextOptions(font)
			{
				HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Center,
				VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Center,
				Origin = new SixLabors.ImageSharp.PointF(image.Width / 2f, image.Height / 2f)
			};

			// Define the text color with the desired opacity (alpha)
			// Alpha is from 0 (transparent) to 1 (opaque)
			var textColor = SixLabors.ImageSharp.Color.White.WithAlpha((byte)(opacity * 255));

			// Draw the watermark text centered on the image
			image.Mutate(ctx =>
			{
				ctx.DrawText(textOptions, watermarkText, textColor);
			});

			return image;
		}

		//end here

		public static FileStreamResult GetFileFromImg(Stream ms, string waterMark, string label, string fileName = null)
		{
			using var img = System.Drawing.Image.FromStream(ms);
			using var tempImg = new Bitmap(img);
			using var graphic = Graphics.FromImage(tempImg);
			ms.Close();
			ms.Dispose();
			waterMark = waterMark == null ? " " : waterMark.Replace(@"\n", Environment.NewLine);
			return WatermarkProcess(img, graphic, tempImg, waterMark, label, fileName);
		}

		public static FileStreamResult GetFileFromPdf(Stream ms, string waterMark, string label, string fileName = null)
		{
			ms.Seek(0, SeekOrigin.Begin);
			using iText.Kernel.Pdf.PdfReader r = new(ms);
			r.SetCloseStream(false);

			waterMark = waterMark == null ? " " : waterMark.Replace(@"\n", Environment.NewLine);
			var watermarkedStream = new MemoryStream();
			using var writer = new iText.Kernel.Pdf.PdfWriter(watermarkedStream);
			writer.SetCloseStream(false);
			using var pdfDoc = new iText.Kernel.Pdf.PdfDocument(r, writer);
			ms.Close();
			ms.Dispose();
			if (!r.IsOpenedWithFullPermission())
			{
				r.Close();
				writer.Close();
				writer.Dispose();
				pdfDoc.Close();
				return new FileStreamResult(watermarkedStream, "application/pdf")
				{
					FileDownloadName = fileName ?? Guid.NewGuid().ToString()
				};
			}
			using (var doc = new iText.Layout.Document(pdfDoc))
			{
				var n = pdfDoc.GetNumberOfPages();
				var font = pdfDoc.GetDefaultFont();
				//var p1 = new Paragraph(waterMark).SetFont(font).SetFontSize(72).SetRotationAngle((float)Math.PI / 4.0f);
				var p1 = new iText.Layout.Element.Paragraph(waterMark).SetFont(font).SetFontSize(72);
				var p2 = new iText.Layout.Element.Paragraph(label).SetFont(font).SetFontSize(12);

				for (var i = 1; i <= n; i++)
				{
					var pagePage = pdfDoc.GetPage(i);
					var pageSize = pagePage.GetPageSize();
					var x = pageSize.GetWidth() / 2;
					var y = pageSize.GetHeight() / 2;
					var over = new iText.Kernel.Pdf.Canvas.PdfCanvas(pdfDoc.GetPage(i));
					var gs1 = new iText.Kernel.Pdf.Extgstate.PdfExtGState().SetFillOpacity(0.3f);
					over.SaveState();
					over.SetExtGState(gs1);
					//p1.SetMaxWidth = 
					//doc.ShowTextAligned(p1, x, y, i, TextAlignment.CENTER, VerticalAlignment.MIDDLE, (float)Math.PI / 3.33f);
					doc.ShowTextAligned(p1, x, y, i, iText.Layout.Properties.TextAlignment.CENTER, iText.Layout.Properties.VerticalAlignment.MIDDLE, 45);
					over.RestoreState();

					pageSize = pagePage.GetPageSizeWithRotation();
					pagePage.SetIgnorePageRotationForContent(true);
					x = pageSize.GetWidth() - 10;
					y = pageSize.GetHeight() * 10 / 100;
					over.SaveState();
					over.SetExtGState(gs1);
					doc.ShowTextAligned(p2, x, y, i, iText.Layout.Properties.TextAlignment.RIGHT, iText.Layout.Properties.VerticalAlignment.TOP, 0);
					over.RestoreState();
				}
			}
			writer.Close();
			writer.Dispose();
			pdfDoc.Close();
			watermarkedStream.Seek(0, SeekOrigin.Begin);
			return new FileStreamResult(watermarkedStream, "application/pdf")
			{
				FileDownloadName = fileName ?? Guid.NewGuid().ToString()
			};
		}

		private static FileStreamResult WatermarkProcess(System.Drawing.Image img, Graphics graphic, System.Drawing.Image tempImg, string waterMark, string label, string fileName = null)
		{
			var watermarkedStream = new MemoryStream();
			waterMark = waterMark == null ? " " : waterMark.Replace(@"\n", Environment.NewLine);
			//var ftype = img.GetType();
			img?.Dispose();
			System.Drawing.Font fontx = new("Times New Roman", 72, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
			System.Drawing.Font font = GetAdjustedFont(graphic, waterMark,
				fontx, tempImg.Width, 300, 10, true);

			System.Drawing.Brush brush = new System.Drawing.SolidBrush(Color.FromArgb(51, 128, 128, 128));
			graphic.TranslateTransform(tempImg.Width / 2.0f, tempImg.Height / 2.0f);
			graphic.RotateTransform(315);
			graphic.TranslateTransform(-tempImg.Width / 2.0f, -tempImg.Height / 2.0f);
			SizeF textSize = graphic.MeasureString(waterMark, font);
			PointF position = new(tempImg.Width / 2.0f - (textSize.Width / 2.0f),
				tempImg.Height / 2.0f - (textSize.Height / 2.0f));
			graphic.DrawString(waterMark, font, brush, position);
			graphic.ResetTransform();

			fontx = new System.Drawing.Font("Times New Roman", 12, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
			font = GetAdjustedFont(graphic, label,
				fontx, tempImg.Width / 4, 100, 10, true);
			textSize = graphic.MeasureString(label, font);
			position = new System.Drawing.Point(tempImg.Width - ((int)textSize.Width + 10),
				tempImg.Height - ((int)textSize.Height + 10));
			graphic.DrawString(label, font, brush, position);
			tempImg.Save(watermarkedStream, System.Drawing.Imaging.ImageFormat.Png);
			fontx.Dispose();
			font.Dispose();
			watermarkedStream.Seek(0, SeekOrigin.Begin);
			tempImg.Dispose();
			return new FileStreamResult(watermarkedStream, "image/png")
			{
				FileDownloadName = fileName ?? Guid.NewGuid().ToString()
			};
		}

		public static System.Drawing.Font GetAdjustedFont(Graphics graphicRef, string graphicString, System.Drawing.Font originalFont, int containerWidth, int maxFontSize, int minFontSize, bool smallestOnFail)
		{
			// We utilize MeasureString which we get via a control instance           
			for (int adjustedSize = maxFontSize; adjustedSize >= minFontSize; adjustedSize--)
			{
				System.Drawing.Font testFont = new(originalFont.Name, adjustedSize, originalFont.Style);

				// Test the string with the new size
				SizeF adjustedSizeNew = graphicRef.MeasureString(graphicString, testFont);

				if (containerWidth > Convert.ToInt32(adjustedSizeNew.Width))
				{
					// Good font, return it
					return testFont;
				}
			}

			// If you get here there was no fontsize that worked
			// return MinimumSize or Original?
			if (smallestOnFail)
			{
				return new System.Drawing.Font(originalFont.Name, minFontSize, originalFont.Style);
			}
			else
			{
				return originalFont;
			}
		}
		//end watermark
	}
}
