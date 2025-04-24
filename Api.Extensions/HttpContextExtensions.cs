using Microsoft.AspNetCore.Http;
using System.Text;

namespace Api.Extensions
{
    public static class HttpContextExtension
    {
        public static string GetHeaderValue(this HttpContext context, string header)
        {
            if (context.Request.Headers.TryGetValue(header, out var value))
                return value.ToString();

            return null;
        }

        /// <summary>
        /// Reads raw body asynchronously from an Http Request.
        /// </summary>
        /// <param name="request">HttpRequest parameter.</param>
        /// <param name="rewind">Rewind stream for the next middlewares.</param>
        /// <returns>Request as string.</returns>
        public static async Task<string> GetRawStringBodyAsync(this HttpRequest request, bool rewind = false)
        {
            if (request.Body != null &&
                request.Body.Length > 0 &&
                request.Body.CanRead &&
                request.Body.CanSeek &&
                !string.IsNullOrWhiteSpace(request.ContentType))
            {
                var contentTypes = new[] { "json", "text", "html", "xml" };
                if (contentTypes.Any(request.ContentType.Contains))
                {
                    request.Body.Position = 0;
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync().ConfigureAwait(false);
                    request.Body.Position = 0;
                    if (rewind)
                        request.Body.Seek(0, SeekOrigin.Begin);

                    return body?.Trim();
                }
            }
            return null;
        }

        /// <summary>
        /// Reads raw body synchronously from an Http Request.
        /// </summary>
        /// <param name="request">HttpRequest parameter.</param>
        /// <param name="rewind">Rewind stream for the next middlewares.</param>
        /// <returns>Request as string.</returns>
        public static string GetRawStringBody(this HttpRequest request, bool rewind = false)
        {
            if (request.Body != null &&
                request.Body.Length > 0 &&
                request.Body.CanRead &&
                request.Body.CanSeek &&
                !string.IsNullOrWhiteSpace(request.ContentType))
            {
                var contentTypes = new[] { "json", "text", "html", "xml" };
                if (contentTypes.Any(request.ContentType.Contains))
                {
                    request.Body.Position = 0;
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = reader.ReadToEnd();
                    request.Body.Position = 0;
                    if (rewind)
                        request.Body.Seek(0, SeekOrigin.Begin);

                    return body?.Trim();
                }
            }
            return null;
        }
    }
}
