using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
    public class RequestActionApproval
	{
        public int Id { get; set; }
        public int? DocumentID { get; set; }
		public int? CategoryID { get; set; }
		public string Reason { get; set; }
		public int? RelatedDocumentId { get; set; }
	}
}
