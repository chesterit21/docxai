using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestNotification : RequestPagination
	{
		public string DocumentTitle { get; set; }
		public DateTime? DTFrom { get; set; }
		public DateTime? DTTo { get; set; }
	}
}
