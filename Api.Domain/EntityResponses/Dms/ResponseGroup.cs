using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseGroupPagination
	{
        public List<ResponseGroup> Record { get; set; }
        public long TotalREcord { get; set; }
    }

    public class ResponseGroup
    {
		public int Id { get; set; }
		public string GroupName { get; set; }
		public string GroupDescription { get; set; }
		public DateTime InsertedAt { get; set; }
		public int CreatedBy { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int UpdatedBy { get; set; }

		public string InsertedByUserName { get; set; }
		public string insertedByByFullName { get; set; }
		public string UpdatedByUserName { get; set; }
		public string UpdatedByFullName { get; set; }
		public bool IsSystem { get; set; } = false;
	}
}
