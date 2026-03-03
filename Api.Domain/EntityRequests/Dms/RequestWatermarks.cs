using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
    public class RequestWatermarks
	{
        public int Id { get; set; }
        public string Text { get; set; }
    }

	public class RequestAddWatermarks
	{
		[Required]
		public string Text { get; set; }
	}
}
