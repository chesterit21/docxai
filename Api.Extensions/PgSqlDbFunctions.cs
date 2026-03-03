using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Extensions
{
	public static class PgSqlDbFunctions
	{
		[DbFunction("DATE_PART", IsBuiltIn = true)]
		public static double DatePart(string field, DateTime timestamp)
		=> throw new NotSupportedException("Direct calls are not supported; use in LINQ queries.");
	}
}
