using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Api.DataAccess
{
    public class RedisCache
    {
        private readonly IDatabase redis;
        public RedisCache(IConfiguration configuration)
        {
            var connectionString = configuration["SqlConnectionString:Redis"]!;
            redis = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
        }

        public string GetString(string key)
        {
            var value = redis.StringGet(key);
            if (value.IsNullOrEmpty)
                return null;

            return value.ToString();
        }

        public bool SetString(string key, string value)
        {
            return redis.StringSet(key, value, TimeSpan.FromHours(1));
        }

        public string Get(string key)
        {
            var value = redis.StringGet(key);
            if (value.HasValue && !value.IsNullOrEmpty)
            {
                return value.ToString();
            }

            return null;
        }

        public bool Set(string key, string value)
        {
            return redis.StringSet(key, value, TimeSpan.FromHours(1));
        }

        public bool Remove(string key)
        {
            if (redis.KeyExists(key))
                return redis.KeyDelete(key);

            return false;
        }
    }
}
