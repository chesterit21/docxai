using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseDocumentMonthlyGrowth
	{
		public int MonthNumber { get; set; }
		public string MonthName { get; set; }
		public int TotalDocuments { get; set; }
	}
}
