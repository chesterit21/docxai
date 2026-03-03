using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestFavoritesDocument : RequestPagination
	{
		public string DocumentTitle { get; set; }
	}
}
