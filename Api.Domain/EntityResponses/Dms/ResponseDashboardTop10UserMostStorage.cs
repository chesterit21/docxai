using Api.Domain.EntityRequests.Dms;
using Api.Domain.Enum;
using Api.Domain.Formatters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseDashboardTop10UserMostStorage
    {
		public int UserID { get; set; }
		public string UserName { get; set; }
		[JsonIgnore]
		public long TotalSizeNum { get; set; }
		//public string TotalSize { get { return SizeFormatter.SizeSuffix(TotalSizeNum, 2); } }
		public string TotalSize { get; set; }
	}
}
