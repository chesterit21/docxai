using Api.Extensions;
using Api.Services.Dms;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MathNet.Numerics;
using Microsoft.Win32;
using MimeKit.Encodings;
using NPOI.SS.Formula.Functions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Syncfusion.EJ2.Grids;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using static SkiaSharp.SKPath;
using static System.Environment;
using Cell = DocumentFormat.OpenXml.Spreadsheet.Cell;
using MyPdfPage = UglyToad.PdfPig.Content.Page;
using Row = DocumentFormat.OpenXml.Spreadsheet.Row;

namespace Api.Services
{
	public class FileProcessor
	{
		private static readonly string TessDataPath = "tessdata"; // Pastikan path ini benar
		private const string LANGUAGE = "ind+eng";
		private const string CHAR_WHITELIST = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789:-";

		public static async Task<string> ExtractTextAsync(string filePath)
		{
			var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
			return fileExtension switch
			{
				".txt" => await File.ReadAllTextAsync(filePath),
				".pdf" => ExtractTextFromPdf(filePath),
				".doc" or ".docx" => ExtractTextFromWord(filePath),
				".xls" or ".xlsx" => ExtractTextFromExcel(filePath),
				".ppt" or ".pptx" => ExtractTextFromPowerPoint(filePath),
				//".jpg" or ".jpeg" or ".png" => await ExtractTextFromImageAsync(filePath),
				_ => throw new NotSupportedException($"Tipe file '{fileExtension}' tidak didukung.")
			};
		}

		private static string ExtractTextFromPdf(string filePath)
		{
			string pdfContent = ExtractTextWithOcr(filePath);
			return pdfContent;
		}

		#region OCR using tesseract net sdk
		
		/// <summary>
		/// Extracts images from a page and attempts to run Tesseract OCR on them.
		/// </summary>
		private static string RunOcrOnPageImages(MyPdfPage page)
		{
			var images = page.GetImages();
			var ocrBuilder = new StringBuilder();

			if (!images.Any())
			{
				return string.Empty; // Return empty string if no images, not a message
			}

			foreach (var image in images)
			{
				byte[] imageBytes = image.RawBytes.ToArray();

				// 1. Convert the image bytes into a MemoryStream
				using var stream = new MemoryStream(imageBytes);

				// 2. Call the dedicated in-memory OCR function (ProcessingOCR)
				// We use the language and whitelist constants defined at the class level.
				string ocrResult = ProcessingOCR(stream, LANGUAGE, CHAR_WHITELIST);

				if (!string.IsNullOrWhiteSpace(ocrResult))
				{
					// Add the result directly without too many headers, as it's part of the content stream
					ocrBuilder.AppendLine(ocrResult);
				}
			}

			return ocrBuilder.ToString();
		}

		// ------------------------------------------------------------------------
		// --- OCR Processing and Preprocessing Logic ---
		// ------------------------------------------------------------------------

		/// <summary>
		/// Processes an image Stream using Tesseract in-memory, eliminating the need for
		/// temporary files and external process management.
		/// </summary>
		/// <param name="fileStrm">The input image stream.</param>
		/// <param name="language">The Tesseract language(s) to use (e.g., "ind+eng").</param>
		/// <param name="charWhitelist">Optional: Characters Tesseract should focus on.</param>
		/// <returns>The extracted text string.</returns>
		public static string ProcessingOCR(
			Stream fileStrm,
			string language,
			string charWhitelist)
		{
			if (fileStrm == null || fileStrm.Length == 0)
			{
				return string.Empty;
			}

			string output = string.Empty;

			try
			{
				// 1. Load the stream into a C# Bitmap (requires System.Drawing.Common)
				//using var originalBitmap = new Bitmap(fileStrm);
				// This replaces the unreliable 'new Bitmap(fileStrm)' call.
				using var originalBitmap = LoadStreamToBitmap(fileStrm);
				if(originalBitmap == null)
					return string.Empty;
				// 2. Perform NEW IN-MEMORY PREPROCESSING 
				using var bitmapToProcess = OcrImagePreprocessing.PreprocessImageInMemory(originalBitmap);

				string baseDir = AppDomain.CurrentDomain.BaseDirectory;
				string TESSDATA_PATH = Path.Combine(baseDir, "tessdata");
				// 3. Initialize the Tesseract Engine using the basic constructor
				using var engine = new TesseractEngine(TESSDATA_PATH, language, EngineMode.Default);

				// 4. Set Tesseract configuration variables (e.g., the character whitelist)
				if (!string.IsNullOrEmpty(charWhitelist))
				{
					engine.SetVariable("tessedit_char_whitelist", charWhitelist);
				}

				// 5. Convert the PROCESSED Bitmap to a Pix object
				using var pix = PixConverter.ToPix(bitmapToProcess);
				//using Pix pix = bitmapToProcess.ToPix();

				// 6. Process the image in memory (using PSM 6 for SingleBlock)
				using (var page = engine.Process(pix, PageSegMode.SingleBlock))
				{
					output = page.GetText();
				}
			}
			catch (ArgumentException)
			{
				// Handle common Bitmap loading failures (e.g., unsupported image format in stream)
				return string.Empty;
			}
			catch (Exception ex)
			{
				// In a production environment, you might log this error here.
				// Console.WriteLine($"Tesseract OCR Error: {ex.Message}");
				return string.Empty; // Return empty string on failure to keep the combined text clean
			}

			// 7. Clean up whitespace and return
			if (!string.IsNullOrWhiteSpace(output))
			{
				output = Regex.Replace(output, @"\s+", " ").Trim();
			}

			return output;
		}

