using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json.Nodes;

namespace Api.Extensions
{
    public static class ClaimsHelper
    {
        public static T GetClaim<T>(this IHttpContextAccessor accessor, string claimName) => accessor.HttpContext.GetClaim<T>(claimName);

        public static T GetClaim<T>(this HttpContext context, string claimName)
        {
            if (string.IsNullOrWhiteSpace(claimName))
                throw new ArgumentNullException(nameof(claimName));

            if (context == null)
                throw new ArgumentNullException(nameof(HttpContext));

            var header = context.Request.Headers["authorization"];
            if (header.Count == 0)
                return default;

            var token = header.ToString().Trim();

            if (token.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(7);

            var splits = token.Split('.');
            if (splits.Length != 3)
                throw new InvalidDataException("The token is invalid");

            token = splits[1];
            token = token.Replace('-', '+').Replace('_', '/');
            token = token.PadRight(token.Length + (4 - token.Length % 4) % 4, '=');
            token = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var o = JsonNode.Parse(token).AsObject();

            foreach (var key in o)
            {
                if (key.Key.Equals(claimName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return o[key.Key].AsValue().GetValue<T>();
                }
            }

            return default;
        }
    }
}
