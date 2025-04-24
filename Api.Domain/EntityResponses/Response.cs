using System.Net;

namespace Api.Domain.EntityResponses
{
    public class BaseResponseData
    {
        public string InsertedByUserName { get; set; }

        public string InsertedByFullName { get; set; }
    }

    public class ResponseData<T>
    {
        public HttpStatusCode Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class ResponseMessage
    {
        public HttpStatusCode Code { get; set; }
        public string Message { get; set; }
    }
}
