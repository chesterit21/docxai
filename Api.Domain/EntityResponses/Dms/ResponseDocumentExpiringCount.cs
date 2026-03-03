using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	//public enum ExpirationPeriod
	//{
	//	Week,
	//	Month,
	//	Year
	//}
	public enum ExpirationPeriod
	{
		ThisYear,
		Lastear,
		NextYear
	}

	public class ResponseDocumentExpiringCount
	{
		// The key value for grouping (e.g. in this is, Week Number, Month Number, Year)
		public int GroupKey { get; set; }

		// A descriptive label (e.g., "Week 45," "November," "2024")
		public string GroupLabel { get; set; }
		public int ExpiringDocumentCount { get; set; }
	}
}
