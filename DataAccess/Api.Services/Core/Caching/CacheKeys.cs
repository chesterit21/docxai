namespace Api.Services.Core.Caching;

/// <summary>
/// Well-known cache key constants.
/// Group by feature prefix to enable prefix-based invalidation.
/// </summary>
public static class CacheKeys
{
    // ===== Model Registry =====
    private const string ModelPrefix = "models:";

    /// <summary>Cache key for all models of a specific provider. e.g. "models:provider:claude"</summary>
    public static string ModelsByProvider(string providerType) => $"{ModelPrefix}provider:{providerType.ToLower()}";

    /// <summary>Cache key for all models across all providers.</summary>
    public static string AllModels => $"{ModelPrefix}all";

    /// <summary>Prefix for invalidating all model-related caches.</summary>
    public static string ModelCachePrefix => ModelPrefix;

    // ===== Provider Config =====
    private const string ProviderConfigPrefix = "providerconfig:";

    public static string ActiveProviderConfig(string providerType) => $"{ProviderConfigPrefix}active:{providerType.ToLower()}";
    public static string AllProviderConfigs => $"{ProviderConfigPrefix}all";
    public static string ProviderConfigCachePrefix => ProviderConfigPrefix;

    // ===== General pattern for future features =====
    // private const string SomeFeaturePrefix = "somefeature:";
    // public static string SomeFeatureKey(string id) => $"{SomeFeaturePrefix}{id}";
    // public static string SomeFeatureCachePrefix => SomeFeaturePrefix;
}
