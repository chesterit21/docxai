using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api.Extensions
{
	public static class DocumentFilesHelper
	{
		/// <summary>
		/// Build the physical file path for a document file record.
		/// Matches existing repository/service convention: {BaseDirectory}/upload/{CategoryId}-{categoryName}/{newDocumentFileName}
		/// </summary>
		public static string GetPhysicalPathForDocumentFile(int categoryID, string categoryName, string newDocumentFileName)
		{
			if (categoryName == null) throw new ArgumentNullException(nameof(categoryName));
			if (newDocumentFileName == null) throw new ArgumentNullException(nameof(newDocumentFileName));

			var baseDir = AppDomain.CurrentDomain.BaseDirectory;
			var safeCategoryName = string.IsNullOrWhiteSpace(categoryName) ? "unknown" : categoryName.Trim();

			var categoryPathFolder = Path.Combine(baseDir, "upload", $"{categoryID}-{safeCategoryName}");
			var fileName = newDocumentFileName;
			return Path.Combine(categoryPathFolder, fileName ?? string.Empty);
		}

		/// <summary>
		/// Lowest-level helper that accepts explicit pieces and returns full physical path.
		/// </summary>
		public static string GetPhysicalPathForDocumentFile(int ownerId, string ownerUserName, int categoryId, string categoryName, string fileName, bool ensureDirectory = false)
		{
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName must be provided", nameof(fileName));

			//var baseDir = AppDomain.CurrentDomain.BaseDirectory;
			var setting = AppSettings.Read();
			string AppUploadFolder = setting.ApplicationInfoData.AppUploadFolder;

			var safeOwnerName = SanitizeForPath(ownerUserName);
			var safeCategoryName = SanitizeForPath(categoryName);
			var safeFileName = SanitizeFileName(fileName);

			var ownerFolder = $"{ownerId}-{safeOwnerName}";
			var categoryFolder = $"{categoryId}-{safeCategoryName}";

			//var folderPath = Path.Combine(baseDir, "upload", ownerFolder, categoryFolder);
			var folderPath = Path.Combine(AppUploadFolder, ownerFolder, categoryFolder);

			if (ensureDirectory)
			{
				try
				{
					Directory.CreateDirectory(folderPath);
				}
				catch
				{
					// best-effort; swallow so helper remains non-throwing for directory creation issues
				}
			}

			return Path.Combine(folderPath, safeFileName);
		}


		/// <summary>
		/// Lowest-level helper that accepts explicit pieces and returns full physical path.
		/// </summary>
		public static string GetPhysicalPathForCategory(int ownerId, string ownerUserName, int categoryId, string categoryName, bool ensureDirectory = false)
		{
			//var baseDir = AppDomain.CurrentDomain.BaseDirectory;
			var setting = AppSettings.Read();
			string AppUploadFolder = setting.ApplicationInfoData.AppUploadFolder;

			var safeOwnerName = SanitizeForPath(ownerUserName);
			var safeCategoryName = SanitizeForPath(categoryName);

			var ownerFolder = $"{ownerId}-{safeOwnerName}";
			var categoryFolder = $"{categoryId}-{safeCategoryName}";
			var categoryPathFolder = Path.Combine(AppUploadFolder, ownerFolder, categoryFolder);

			if (ensureDirectory)
			{
				try
				{
					if (!Directory.Exists(categoryPathFolder))
					{
						Directory.CreateDirectory(categoryPathFolder);
					}
				}
				catch
				{
					// best-effort; swallow so helper remains non-throwing for directory creation issues
				}
			}

			return categoryPathFolder;
		}

		/// <summary>
		/// Lowest-level helper that accepts explicit pieces and returns full physical path.
		/// </summary>
		public static string GetResourcePathForDocumentFile(int ownerId, string ownerUserName, int categoryId, string categoryName, string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("fileName must be provided", nameof(fileName));

			var baseDir = AppDomain.CurrentDomain.BaseDirectory;

			var safeOwnerName = SanitizeForPath(ownerUserName);
			var safeCategoryName = SanitizeForPath(categoryName);
			var safeFileName = SanitizeFileName(fileName);

			var ownerFolder = $"{ownerId}-{safeOwnerName}";
			var categoryFolder = $"{categoryId}-{safeCategoryName}";

			return Path.Combine("\\","resource", ownerFolder, categoryFolder, safeFileName).Replace('\\', '/');
		}

		/// <summary>
		/// Create a FileStreamResult for a document file record.
		/// Caller is responsible to have loaded the ResponseDocumentFiles model (via repository/service).
		/// Throws FileNotFoundException when file missing.
		/// </summary>
		public static FileStreamResult CreateFileStreamResult(int ownerId, string ownerUserName, int categoryId, string categoryName, string fileName)
		{
			//if (model == null) throw new ArgumentNullException(nameof(model));

			var filePath = GetPhysicalPathForDocumentFile(ownerId, ownerUserName, categoryId, categoryName, fileName);
			if (!File.Exists(filePath))
				throw new FileNotFoundException("Document file not found", filePath);

			var stream = new FileStream(
				filePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				bufferSize: 64 * 1024,
				useAsync: true);

			var provider = new FileExtensionContentTypeProvider();
			if (!provider.TryGetContentType(filePath, out var contentType))
			{
				contentType = "application/octet-stream";
			}

			var downloadName = string.IsNullOrWhiteSpace(fileName)
				? Path.GetFileName(filePath)
				: fileName;

			return new FileStreamResult(stream, contentType)
			{
				FileDownloadName = downloadName
			};
		}

		///// <summary>
		///// Convenience async helper that accepts a loader function to fetch the model by id,
		///// then returns a FileStreamResult. Useful from controllers/services:
		///// await DocumentFilesHelper.CreateFileStreamResultAsync(id, async i => await repository.GetInfo(i));
		///// </summary>
		//public static async Task<FileStreamResult> CreateFileStreamResultAsync(int documentFileId, Func<int, Task<ResponseDocumentFiles>> loader)
		//{
		//	if (loader == null) throw new ArgumentNullException(nameof(loader));
		//	var model = await loader(documentFileId).ConfigureAwait(false);
		//	if (model == null) throw new FileNotFoundException("Document file metadata not found");
		//	return CreateFileStreamResult(model);
		//}

		#region Helpers - sanitization
		private static string SanitizeForPath(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return "unknown";
			// remove invalid path/file chars
			var invalid = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).Distinct().ToArray();
			var cleaned = input;
			foreach (var c in invalid)
				cleaned = cleaned.Replace(c, '-');

			// collapse whitespace and replace with single dash
			cleaned = Regex.Replace(cleaned, @"\s+", "-").Trim('-');

			// limit length to avoid very long folder names
			if (cleaned.Length > 128) cleaned = cleaned.Substring(0, 128);

			// fallback
			return string.IsNullOrWhiteSpace(cleaned) ? "unknown" : cleaned;
		}

		private static string SanitizeFileName(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName)) return Guid.NewGuid().ToString();
			var invalid = Path.GetInvalidFileNameChars();
			var cleaned = fileName;
			foreach (var c in invalid)
				cleaned = cleaned.Replace(c, '-');

			cleaned = Regex.Replace(cleaned, @"\s+", "_").Trim('_');

			// keep extension if present, limit name length while preserving extension
			var ext = Path.GetExtension(cleaned);
			var nameOnly = string.IsNullOrEmpty(ext) ? cleaned : Path.GetFileNameWithoutExtension(cleaned);
			if (nameOnly.Length > 200)
				nameOnly = nameOnly.Substring(0, 200);
			return string.IsNullOrEmpty(ext) ? nameOnly : $"{nameOnly}{ext}";
		}
		#endregion
	}
}