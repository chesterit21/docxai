using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDeleteWorkflowUser
	{
		[Required]
		public int ApprovalID { get; set; }
		[Required]
		public int userID { get; set; }
	}
}