		/// <summary>
		/// Robustly loads an image stream into a System.Drawing.Bitmap using ImageSharp as an intermediary.
		/// </summary>
		/// <param name="imageStream">The raw image data stream from PdfPig.</param>
		/// <returns>A valid System.Drawing.Bitmap or null if loading fails.</returns>
		private static Bitmap LoadStreamToBitmap(Stream imageStream)
		{
			try
			{
				// Rewind the stream just in case
				imageStream.Seek(0, SeekOrigin.Begin);

				// 1. Load the raw data using ImageSharp's robust loader
				using (var imageSharp = SixLabors.ImageSharp.Image.Load<Rgba32>(imageStream))
				{
					// 2. Save the ImageSharp object back into a memory stream, explicitly as PNG
					using var outputStream = new MemoryStream();
					imageSharp.Save(outputStream, new PngEncoder());
					outputStream.Seek(0, SeekOrigin.Begin);

					// 3. System.Drawing.Bitmap can reliably load PNG from a stream
					return new Bitmap(outputStream);
				}
			}
			catch (Exception)
			{
				// Console.WriteLine($"ImageSharp failed to load image stream. Skipping.");
				return null; // Return null on any loading failure
			}
		}

		/// <summary>
		/// Extracts text from a PDF by combining the embedded text layer and any text found via image OCR.
		/// </summary>
		/// <param name="filePath">The path to the PDF file.</param>
		/// <returns>Extracted text from the PDF, including OCR results.</returns>
		public static string ExtractTextWithOcr(string filePath)
		{
			using var document = PdfDocument.Open(filePath);
			var textBuilder = new StringBuilder();
			var wordExtractor = NearestNeighbourWordExtractor.Instance;

			for (int i = 1; i <= document.NumberOfPages; i++)
			{
				MyPdfPage page = document.GetPage(i);
				//textBuilder.Append($"[PAGE {i} - COMBINED EXTRACTION]\n");

				// 1. Always attempt to extract the embedded text (Text Layer)
				var words = wordExtractor.GetWords(page.Letters);
				string extractedText = string.Join(" ", words.Select(w => w.Text)).Trim();

				// 2. Always run OCR on images (OCR Layer)
				string ocrText = RunOcrOnPageImages(page).Trim();

				// 3. Combine the results

				// Append the embedded text first (it's usually the most accurate)
				if (!string.IsNullOrWhiteSpace(extractedText))
				{
					textBuilder.Append(extractedText);
					textBuilder.Append("\n");
				}

				// Append the OCR results
				if (!string.IsNullOrWhiteSpace(ocrText))
				{
					// If embedded text was present, add a clear separator
					if (!string.IsNullOrWhiteSpace(extractedText))
					{
						textBuilder.Append("\n");
					}
					textBuilder.Append(ocrText);
					textBuilder.Append("\n");
				}

				// Handle the case where the page is truly empty
				if (string.IsNullOrWhiteSpace(extractedText) && string.IsNullOrWhiteSpace(ocrText))
				{
					textBuilder.Append("[No embedded text or OCR results found.]\n");
				}

				textBuilder.Append("\n");
			}

			return textBuilder.ToString().Trim();
		}

