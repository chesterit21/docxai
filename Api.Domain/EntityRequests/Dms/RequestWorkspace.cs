using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestWorkspace : RequestPagination
	{
		public string DocumentOrCategoryTitle { get; set; }
		
		public string DocumentOrCategoryDesc { get; set; }		
	}
}
