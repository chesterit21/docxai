using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestCategoryWorkflow
	{
		public RequestCTApproval Approval { get; set; }
		public List<RequestCTApprovalFlow> Flows { get; set; }

		public class RequestCTApproval
		{
			[Required]
			public int? CategoryID { get; set; }
			[Required]
			public string Notes { get; set; }
		}

		public class RequestCTApprovalFlow
		{
			//[Required]
			public int UserID { get; set; }
			//[Required]
			public int GroupID { get; set; }
			[Required]
			public int Step { get; set; }
		}
	}
}
