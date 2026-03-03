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

namespace Api.Services.Dms
{
	public class DocumentEditorService(IHttpContextAccessor accessor, ILanguageRepository languageRepository,
		IDocumentFilesRepository documentFileRepository)
			: BaseService(accessor, languageRepository)
	{

		public string Import(IFormCollection data)
		{
			if (data.Files.Count == 0)
				return null;
			Stream stream = new MemoryStream();
			IFormFile file = data.Files[0];
			int index = file.FileName.LastIndexOf('.');
			string type = index > -1 && index < file.FileName.Length - 1 ?
				file.FileName.Substring(index) : ".docx";
			file.CopyTo(stream);
			stream.Position = 0;

			//Hooks MetafileImageParsed event.
			WordDocument.MetafileImageParsed += OnMetafileImageParsed;
			WordDocument document = WordDocument.Load(stream, GetFormatType(type.ToLower()));
			//Unhooks MetafileImageParsed event.
			WordDocument.MetafileImageParsed -= OnMetafileImageParsed;

			string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
			document.Dispose();
			return json;
		}

		private static void OnMetafileImageParsed(object sender, MetafileImageParsedEventArgs args)
		{
			if (args.IsMetafile)
			{
				//MetaFile image conversion(EMF and WMF)
				//You can write your own method definition for converting metafile to raster image using any third-party image converter.
				args.ImageStream = ConvertMetafileToRasterImage(args.MetafileStream);
			}
			else
			{
				//TIFF image conversion
				args.ImageStream = TiffToPNG(args.MetafileStream);

			}
		}

		private static MemoryStream TiffToPNG(Stream tiffStream)
		{
			MemoryStream imageStream = new MemoryStream();
			using (Tiff tif = Tiff.ClientOpen("in-memory", "r", tiffStream, new TiffStream()))
			{
				// Find the width and height of the image
				FieldValue[] value = tif.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGEWIDTH);
				int width = value[0].ToInt();

				value = tif.GetField(BitMiracle.LibTiff.Classic.TiffTag.IMAGELENGTH);
				int height = value[0].ToInt();

				// Read the image into the memory buffer
				int[] raster = new int[height * width];
				if (!tif.ReadRGBAImage(width, height, raster))
				{
					throw new Exception("Could not read image");
				}

				// Create a bitmap image using SkiaSharp.
				using (SKBitmap sKBitmap = new SKBitmap(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
				{
					// Convert a RGBA value to byte array.
					byte[] bitmapData = new byte[sKBitmap.RowBytes * sKBitmap.Height];
					for (int y = 0; y < sKBitmap.Height; y++)
					{
						int rasterOffset = y * sKBitmap.Width;
						int bitsOffset = (sKBitmap.Height - y - 1) * sKBitmap.RowBytes;

						for (int x = 0; x < sKBitmap.Width; x++)
						{
							int rgba = raster[rasterOffset++];
							bitmapData[bitsOffset++] = (byte)((rgba >> 16) & 0xff);
							bitmapData[bitsOffset++] = (byte)((rgba >> 8) & 0xff);
							bitmapData[bitsOffset++] = (byte)(rgba & 0xff);
							bitmapData[bitsOffset++] = (byte)((rgba >> 24) & 0xff);
						}
					}

					// Convert a byte array to SKColor array.
					SKColor[] sKColor = new SKColor[bitmapData.Length / 4];
					int index = 0;
					for (int i = 0; i < bitmapData.Length; i++)
					{
						sKColor[index] = new SKColor(bitmapData[i + 2], bitmapData[i + 1], bitmapData[i], bitmapData[i + 3]);
						i += 3;
						index += 1;
					}

					// Set the SKColor array to SKBitmap.
					sKBitmap.Pixels = sKColor;

					// Draft the SKBitmap to PNG image stream.
					sKBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(imageStream);
					imageStream.Flush();
				}
			}
			return imageStream;
		}

		internal static FormatType GetFormatType(string format)
		{
			if (string.IsNullOrEmpty(format))
				throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
			switch (format.ToLower())
			{
				case ".dotx":
				case ".docx":
				case ".docm":
				case ".dotm":
					return FormatType.Docx;
				case ".dot":
				case ".doc":
					return FormatType.Doc;
				case ".rtf":
					return FormatType.Rtf;
				case ".txt":
					return FormatType.Txt;
				case ".xml":
					return FormatType.WordML;
				case ".html":
					return FormatType.Html;
				default:
					throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
			}
		}

		private static Stream ConvertMetafileToRasterImage(Stream ImageStream)
		{
			//Here we are loading a default raster image as fallback.
			Stream imgStream = GetManifestResourceStream("ImageNotFound.jpg");
			return imgStream;
			//To do : Write your own logic for converting metafile to raster image using any third-party image converter(Syncfusion doesn't provide any image converter).
		}

		private static Stream GetManifestResourceStream(string fileName)
		{
			System.Reflection.Assembly execAssembly = typeof(WDocument).Assembly;
			string[] resourceNames = execAssembly.GetManifestResourceNames();
			foreach (string resourceName in resourceNames)
			{
				if (resourceName.EndsWith("." + fileName))
				{
					fileName = resourceName;
					break;
				}
			}
			return execAssembly.GetManifestResourceStream(fileName);
		}

		public class CustomParameter
		{
			public string content { get; set; }
			public string type { get; set; }
		}

		public string SystemClipboard(CustomParameter param)
		{
			if (param.content != null && param.content != "")
			{
				try
				{
					//Hooks MetafileImageParsed event.
					WordDocument.MetafileImageParsed += OnMetafileImageParsed;
					WordDocument document = WordDocument.LoadString(param.content, GetFormatType(param.type.ToLower()));
					//Unhooks MetafileImageParsed event.
					WordDocument.MetafileImageParsed -= OnMetafileImageParsed;
					string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
					document.Dispose();
					return json;
				}
				catch (Exception)
				{
					return "";
				}
			}
			return "";
		}

		public string LoadDefault()
		{
			Stream stream = System.IO.File.OpenRead("App_Data/GettingStarted.docx");
			stream.Position = 0;

			WordDocument document = WordDocument.Load(stream, FormatType.Docx);
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
			document.Dispose();
			return json;
		}

		public async Task<string> LoadDocument(int documentFileId)
		{
			await ValidateInputAsync(documentFileId);
			await CheckIfExist(documentFileId);

			var docFile = await documentFileRepository.GetSingleAsync(x => x.Id == documentFileId);
			var documentPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "upload", docFile.NewDocumentFileName);

			Stream stream = null;
			if (System.IO.File.Exists(documentPath))
			{
				byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
				stream = new MemoryStream(bytes);
			}
			//else
			//{
			//	bool result = Uri.TryCreate(uploadDocument.DocumentName, UriKind.Absolute, out Uri uriResult)
			//		&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
			//	if (result)
			//	{
			//		stream = GetDocumentFromURL(uploadDocument.DocumentName).Result;
			//		if (stream != null)
			//			stream.Position = 0;
			//	}
			//}
			WordDocument document = WordDocument.Load(stream, FormatType.Docx);
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
			document.Dispose();
			return json;
		}

		private async Task CheckIfExist(int id)
		{
			var any = await documentFileRepository.AnyAsync(x => x.Id == id);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Id {id}");
			}
		}

