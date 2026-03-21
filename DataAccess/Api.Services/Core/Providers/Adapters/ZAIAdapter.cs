using Microsoft.Extensions.Logging;
using Api.Services.Core.Models;
using Api.Services.Core.Providers.Base;
using Api.DataAccess.Models.Systems;

namespace Api.Services.Core.Providers.Adapters;

/// <summary>
/// ZAI adapter - extends OpenAI adapter as it uses OpenAI-compatible API.
/// </summary>
public class ZAIAdapter : OpenAIAdapter
{
    public override string ProviderName => "ZAI";

    public override ProviderCapability Capabilities => new()
    {
        SupportsStreaming = true,
        SupportsVision = false,
        SupportsFunctionCalling = true,
        SupportsSystemPrompt = true,
        MaxContextTokens = GetMaxContextTokens(32768),
        SupportedModels = _modelRegistry.GetAvailableModels(_config.ProviderType)
            .Select(m => m.ModelId).ToList()
    };

    public ZAIAdapter(
        HttpClient http,
        ProviderConfig config,
        IModelRegistry modelRegistry,
        ILogger<ZAIAdapter> logger)
        : base(http, config, modelRegistry, logger) { }

    protected override void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_config.BaseUrl ?? "https://api.za.ai");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en");
    }

    protected override string DefaultEndpoint => "/api/paas/v4/chat/completions";

    private int GetMaxContextTokens(int defaultValue)
    {
        // 1. Try specific Default Model
        if (!string.IsNullOrEmpty(_config.DefaultModel))
        {
            var model = _modelRegistry.GetModel(_config.ProviderType, _config.DefaultModel);
            if (model != null)
            {
                return model.Limits.MaxContextTokens;
            }
        }

        // 2. Try max of any available model
        var models = _modelRegistry.GetAvailableModels(_config.ProviderType);
        if (models.Any())
        {
            return models.Max(m => m.Limits.MaxContextTokens);
        }

        // 3. Metadata fallback
        if (_config.Metadata != null &&
            _config.Metadata.TryGetValue("MaxContextTokens", out var tokenObj) &&
            int.TryParse(tokenObj.ToString(), out var tokens))
        {
            return tokens;
        }

        return defaultValue;
    }
}
