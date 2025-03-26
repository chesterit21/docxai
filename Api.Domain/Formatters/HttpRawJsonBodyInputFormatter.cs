using Microsoft.AspNetCore.Mvc.Formatters;

namespace Api.Domain.Formatters
{
    /// <summary>
    /// Enables users to request and pass string parameter without binding to a particular class or object.
    /// </summary>
    public class HttpRawJsonBodyInputFormatter : InputFormatter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpRawJsonBodyInputFormatter()
        {
            SupportedMediaTypes.Add("application/json");
        }

        /// <summary>
        /// Overrides ReadRequestBodyAsync. Intercepts the request body if the parameter type in the Controller is string.
        /// </summary>
        /// <param name="context">InputFormatterContext</param>
        /// <returns>InputFormatterResult</returns>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using var reader = new StreamReader(request.Body);
            var content = await reader.ReadToEndAsync();
            return await InputFormatterResult.SuccessAsync(content);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }
    }
}
