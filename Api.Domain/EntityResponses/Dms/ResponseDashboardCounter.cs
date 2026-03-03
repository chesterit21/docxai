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
    public class ResponseDashboardCounter
    {
        public int TotalDocuments { get; set; }
        public int ExpiringDocuments { get; set; }
        public int ExpiredDocuments { get; set; }
        public int PendingApprovalDocuments { get; set; }
        public int RejectedDocuments { get; set; }
		public int ApprovedDocuments { get; set; }
		//public int TotalSizeDocuments { get; set; }
        public string TotalSizeDocuments { get; set; }

		public int PendingMyApproval { get; set; }
	}
}
