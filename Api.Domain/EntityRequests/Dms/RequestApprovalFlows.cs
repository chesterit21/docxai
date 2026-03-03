using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
    public class RequestApprovalFlows
	{
        public int Id { get; set; }
		public int ApprovalID { get; set; }
		public int Step { get; set; }
		public int UserID { get; set; }
		public int RoleID { get; set; }
	}
}
