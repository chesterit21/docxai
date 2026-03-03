using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.Enum;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseDocumentLog
    {
		public Guid Id { get; set; }
		public Guid? LogAuditTrailID { get; set; }
		public int DocumentID { get; set; }
		public string ActionLogDocument { get; set; }

		public string DocumentTitle { get; set; }
        public string DocumentDesc { get; set; }
		public string Before { get; set; }
		public string After { get; set; }
		public DateTime InsertedAt { get; set; }
		public string InsertedByUserName { get; set; }
		public string InsertedByFullName { get; set; }
		public string UpdatedByUserName { get; set; }
		public string UpdatedByFullName { get; set; }
	}
}
