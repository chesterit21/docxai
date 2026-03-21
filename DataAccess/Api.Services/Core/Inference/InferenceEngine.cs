using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Api.Services.Core.Models;
using Api.Services.Core.Models.Streaming;
using Api.Services.Core.Providers.Base;
using Api.Services.Core.Providers.Factories;
using Api.DataAccess.Models.Systems;
using Api.Repository.Systems;

namespace Api.Services.Core.Inference;

/// <summary>
/// Main inference engine that routes requests to appropriate LLM providers.
/// </summary>
public class InferenceEngine
{
    private readonly ProviderFactory _providerFactory;
    private readonly IAiModelRepository _aiModelRepo;
    private readonly ILogger<InferenceEngine> _logger;

    public InferenceEngine(
        ProviderFactory providerFactory,
        IAiModelRepository aiModelRepo,
        ILogger<InferenceEngine> logger)
    {
        _providerFactory = providerFactory;
        _aiModelRepo = aiModelRepo;
        _logger = logger;
    }

    public async Task<InferenceResponse> ExecuteAsync(
        InferenceRequest request,
        string providerName = null,
        CancellationToken ct = default)
    {
        // Get config from database
        var modelEntity = await GetAiModelAsync(providerName, request.Model, ct);

        if (modelEntity == null)
        {
            throw new InvalidOperationException(
                $"No active provider configuration found for model '{request.Model ?? "default"}'");
        }

        var config = MapToProviderConfig(modelEntity);

        // Create provider adapter
        var provider = _providerFactory.CreateProvider(config);

        _logger.LogInformation(
            "Executing inference with {Provider} using model {Model}",
            provider.ProviderName,
            request.Model ?? config.DefaultModel);

        // Execute
        return await provider.InferNonStreamingAsync(request, ct);
    }

    public async IAsyncEnumerable<StreamChunk> StreamAsync(
        InferenceRequest request,
        string providerName = null,
        Guid? configId = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        AiModel modelEntity = null;
        if (configId.HasValue)
        {
            modelEntity = await _aiModelRepo.GetSingleAsync(m => m.Id == configId.Value);
        }
        else
        {
            modelEntity = await GetAiModelAsync(providerName, request.Model, ct);
        }

        if (modelEntity == null)
        {
            throw new InvalidOperationException(
                $"No active provider configuration found for model '{request.Model ?? "default"}'");
        }

        var config = MapToProviderConfig(modelEntity);
        var provider = _providerFactory.CreateProvider(config);

        _logger.LogInformation(
            "Starting streaming inference with {Provider} using model {Model}",
            provider.ProviderName,
            request.Model ?? config.DefaultModel);

        await foreach (var chunk in provider.StreamAsync(request, ct))
        {
            yield return chunk;
        }
    }

    public async Task<EmbeddingResponse> EmbedAsync(
        EmbeddingRequest request,
        string providerName = null,
        CancellationToken ct = default)
    {
        var modelEntity = await GetAiModelAsync(providerName, request.Model, ct);

        if (modelEntity == null)
        {
            throw new InvalidOperationException(
                $"No active provider configuration found for model '{request.Model ?? "default"}'");
        }

        var config = MapToProviderConfig(modelEntity);
        var provider = _providerFactory.CreateProvider(config);

        _logger.LogInformation(
            "Executing embedding with {Provider} using model {Model}",
            provider.ProviderName,
            request.Model ?? config.DefaultModel);

        return await provider.EmbedAsync(request, ct);
    }

    public async Task<bool> ValidateProviderAsync(
        string providerName,
        CancellationToken ct = default)
    {
        var modelEntity = await GetAiModelAsync(providerName, null, ct);

        if (modelEntity == null)
            return false;

        var config = MapToProviderConfig(modelEntity);
        var provider = _providerFactory.CreateProvider(config);
        return await provider.ValidateAsync(ct);
    }

    private async Task<AiModel> GetAiModelAsync(string providerName, string modelName, CancellationToken ct)
    {
        var all = await _aiModelRepo.GetAsync();
        if (modelName != null)
        {
            return all.FirstOrDefault(m => m.ModelName.Equals(modelName, StringComparison.OrdinalIgnoreCase));
        }
        if (providerName != null)
        {
            return all.FirstOrDefault(m => m.Provider.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        }
        return all.FirstOrDefault();
    }

    private ProviderConfig MapToProviderConfig(AiModel entity)
    {
        return new ProviderConfig
        {
            ProviderType = entity.Provider,
            BaseUrl = entity.UrlApi,
            ApiKey = entity.ApiKey,
            DefaultModel = entity.ModelName,
            TimeoutSeconds = 60,
            MaxRetries = 2
        };
    }
}