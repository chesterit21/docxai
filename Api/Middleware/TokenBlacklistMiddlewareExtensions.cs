using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docubase.api.Middlewares
{
	public static class TokenBlacklistMiddlewareExtensions
	{
		public static IApplicationBuilder UseTokenBlacklist(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<Filters.TokenBlacklistMiddleware>();
		}
	}
}
