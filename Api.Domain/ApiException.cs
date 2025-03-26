namespace Api.Domain
{
    [Serializable]
    public class ApiException : Exception
    {
        public ApiException(string message)
            : base(message)
        {
            StatusCode = 400;
        }

        public ApiException(string message, int statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(string message, System.Net.HttpStatusCode statusCodes)
            : base(message)
        {
            StatusCode = (int)statusCodes;
        }

        public int StatusCode { get; set; }
    }
}
