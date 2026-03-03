using Api.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseApprovalActivities
	{
		public int Id { get; set; }
		public int ApprovalID { get; set; }
		public ApprovalActivity? ApprovalActivity { get; set; }
		public string ApprovalActivityDesc { get; set; }
		public DateTime ApprovalDate { get; set; }
		public string Remark { get; set; }
		public string Reason { get; set; }
		public int CurrentStep { get; set; }
		public int? NextStep { get; set; }
		public int? RelatedDocumentID { get; set; }
		public int? InsertedBy { get; set; }
		public string UserName { get; set; }
		//public DateTime InsertedAt { get; set; }
		public ResponseUser ActorInfo { get; set; }

		//public ResponseApprovals Approvals { get; set; }
	}
}
