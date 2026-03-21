using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Api.Services.Core.Caching;

/// <summary>
/// IMemoryCache wrapper implementing ICacheService.
/// Thread-safe, supports prefix-based invalidation, and configurable default TTL.
/// Register via DI as Singleton.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly TimeSpan _defaultExpiration;

    // Track active keys for prefix-based invalidation
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public MemoryCacheService(
        IMemoryCache cache,
        ILogger<MemoryCacheService> logger,
        TimeSpan? defaultExpiration = null)
    {
        _cache = cache;
        _logger = logger;
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(5);
    }

    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out T? cached) && cached is not null)
        {
            _logger.LogDebug("Cache HIT: {Key}", key);
            return cached;
        }

        _logger.LogDebug("Cache MISS: {Key}, creating...", key);
        var value = factory();
        Set(key, value!, expiration);
        return value;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out T? cached) && cached is not null)
        {
            _logger.LogDebug("Cache HIT: {Key}", key);
            return cached;
        }

        _logger.LogDebug("Cache MISS: {Key}, creating async...", key);
        var value = await factory();
        Set(key, value!, expiration);
        return value;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        Set(key, value, expiration);
        return Task.CompletedTask;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var ttl = expiration ?? _defaultExpiration;
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(ttl)
            .RegisterPostEvictionCallback((evictedKey, _, reason, _) =>
            {
                _keys.TryRemove(evictedKey.ToString()!, out _);
                _logger.LogDebug("Cache evicted: {Key} reason: {Reason}", evictedKey, reason);
            });

        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);
    }

    public bool TryGet<T>(string key, out T? value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger.LogDebug("Cache removed: {Key}", key);
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        _logger.LogDebug("Cache cleared {Count} keys with prefix: {Prefix}", keysToRemove.Count, prefix);
    }
}
