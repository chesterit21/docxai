using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Masters
{
	public class RequestWatermarkList : RequestPagination
	{
		public string search { get; set; }
	}
}
