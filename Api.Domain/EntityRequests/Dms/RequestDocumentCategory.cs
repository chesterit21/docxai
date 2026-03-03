using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDocumentCategory : RequestPagination
	{
		[Required]
		public int CategoryId { get; set; }
		public string DocumentTile { get; set; }
	}

	public class RequestDocument : RequestPagination
	{
		public string SearchText { get; set; }
	}
}
