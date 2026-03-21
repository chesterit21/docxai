using System.Text.Json;
using Microsoft.Extensions.Logging;
using Api.Repository.Systems;
using Microsoft.Extensions.DependencyInjection;
using Api.Services.Core.Models;
using Api.Services.Core.Caching;

namespace Api.Services.Core.Models;

/// <summary>
/// Database-backed implementation of IModelRegistry.
/// Uses ICacheService for in-memory caching (delegated to SFCore.StateManagement).
/// Replaces the hardcoded ModelRegistry.
/// </summary>
public class DbModelRegistry : IModelRegistry
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DbModelRegistry> _logger;

    // Cache key prefix — matches CacheKeys.ModelCachePrefix
    private const string CachePrefix = "models:";
    private const string AllModelsKey = "models:all";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DbModelRegistry(
        IServiceScopeFactory scopeFactory,
        ICacheService cacheService,
        ILogger<DbModelRegistry> logger)
    {
        _scopeFactory = scopeFactory;
        _cacheService = cacheService;
        _logger = logger;
    }

    public ModelInfo? GetModel(string providerType, string modelId)
    {
        var models = GetCachedModels();
        var key = providerType?.ToLower() ?? "";
        if (models.TryGetValue(key, out var list))
        {
            return list.FirstOrDefault(m =>
                m.ModelId.Equals(modelId, StringComparison.OrdinalIgnoreCase));
        }
        return null;
    }

    public List<ModelInfo> GetAvailableModels(string providerType)
    {
        var models = GetCachedModels();
        var key = providerType?.ToLower() ?? "";
        if (models.TryGetValue(key, out var list))
        {
            return list.Where(m => m.IsAvailable).ToList();
        }
        return new List<ModelInfo>();
    }

    public List<ModelInfo> GetModelsByCapability(ModelCapabilityFilter filter)
    {
        var models = GetCachedModels();
        var allModels = models.Values.SelectMany(m => m).ToList();

        return allModels.Where(m =>
        {
            if (filter.RequiresVision == true && !m.Capabilities.SupportsVision) return false;
            if (filter.RequiresEmbedding == true && !m.Capabilities.SupportsEmbedding) return false;
            if (filter.RequiresFunctionCalling == true && !m.Capabilities.SupportsFunctionCalling) return false;
            if (filter.RequiresStreaming == true && !m.Capabilities.SupportsStreaming) return false;
            if (filter.MinContextTokens.HasValue && m.Limits.MaxContextTokens < filter.MinContextTokens.Value) return false;
            return m.IsAvailable;
        }).ToList();
    }

    public bool IsModelAvailable(string providerType, string modelId)
    {
        var model = GetModel(providerType, modelId);
        return model?.IsAvailable ?? false;
    }

    /// <summary>
    /// Returns all model IDs for a provider (used by adapters for SupportedModels).
    /// </summary>
    public List<string> GetSupportedModelIds(string providerType)
    {
        return GetAvailableModels(providerType)
            .Select(m => m.ModelId)
            .ToList();
    }

    /// <summary>
    /// Invalidates all model-related caches.
    /// Call this after any CRUD operation on ai_models.
    /// </summary>
    public void InvalidateCache()
    {
        _cacheService.RemoveByPrefix(CachePrefix);
        _logger.LogInformation("Model registry cache invalidated");
    }

    private Dictionary<string, List<ModelInfo>> GetCachedModels()
    {
        return _cacheService.GetOrCreate(AllModelsKey, () =>
        {
            _logger.LogInformation("Loading model registry from database...");

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IAiModelRepository>();

            var allEntities = repository.GetAsync().GetAwaiter().GetResult();

            var result = new Dictionary<string, List<ModelInfo>>();
            foreach (var group in allEntities.GroupBy(e => e.Provider.ToLower()))
            {
                result[group.Key] = group.Select(entity => new ModelInfo
                {
                    ModelId = entity.ModelName, // Mapping ModelName to ModelId as it's the primary identifier for requests
                    DisplayName = entity.ModelName,
                    ProviderType = entity.Provider,
                    Capabilities = new ModelCapabilities(), // Simplified: default capabilities
                    Limits = new ModelLimits { MaxOutputTokens = entity.MaxToken },
                    Pricing = new ModelPricing(),
                    IsAvailable = true, // Default to true if in table
                    Description = entity.ModelName
                }).ToList();
            }

            var allEntitiesCount = allEntities.Count;
            var resultCount = result.Count;

            _logger.LogInformation("Model registry loaded: {Count} models across {Providers} providers",
                allEntitiesCount, resultCount);
            return result;
        }, CacheTtl) ?? new Dictionary<string, List<ModelInfo>>();
    }

    private static T DeserializeOrDefault<T>(string json) where T : new()
    {
        if (string.IsNullOrWhiteSpace(json)) return new T();
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
        }
        catch
        {
            return new T();
        }
    }
}
