using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDocumentReminder
	{
		public int DocumentID { get; set; }
		public short? DayOfReminder { get; set; }
		public DateTime? DateofReminder { get; set; }
	}

	public class RequestDocumentRemindersNew
	{
		[Required]
		[Column(TypeName = "integer")]
		public int DocumentID { get; set; }

		[Required]
		public DateTime ReminderDateTime { get; set; }

		[Required]
		[Column(TypeName = "text")]
		public string ReminderDesc { get; set; }
	}

	public class RequestDocumentRemindersUpdate : RequestDocumentRemindersNew
	{
		public int Id { get; set; }
		[Required]
		[Column(TypeName = "integer")]
		public int DocumentID { get; set; }

		[Required]
		public DateTime ReminderDateTime { get; set; }

		[Required]
		[Column(TypeName = "text")]
		public string ReminderDesc { get; set; }
	}

	public class RequestDocumentReminderNewList : RequestPagination
	{
		public int DocumentID { get; set; }
	}
}