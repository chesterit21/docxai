using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestAttributes
	{
		[Required]
		public int Id { get; set; }
		[Required]
		public string AttributeName { get; set; }
		public string AttributeType { get; set; }
		[Required]
		public string AttributeElement { get; set; }
	}

	public class RequestAddAttributes
	{
		[Required]
		public string AttributeName { get; set; }
		public string AttributeType { get; set; }
		[Required]
		public string AttributeElement { get; set; }
	}

	public class JsonAttribute
	{
		public string type { get; set; }
		public bool required { get; set; }
		public string label { get; set; }
		public string placeholder { get; set; }
		public string helptext { get; set; }
		public int min { get; set; }
		public int max { get; set; }
		public string name { get; set; }
	}

	public class RequestAttrList : RequestPagination
	{
		public string search { get; set; }
	}

}
