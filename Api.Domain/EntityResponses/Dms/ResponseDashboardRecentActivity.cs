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
	public class ResponseDashboardRecentActivityPagination
	{
		public List<ResponseDashboardRecentActivity> Record { get; set; }
		public long TotalRecord { get; set; }
	}

	public class ResponseDashboardRecentActivity
    {
		public Guid Id { get; set; }
		public int DocumentID { get; set; }
        public string DocumentTitle { get; set; }
        //public string FileName { get; set; }
        //public string Version { get; set; }
        public string Activity { get; set; }
        public DateTime ActivityDate { get; set; }
        public string ActivityBy { get; set; }
	}
}
