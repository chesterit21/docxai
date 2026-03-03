using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestCategoryList : RequestPagination
	{
	    public int? ParentCategoryId { get; set; }
		public string CategoryName { get; set; }
	}
}
