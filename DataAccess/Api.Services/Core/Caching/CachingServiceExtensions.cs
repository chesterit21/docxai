using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Services.Core.Caching;

/// <summary>
/// DI registration extension for caching services.
/// </summary>
public static class CachingServiceExtensions
{
    /// <summary>
    /// Registers IMemoryCache + ICacheService as Singleton.
    /// Call in Program.cs: builder.Services.AddSFCoreCaching();
    /// </summary>
    public static IServiceCollection AddSFCoreCaching(
        this IServiceCollection services,
        TimeSpan? defaultExpiration = null)
    {
        services.AddMemoryCache();

        services.AddSingleton<ICacheService>(sp =>
            new MemoryCacheService(
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<ILogger<MemoryCacheService>>(),
                defaultExpiration));

        return services;
    }
}
