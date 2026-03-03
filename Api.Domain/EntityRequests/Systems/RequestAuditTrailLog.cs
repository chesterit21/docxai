using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Systems
{
	public class RequestAuditTrailLogsPagination : RequestPagination
	{
		[Required]
		public string LogTransationID { get; set; }
	}
}
