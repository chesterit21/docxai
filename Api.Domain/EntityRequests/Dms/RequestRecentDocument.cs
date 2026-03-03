using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestRecentDocument : RequestPagination
	{
		public int CategoryId { get; set; }
		public string DocumentTitle { get; set; }
	}
}
