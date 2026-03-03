using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestAttributeCollections
	{
		//public Guid Id { get; set; }
		public string CollectionName { get; set; }
		public string CollectionDescription { get; set; }
		public string AttributeElementCollection { get; set; }
	}

	public class RequestUpdateAttributeCollections : RequestAttributeCollections
	{
		public Guid Id { get; set; }
	}

	public class RequestAttrCollectionList : RequestPagination
	{
		public string search { get; set; }
	}
}
