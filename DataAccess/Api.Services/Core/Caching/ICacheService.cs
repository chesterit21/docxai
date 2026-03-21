namespace Api.Services.Core.Caching;

/// <summary>
/// Abstraction for in-memory caching.
/// Interface lives in Domain so any project can consume it.
/// Implementation uses Microsoft.Extensions.Caching.Memory
/// and is registered in SFCore.StateManagement.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets or creates a cached value.
    /// If the key exists and hasn't expired, returns cached value.
    /// Otherwise, calls the factory delegate, caches the result, and returns it.
    /// </summary>
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Async version of GetOrCreate.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Sets a value in the cache with optional expiration asynchronously.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Sets a value in the cache with optional expiration.
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Tries to get a value from the cache.
    /// </summary>
    bool TryGet<T>(string key, out T? value);

    /// <summary>
    /// Removes a specific key from the cache.
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Removes all keys that start with the given prefix.
    /// Useful for invalidating groups of related cache entries.
    /// Example: RemoveByPrefix("models:") clears all model caches.
    /// </summary>
    void RemoveByPrefix(string prefix);
}
