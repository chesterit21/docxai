using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDocumentCountByCategory : RequestDatetimeRange
	{
		public int? UserId { get; set; }
	}
}
