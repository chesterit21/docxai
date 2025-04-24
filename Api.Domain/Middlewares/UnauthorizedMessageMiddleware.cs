using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Api.Domain.Middlewares
{
    public class UnauthorizedMessageMiddleware
    {
        private readonly RequestDelegate _next;

        public UnauthorizedMessageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next.Invoke(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                var obj = new
                {
                    code = 401,
                    message = "unauthorized-token"
                };
                var json = JsonSerializer.Serialize(obj);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
                //return;
            }

        }
    }
}
