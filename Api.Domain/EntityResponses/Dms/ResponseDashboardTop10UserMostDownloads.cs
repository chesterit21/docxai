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
    public class ResponseDashboardTop10UserMostDownloads
    {
        public int UserID { get; set; }
        public string UserFullName { get; set; }
        public int DownloadCount { get; set; }
	}
}
