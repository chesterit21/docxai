using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Services.Masters;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace Api.Services.Dms
{
	public interface ITokenBlacklistService
	{
		void AddToken(string token, DateTime expires);
		bool IsTokenBlacklisted(string token);
	}

	public class InMemoryTokenBlacklistService : ITokenBlacklistService
	{
		// Store token and expiration time
		private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new ConcurrentDictionary<string, DateTime>();
		public void AddToken(string token, DateTime expires)
		{
			// Add token with expiration time
			_blacklistedTokens.TryAdd(token, expires);
		}
		public bool IsTokenBlacklisted(string token)
		{
			// Clean-up expired tokens lazily
			foreach (var entry in _blacklistedTokens)
			{
				if (entry.Value < DateTime.Now)
				{
					_blacklistedTokens.TryRemove(entry.Key, out _);
				}
			}
			return _blacklistedTokens.ContainsKey(token);
		}
	}

	//public class TokenBlacklistService : BaseService
	//{
	//}
}
