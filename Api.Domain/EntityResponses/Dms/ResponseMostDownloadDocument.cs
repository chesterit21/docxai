using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public static class ActivityWeights
	{
		public const int DocumentCreate = 5;  // High effort
		public const int ApprovalAction = 3;  // Medium effort
		public const int Login = 1;           // Low effort
	}

	public class ResponseMostActiveUsers
	{
		public int UserId { get; set; }
		public string UserName { get; set; }

		// Raw Counts
		public int LoginCount { get; set; }
		//public int DocumentsCreated { get; set; }
		//public int ApprovalsActioned { get; set; }

		//// The Calculated Metric
		//public int TotalActivityScore { get; set; }
	}
}
