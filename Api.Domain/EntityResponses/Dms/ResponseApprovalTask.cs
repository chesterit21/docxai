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
    public class ResponseApprovalTask
    {
        public int RequestID { get; set; }
        //public int? DocumentIDs { get; set; }
        //public int UserID { get; set; }
		//public int Step { get; set; }
		public string Approver { get; set; }
        public string NextApprover { get; set; }
        public string Initiator { get; set; }

        public DateTime? ApprovalDate { get; set; }
        public ApprovalStatusEnum ApprovalStatus { get; set; }
        public string ApprovalStatusName { get; set; }
        public List<ResponseApprovalActivities> ApprovalActivities { get; set; }
        //public ResponseApprovalActivities LastActivities { get; set; }

	}

    public class ResponseApprovalTaskAndCount
    {
        public List<ResponseApprovalTask> ResponseApprovalTasks { get; set; }
        public long PageCount {  get; set; }
    }
}
