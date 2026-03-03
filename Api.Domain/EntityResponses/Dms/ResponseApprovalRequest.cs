using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.Enum;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseApprovalRequest
    {
        public int RequestID { get; set; }
        public int? DocumentID { get; set; }
		public string DocumentTitle { get; set; }
		public int UserID { get; set; }
        public string Approver { get; set; }
        public string NextApprover { get; set; }
        public string ApprovalStep { get; set; }

        public DateTime? RequestDate { get; set; }
        public ApprovalStatusEnum ApprovalStatus { get; set; }
        public string ApprovalStatusName { get; set; }
        public DateTime? ResponseDate { get; set; }
		public string LastRemark { get; set; }
		public ResponseDetailMyApprovalRequest Detail { get; set; }
	}

	public class ResponseApprovalRequestAndCount
	{
		public List<ResponseApprovalRequest> ResponseApprovalRequests { get; set; }
		public long PageCount { get; set; }
	}

    public class ResponseCheckApproval
    {
		public int ApprovalID { get; set; }
		public int? DocumentID { get; set; }
		public string Notes { get; set; }
		public List<ResponseDocumentFiles> DocumentFiles { get; set; }
		public List<ResponseApprovalActivities> ApprovalActivities  { get; set; }
	}

	public class ResponseDetailMyApprovalRequest
	{
		public int ApprovalID { get; set; }
		public int DocumentID { get; set; }
		public string Notes { get; set; }
		public ResponseApprovalDocumentFiles MainFile { get; set; }
		public List<ResponseApprovalActivities> ApproveActivity { get; set; }
		public List<ResponseApprovalFlowMyRequest> ApprovalFlows { get; set; }
	}

}
