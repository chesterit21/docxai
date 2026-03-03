using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDocumentUpload
	{
		[Required]
		public int DocumentID { get; set; }
		//[Required]
		public bool IsMainDocumentFile { get; set; }
		[Required]
		public IFormFile fileUpload { get; set; }
	}
}
