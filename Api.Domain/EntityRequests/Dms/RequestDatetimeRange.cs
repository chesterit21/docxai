using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDatetimeRange : BaseRequest
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}
}
