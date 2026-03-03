using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Enum;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestApprovalActivities
	{
		[Required]
		public int ApprovalID { get; set; }
		public ApprovalActivity ApprovalActivity { get; set; }
		public string ApprovalActivityName { get; set; }
		public string Remark { get; set; }
		public string Reason { get; set; }
		public int CurrentStep { get; set; }
		public int NextStep { get; set; }
		public int? RelatedDocumentID { get; set; }
		//public ResponseApprovals Approvals { get; set; }
	}

	public class RequestUpdateApprovalActivities : RequestApprovalActivities
	{
		public int Id { get; set; }
	}
}
