using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Users
{
    public class ResponseEmailVerification
    {
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
