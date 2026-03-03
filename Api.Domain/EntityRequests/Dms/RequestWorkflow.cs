using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestWorkflow
	{
		public RequestApproval Approval { get; set; }
		public List<RequestApprovalFlow> Flows { get; set; }

		public class RequestApproval
		{
			[Required]
			public int DocumentID { get; set; }
			[Required]
			public int? CategoryID { get; set; }
			[Required]
			public string Notes { get; set; }
		}

		public class RequestApprovalFlow
		{
			[Required]
			public int userID { get; set; }
			[Required]
			public int Step { get; set; }
		}
	}
}