		/// <summary>
		/// Processes an image Stream using Tesseract in-memory, eliminating the need for
		/// temporary files and external process management.
		/// </summary>
		/// <param name="fileStrm">The input image stream.</param>
		/// <param name="language">The Tesseract language(s) to use (e.g., "ind+eng").</param>
		/// <param name="charWhitelist">Optional: Characters Tesseract should focus on.</param>
		/// <returns>The extracted text string.</returns>
		/// 
		/// bisa nanti digunakan untu pengganti method ExtractTextFromImageAsync
		public static string ProcessingOCR(string filePath)
		{
			string output = string.Empty;

			try
			{
				MemoryStream fileStrm;

				using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					fileStrm = new MemoryStream();
					fs.CopyTo(fileStrm);
				}

				fileStrm.Position = 0;

				// 1. Load the stream into a C# Bitmap (requires System.Drawing.Common)
				//using var originalBitmap = new Bitmap(fileStrm);
				// This replaces the unreliable 'new Bitmap(fileStrm)' call.
				using var originalBitmap = LoadStreamToBitmap(fileStrm);
				if (originalBitmap == null)
					return string.Empty;
				// 2. Perform NEW IN-MEMORY PREPROCESSING 
				using var bitmapToProcess = OcrImagePreprocessing.PreprocessImageInMemory(originalBitmap);

				string baseDir = AppDomain.CurrentDomain.BaseDirectory;
				string TESSDATA_PATH = Path.Combine(baseDir, "tessdata");
				// 3. Initialize the Tesseract Engine using the basic constructor
				using var engine = new TesseractEngine(TESSDATA_PATH, LANGUAGE, EngineMode.Default);

				// 4. Set Tesseract configuration variables (e.g., the character whitelist)
				engine.SetVariable("tessedit_char_whitelist", CHAR_WHITELIST);

				// 5. Convert the PROCESSED Bitmap to a Pix object
				using var pix = PixConverter.ToPix(bitmapToProcess);
				//using Pix pix = bitmapToProcess.ToPix();

				// 6. Process the image in memory (using PSM 6 for SingleBlock)
				using (var page = engine.Process(pix, PageSegMode.SingleBlock))
				{
					output = page.GetText();
				}
			}
			catch (ArgumentException)
			{
				// Handle common Bitmap loading failures (e.g., unsupported image format in stream)
				return string.Empty;
			}
			catch (Exception ex)
			{
				// In a production environment, you might log this error here.
				// Console.WriteLine($"Tesseract OCR Error: {ex.Message}");
				return string.Empty; // Return empty string on failure to keep the combined text clean
			}

			// 7. Clean up whitespace and return
			if (!string.IsNullOrWhiteSpace(output))
			{
				output = Regex.Replace(output, @"\s+", " ").Trim();
			}

			return output;
		}

		#endregion OCR using netsdk

		private static string ExtractTextFromWord(string filePath)
		{
			using var document = WordprocessingDocument.Open(filePath, false);
			return document.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
		}

		private static string ExtractTextFromExcel(string filePath)
		{
			using var document = SpreadsheetDocument.Open(filePath, false);
			var textBuilder = new System.Text.StringBuilder();
			var workbookPart = document.WorkbookPart;
			if (workbookPart == null) return string.Empty;

			var stringTablePart = workbookPart.SharedStringTablePart;
			var stringTable = stringTablePart?.SharedStringTable;

			foreach (var worksheetPart in workbookPart.WorksheetParts)
			{
				var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
				if (sheetData == null) continue;

				foreach (var row in sheetData.Elements<Row>())
				{
					foreach (var cell in row.Elements<Cell>())
					{
						if (cell.CellValue != null)
						{
							if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
							{
								var sharedStringIndex = int.Parse(cell.CellValue.InnerText);
								textBuilder.Append(stringTable?.ElementAt(sharedStringIndex).InnerText);
							}
							else
							{
								textBuilder.Append(cell.CellValue.InnerText);
							}
							textBuilder.Append(" ");
						}
					}
				}
			}
			return textBuilder.ToString();
		}

		private static string ExtractTextFromPowerPoint(string filePath)
		{
			using var document = PresentationDocument.Open(filePath, false);
			var textBuilder = new System.Text.StringBuilder();
			var presentationPart = document.PresentationPart;
			if (presentationPart == null) return string.Empty;

			foreach (var slideId in presentationPart.Presentation.SlideIdList.Elements<SlideId>())
			{
				var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId!);
				foreach (var shape in slidePart.Slide.Descendants<Shape>())
				{
					textBuilder.Append(shape.InnerText);
					textBuilder.Append(" ");
				}
			}
			return textBuilder.ToString();
		}

		private static async Task<string> ExtractTextFromImageAsync(string filePath)
		{
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			string TESSDATA_PATH = Path.Combine(baseDir, "tessdata");
			using var engine = new TesseractEngine(TESSDATA_PATH, "eng", EngineMode.Default);
			using var img = Pix.LoadFromFile(filePath);
			using var page = engine.Process(img);
			return await Task.FromResult(page.GetText());
		}
	}
}
