using Api.DataAccess.Models.Systems;
using Api.Domain;
using Api.Domain.EntityResponses;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Text.RegularExpressions;

namespace Api.Services.Systems
{
    public class LogApplicationService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IApplicationLogRepository repository) : BaseService(accessor, languageRepository)
    {
        public async Task<ResponsePagination> Get(int page, int limit)
        {
            await ValidateInputAsync([page, limit]);

            var totalRecords = await repository.CountAsync();
            var records = await repository.GetAsync(page, limit);

            return new ResponsePagination
            {
                TotalRecords = totalRecords,
                TotalPages = GetTotalPages(totalRecords, limit),
                Data = records
            };
        }

        public async Task<object> Get(DateTime startDate, DateTime endDate, int page, int limit)
        {
            await ValidateInputDateRangeAsync(startDate, endDate);

            var totalRecords = await repository.CountAsync(startDate, endDate);
            var records = await repository.GetAsync(x => x.InsertedAt >= startDate && x.InsertedAt <= endDate, page, limit, "InsertedAt", "desc");

            return new ResponsePagination
            {
                TotalRecords = totalRecords,
                TotalPages = GetTotalPages(totalRecords, limit),
                Data = records
            };
        }

        public async Task<Guid> Write(ExceptionContext context)
        {
            var request = context.HttpContext.Request;
            var body = await request.GetRawStringBodyAsync() ?? request.QueryString.Value?.ToString();

            int code = 500;
            string errorType = "Internal Server Error";
            if (context.Exception is ApiException realException)
            {
                code = realException.StatusCode;
                errorType = Enum.GetName(typeof(HttpStatusCode), code);//((HttpStatusCode)code).ToString();
            }

            var trace = Regex.Replace(context.Exception.StackTrace, @"\s\r\n\t+", " ").Replace(" at ", "\r\nat ").Trim();

            var email = accessor?.HttpContext?.User?.Identity?.Name ?? "N/A"; //null if called from background service or test tools
            var name = accessor?.GetClaim<string>("name");

            if (email != "N/A" && !string.IsNullOrWhiteSpace(name))
                email = $"{email} - {name}";

            if (request.Method == "POST")
            {
                body = MaskPassword(body);
            }

            var error = new ApplicationLog
            {
                UserAgent = accessor?.HttpContext?.Request?.Headers?.UserAgent.ToString(),
                IPAddress = accessor?.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                Endpoint = $"[{request.Method}] {request.Path}",
                Message = context.Exception.GetExceptionMessages(),
                StackTrace = trace,
                Type = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(errorType),
                Parameter = body,
                UserName = email
            };

            await repository.InsertAsync(error);

            return error.Id;
        }
    }
}
