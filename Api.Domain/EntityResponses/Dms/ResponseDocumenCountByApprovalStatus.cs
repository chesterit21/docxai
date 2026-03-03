using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseDocumenCountByApprovalStatus
	{
		public Domain.Enum.ApprovalStatusEnum Status { get; set; }
		public string StatusName { get; set; }
		public int DocumentCount { get; set; }
	}
}
