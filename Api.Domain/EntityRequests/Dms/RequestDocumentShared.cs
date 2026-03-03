using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDocumentShared
	{
		[Required]
		public int DocumentID { get; set; }
		public List<RequestUserDocumentPrivillege> requestUserDocPrivillege { get; set; }
	}

	public class RequestUserDocumentPrivillege
	{
		public string ShareType { get; set; } // fill with "user" or "group"
		public int? UserID { get; set; }
		public int? GroupID { get; set; }
		public bool IsView { get; set; }
		public bool IsEdit { get; set; }
		public bool IsDelete { get; set; }
	}
}
