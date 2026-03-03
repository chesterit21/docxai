using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDocumentFiles
	{
		public int Id { get; set; }
		public int DocumentID { get; set; }
		public IFormFile file { get; set; }
		public bool IsMainDocumentFile { get; set; }
	}

	public class RequestDocumentFilesUpdateMainDocument
	{
		[Required]
		public int DocumentID { get; set; }
		[Required]
		public int DocumentFileID{ get; set; }
		[Required]
		public bool IsMainDocumentFile { get; set; }
	}

	public class RequestDocumentFileList : RequestPagination
	{
		public string search { get; set; }
	}

	public class RequestDocumentExcelFile
	{
		public int Id { get; set; }
		public Stream streamOfTheFile { get; set; }
		public bool IsMainDocumentFile { get; set; }
		public bool IsNewVersion { get; set;}
		public string FileContentType { get; set; }
	}
}
