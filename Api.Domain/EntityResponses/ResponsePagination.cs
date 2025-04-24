namespace Api.Domain.EntityResponses
{
    public class ResponsePagination
    {
        public long TotalRecords { get; set; }
        public long TotalPages { get; set; }
        public object Data { get; set; }
    }
}
