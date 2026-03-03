using Api.Domain.EntityRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseAttributePagination : RequestPagination
	{
        public List<ResponseAttributeList> Record { get; set; }
        public long TotalRecord { get; set; }
    }

    public class ResponseAttributeList
	{
		public int Id { get; set; }
		public string AttributeName { get; set; }
		public string AttributeElement { get; set; }
		public string AttributeType { get; set; }
		public bool IsActive { get; set; }
		public DateTime InsertedAt { get; set; }
		public int CreatedBy { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int UpdatedBy { get; set; }

		public string InsertedByUserName { get; set; }
		public string InsertedByByFullName { get; set; }
		public string UpdatedByUserName { get; set; }
		public string UpdatedByFullName { get; set; }
		public bool IsSystem { get; set; } = false;
	}
}
