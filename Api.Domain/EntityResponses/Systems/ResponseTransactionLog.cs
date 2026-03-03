using Api.Domain.EntityRequests;
using Api.Domain.EntityResponses.Dms;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Domain.EntityResponses.Systems
{
    public class ResponseTransactionLog : BaseResponseData
    {
		public Guid Id { get; set; }

		public string IPAddress { get; set; }

		public string UserAgent { get; set; }

		public string Action { get; set; }

		public string Description { get; set; }

		public string Path { get; set; }

		public string Parameter { get; set; }
		public DateTime InsertedAt { get; set; }
		public ResponseAuditTrail Audit { get; set; } = null;
	}

	public class ResponseTransactionLogPagination : RequestPagination
	{
		public List<ResponseTransactionLog> Record { get; set; }
		public long TotalRecord { get; set; }
	}
}