		async Task<MemoryStream> GetDocumentFromURL(string url)
		{
			var client = new HttpClient(); ;
			var response = await client.GetAsync(url);
			var rawStream = await response.Content.ReadAsStreamAsync();
			if (response.IsSuccessStatusCode)
			{
				MemoryStream docStream = new MemoryStream();
				rawStream.CopyTo(docStream);
				return docStream;
			}
			else { return null; }
		}

		public class SaveParameter
		{
			public string Content { get; set; }
			public string FileName { get; set; }
			public int DocumentId { get; set; }
		}

		public void Save([FromBody] SaveParameter data)
		{
			string name = data.FileName;
			string format = RetrieveFileType(name);
			if (string.IsNullOrEmpty(name))
			{
				name = $"DMS-WordDocument-{DateTime.Now.ToString("ddMMyyyyHHmmss")}.doc";
			}
			WDocument document = WordDocument.Save(data.Content);
			//in here give a path to category from document id
			FileStream fileStream = new FileStream(name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			document.Save(fileStream, GetWFormatType(format));
			document.Close();
		}

		private string RetrieveFileType(string name)
		{
			int index = name.LastIndexOf('.');
			string format = index > -1 && index < name.Length - 1 ?
				name.Substring(index) : ".doc";
			return format;
		}

		internal static WFormatType GetWFormatType(string format)
		{
			if (string.IsNullOrEmpty(format))
				throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
			switch (format.ToLower())
			{
				case ".dotx":
					return WFormatType.Dotx;
				case ".docx":
					return WFormatType.Docx;
				case ".docm":
					return WFormatType.Docm;
				case ".dotm":
					return WFormatType.Dotm;
				case ".dot":
					return WFormatType.Dot;
				case ".doc":
					return WFormatType.Doc;
				case ".rtf":
					return WFormatType.Rtf;
				case ".html":
					return WFormatType.Html;
				case ".txt":
					return WFormatType.Txt;
				case ".xml":
					return WFormatType.WordML;
				case ".odt":
					return WFormatType.Odt;
				default:
					throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
			}
		}

		public FileStreamResult ExportSFDT([FromBody] SaveParameter data)
		{
			string name = data.FileName;
			string format = RetrieveFileType(name);
			if (string.IsNullOrEmpty(name))
			{
				name = "Document1.doc";
			}
			WDocument document = WordDocument.Save(data.Content);
			return SaveDocument(document, format, name);
		}

		public FileStreamResult Export(IFormCollection data)
		{
			if (data.Files.Count == 0)
				return null;
			string fileName = this.GetValue(data, "filename");
			string name = fileName;
			string format = RetrieveFileType(name);
			if (string.IsNullOrEmpty(name))
			{
				name = "Document1";
			}
			WDocument document = this.GetDocument(data);
			return SaveDocument(document, format, fileName);
		}

		private FileStreamResult SaveDocument(WDocument document, string format, string fileName)
		{
			Stream stream = new MemoryStream();
			string contentType = "";
			if (format == ".pdf")
			{
				contentType = "application/pdf";
			}
			else
			{
				WFormatType type = GetWFormatType(format);
				switch (type)
				{
					case WFormatType.Rtf:
						contentType = "application/rtf";
						break;
					case WFormatType.WordML:
						contentType = "application/xml";
						break;
					case WFormatType.Html:
						contentType = "application/html";
						break;
					case WFormatType.Dotx:
						contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
						break;
					case WFormatType.Docx:
						contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
						break;
					case WFormatType.Doc:
						contentType = "application/msword";
						break;
					case WFormatType.Dot:
						contentType = "application/msword";
						break;
				}
				document.Save(stream, type);
			}
			document.Close();
			stream.Position = 0;
			return new FileStreamResult(stream, contentType)
			{
				FileDownloadName = fileName
			};
		}

		private string GetValue(IFormCollection data, string key)
		{
			if (data.ContainsKey(key))
			{
				string[] values = data[key];
				if (values.Length > 0)
				{
					return values[0];
				}
			}
			return "";
		}

		private WDocument GetDocument(IFormCollection data)
		{
			Stream stream = new MemoryStream();
			IFormFile file = data.Files[0];
			file.CopyTo(stream);
			stream.Position = 0;

			WDocument document = new WDocument(stream, WFormatType.Docx);
			stream.Dispose();
			return document;
		}
	}
}
