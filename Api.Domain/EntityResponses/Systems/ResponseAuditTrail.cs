namespace Api.Domain.EntityResponses.Systems
{
    public class ResponseAuditTrail : BaseResponseData
    {
        public Guid Id { get; set; }

        public Guid? TransactionLogId { get; set; }

        public string TableName { get; set; }

        public string Command { get; set; }

        public object Before { get; set; }

        public object After { get; set; }

        public DateTime? InsertedAt { get; set; }

        public int? InsertedBy { get; set; }
    }
}
