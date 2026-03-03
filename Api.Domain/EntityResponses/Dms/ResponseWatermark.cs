using Api.Domain.EntityRequests.Dms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseWatermarkRequestAndCount
	{
		public List<ResponseWatermark> Records { get; set; }
		public long TotalRecord { get; set; }
	}


	public class ResponseWatermark
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public string InsertedByFullname { get; set; }
		public DateTime InsertedAt { get; set; }
		public string UpdatedByFullName { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
