using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseDocumenCountByCategory
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public int DocumentCount { get; set; }
	}
}
