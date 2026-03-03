using Api.Services.Dms;

namespace Docubase.api.Filters
{
	// Middleware to check if token is blacklisted
	public class TokenBlacklistMiddleware
	{
		private readonly RequestDelegate _next;
		public TokenBlacklistMiddleware(RequestDelegate next)
		{
			_next = next;
		}
		public async Task Invoke(HttpContext context, ITokenBlacklistService blacklistService)
		{
			var authHeader = context.Request.Headers["Authorization"].ToString();
			if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			{
				var token = authHeader.Substring("Bearer ".Length).Trim();
				if (blacklistService.IsTokenBlacklisted(token))
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					await context.Response.WriteAsync("Token has been revoked.");
					return;
				}
			}
			await _next(context);
		}
	}
}
