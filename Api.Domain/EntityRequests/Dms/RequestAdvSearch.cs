using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestAdvSearch : RequestPagination
	{
		public string DocumentTitle { get; set; }
		public DateTime? DtFrom { get; set; }
		public DateTime? DtTo { get; set; }
		public int? CategoryId { get; set; }
	}
}
