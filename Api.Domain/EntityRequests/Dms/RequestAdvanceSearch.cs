using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestAdvanceSearch
	{
		public class RequestApproval
		{
			public string DocumentTitle { get; set; }
			public DateTime? ModifyDateFrom { get; set; }
			public DateTime? ModifyDateTo { get; set; }
			public int? CategoryID { get; set; }
		}
	}
}
