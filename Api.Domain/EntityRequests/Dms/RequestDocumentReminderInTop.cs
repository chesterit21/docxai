using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDocumentReminderInTop : RequestPagination
	{
		public string ReminderDesc { get; set; }
		public DateTime? DTFrom { get; set; }
		public DateTime? DTTo { get; set; }
	}
}
