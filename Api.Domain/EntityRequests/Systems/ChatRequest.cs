using System.Collections.Generic;

namespace Api.Domain.EntityRequests.Systems
{
    public class ChatRequest
    {
        public int session_id { get; set; }
        public int user_id { get; set; }
        public string message { get; set; }
        public List<int> document_ids { get; set; }
        public List<long> document_sizes { get; set; }
    }
}
