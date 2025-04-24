using Api.Domain;
using Api.Extensions;
using Api.Services.Systems;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Text.Json;

namespace Docubase.api.Filters
{
    public class ExceptionFilter(ILogger<ExceptionFilter> logger, LogApplicationService log) : IAsyncExceptionFilter
    {
        private async Task WriteToDatabaseSqlServer(ExceptionContext context)
        {
            try
            {
                await log.Write(context);
            }
            catch (Exception ex)
            {
                logger.LogWarning(context.Exception.Message, ex);
                logger.LogError(ex, ex.Message);
            }
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (!Debugger.IsAttached)
                await WriteToDatabaseSqlServer(context);

            int code = 500;
            if (context.Exception is ApiException aex)
            {
                code = aex.StatusCode;
            }

            var result = JsonSerializer.Serialize(new
            {
                code,
                success = false,
                message = context.Exception.GetExceptionMessages()
                //Data = context.Exception.StackTrace,
            });

            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = code;
            response.ContentType = "application/json";
            response.ContentLength = result.Length;
            await response.WriteAsync(result);
        }
    }
}
