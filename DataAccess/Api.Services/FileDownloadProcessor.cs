using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Threading;

namespace Api.Services
{
	public static class FileDownloadProcessor
	{
		/// <summary>
		/// Create a FileStreamResult for the specified physical file path.
		/// - Sets an appropriate content-type using FileExtensionContentTypeProvider.
		/// - Opens the file stream for asynchronous reading.
		/// - Sets the download filename (Content-Disposition).
		/// 
		/// Note: This helper does NOT implement Range (resume) requests. For large files
		/// or resumable downloads implement range handling in the controller using HttpContext.
		/// </summary>
		/// <param name="filePath">Full physical path to the file on disk.</param>
		/// <param name="downloadFileName">Optional client filename. If null the physical file name is used.</param>
		/// <returns>FileStreamResult streaming the file, or throws FileNotFoundException when file is missing.</returns>
		public static FileStreamResult CreateFileStreamResult(string filePath, string? downloadFileName = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("filePath must be provided", nameof(filePath));

			if (!System.IO.File.Exists(filePath))
				throw new FileNotFoundException("File not found", filePath);

			// Open stream with Read/Share semantics appropriate for downloads.
			var stream = new FileStream(
				filePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				bufferSize: 64 * 1024,
				useAsync: true);

			// Resolve content type
			var provider = new FileExtensionContentTypeProvider();
			if (!provider.TryGetContentType(filePath, out var contentType))
			{
				contentType = "application/octet-stream";
			}

			var result = new FileStreamResult(stream, contentType)
			{
				FileDownloadName = downloadFileName ?? Path.GetFileName(filePath)
			};

			return result;
		}

		/// <summary>
		/// Convenience overload that accepts a Stream already opened by caller.
		/// Caller is responsible for opening the stream and (optionally) disposing it.
		/// The returned FileStreamResult will use the provided stream directly.
		/// </summary>
		/// <param name="stream">Readable stream for the file content.</param>
		/// <param name="fileName">Suggested download file name.</param>
		/// <param name="contentType">Optional content type. If null defaults to application/octet-stream.</param>
		/// <returns>FileStreamResult</returns>
		public static FileStreamResult CreateFileStreamResult(Stream stream, string fileName, string? contentType = null)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (string.IsNullOrWhiteSpace(fileName))
				throw new ArgumentException("fileName must be provided", nameof(fileName));

			var provider = new FileExtensionContentTypeProvider();
			if (string.IsNullOrWhiteSpace(contentType))
			{
				if (!provider.TryGetContentType(fileName, out contentType))
					contentType = "application/octet-stream";
			}

			var result = new FileStreamResult(stream, contentType)
			{
				FileDownloadName = fileName
			};

			return result;
		}
	}
}