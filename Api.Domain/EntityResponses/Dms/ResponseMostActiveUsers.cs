using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseMostDownloadDocument
	{
		public int DocumentId { get; set; }
		public string DocumentTitle { get; set; }
		public string CategoryName { get; set; }
		public int DownloadCount { get; set; }
	}

	public class ResponseMostViewedDocument
	{
		public int DocumentId { get; set; }
		public string DocumentTitle { get; set; }
		public string CategoryName { get; set; }
		public int ViewCount { get; set; }
	}
}
