using Api.Domain.EntityResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Collections;
using System.Net;

namespace Api.Domain
{
    public static class ResultFactory
    {
        public static HttpResult Create(object data)
        {
            var result = new HttpResult.DataResult
            {
                Code = HttpStatusCode.OK,
                Data = data
            };


            //if (data == null)
            //{
            //    result.Code = HttpStatusCode.NoContent;
            //}
            //else if (typeof(IEnumerable).IsAssignableFrom(data.Data.GetType()))
            //{
            //    var ien = ((IEnumerable)data.Data).Cast<object>().ToList();
            //    if (ien.Count == 0)
            //    {
            //        result.Code = HttpStatusCode.NoContent;
            //    }
            //}

            result.Message = result.Code.ToString();

            return new HttpResult(result);
        }

        //BaseManualResponse
        public static HttpResult Create<T>(ResponseData<T> response)
        {
            var result = new HttpResult.DataResult
            {
                Code = response.Code,
                Message = response.Message,
                Data = response.Data
            };

            return new HttpResult(result);
        }

        //public static HttpResult Create(string message, HttpStatusCode code, object data)
        //{
        //    var result = new HttpResult.DataResult
        //    {
        //        Code = code,
        //        Message = message,
        //        Data = data
        //    };

        //    return new HttpResult(result);
        //}

        public static HttpResultMessage Create(string message, HttpStatusCode code)
        {
            var result = new HttpResultMessage.DataResult
            {
                Code = code,
                Message = message
            };

            return new HttpResultMessage(result);
        }

        public static HttpResult Create()
        {
            var result = new HttpResult.DataResult
            {
                Code = HttpStatusCode.OK,
                Message = "OK"
            };

            return new HttpResult(result);
        }

        public static HttpResultPagination Create(ResponsePagination data)
        {
            var result = new HttpResultPagination.DataResult
            {
                Code = HttpStatusCode.OK,
                Data = data.Data,
                TotalPages = data.TotalPages,
                TotalRecords = data.TotalRecords,
                Message = "OK"
            };

            //if (data == null || data.Data == null || data.Data == null)
            //{
            //    result.Code = HttpStatusCode.NoContent;
            //}
            //else if (typeof(IEnumerable).IsAssignableFrom(data.Data.GetType()))
            //{
            //    var ien = ((IEnumerable)data.Data).Cast<object>().ToList();
            //    if (ien.Count == 0)
            //    {
            //        result.Code = HttpStatusCode.NoContent;
            //    }
            //}

            result.Message = result.Code.ToString();

            return new HttpResultPagination(result);
        }

        public class HttpResult : ObjectResult
        {
            public HttpResult([ActionResultObjectValue] DataResult data)
                : base(data)
            {
                StatusCode = (int)data.Code;
            }

            public class DataResult
            {
                public HttpStatusCode Code { get; internal set; }
                public string Message { get; internal set; }
                public object Data { get; internal set; }
            }
        }

        public class HttpResultMessage : ObjectResult
        {
            public HttpResultMessage([ActionResultObjectValue] DataResult data)
                : base(data)
            {
                StatusCode = (int)data.Code;
            }

            public class DataResult
            {
                public HttpStatusCode Code { get; internal set; }
                public string Message { get; internal set; }
            }
        }

        public class HttpResultPagination : ObjectResult
        {
            public HttpResultPagination([ActionResultObjectValue] DataResult data)
                : base(data)
            {
                StatusCode = (int)data.Code;
            }

            public class DataResult
            {
                public HttpStatusCode Code { get; internal set; }
                public string Message { get; internal set; }
                public long TotalRecords { get; set; }
                public long TotalPages { get; set; }
                public object Data { get; internal set; }
            }
        }
    }
}