using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Domain.EntityRequests.Dms;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseDocumenReminderListPagination
	{
		public List<ResponseDocumenReminderListList> Record { get; set; }
		public long TotalRecord { get; set; }
	}

	public class ResponseDocumenReminderListList
	{
		public int DocumentId { get; set; }
		public int ReminderId { get; set; }
		public DateTime ReminderDateTime { get; set; }
		public string ReminderDesc { get; set; }
		public string DocumentTitle { get; set; }
		public string DocumentOwner { get; set; }
	}
}
