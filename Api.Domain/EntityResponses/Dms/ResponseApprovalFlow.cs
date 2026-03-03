using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseApprovalFlow
    {
        public int Id { get; set; }
        public int ApprovalID { get; set; }
        public int Step { get; set; }
		public int ApprovalActivityCode { get; set; }
		public string ApprovalActivity { get; set; }
        public ResponseUser User { get; set; }
    }

	public class ResponseApprovalFlowMyRequest
	{
		public int Id { get; set; }
		public int Step { get; set; }
		public int ApprovalActivityCode { get; set; }
		public string Status { get; set; }
		public DateTime? AcitivityDate { get; set; }
		public ResponseUser User { get; set; }
	}
}
