using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseAttributeCollectionPagination
	{
        public List<ResponseAttributeCollectionList> Record { get; set; }
        public long TotalRecord { get; set; }
    }

    public class ResponseAttributeCollectionList
	{
		public Guid Id { get; set; }
		public string CollectionName { get; set; }
		public string CollectionDescription { get; set; }
		public string AttributeElementCollection { get; set; }
		public DateTime InsertedAt { get; set; }
		public int CreatedBy { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int UpdatedBy { get; set; }

		public string InsertedByUserName { get; set; }
		public string InsertedByByFullName { get; set; }
		public string UpdatedByUserName { get; set; }
		public string UpdatedByFullName { get; set; }

	}
}
