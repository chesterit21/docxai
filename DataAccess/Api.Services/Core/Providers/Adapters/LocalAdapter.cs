using Microsoft.Extensions.Logging;
using Api.Services.Core.Models;
using Api.Services.Core.Providers.Base;
using Api.DataAccess.Models.Systems;

namespace Api.Services.Core.Providers.Adapters;

/// <summary>
/// Local adapter for local LLM servers (llama-server, vLLM, Ollama, LMStudio, etc.).
/// Most local servers are OpenAI-compatible.
/// </summary>
public class LocalAdapter : OpenAIAdapter
{
    public override string ProviderName => "Local";

    public override ProviderCapability Capabilities => new()
    {
        SupportsStreaming = true,
        SupportsVision = false, // Depends on model/server, safer to default false or check metadata
        SupportsFunctionCalling = true, // Many local models/servers now support this
        SupportsSystemPrompt = true,
        MaxContextTokens = GetMaxContextTokens(32768), // Default to 32k commonly supported by Mistral/Llama3
        SupportedModels = _modelRegistry.GetAvailableModels(_config.ProviderType)
            .Select(m => m.ModelId).ToList()
    };

    public LocalAdapter(
        HttpClient http,
        ProviderConfig config,
        IModelRegistry modelRegistry,
        ILogger<LocalAdapter> logger)
        : base(http, config, modelRegistry, logger) { }

    protected override void ConfigureHttpClient()
    {
        // Default to localhost:8080 (llama.cpp server default)
        // Users can change this in DB to http://localhost:11434/v1 (Ollama) or http://localhost:8000/v1 (vLLM)
        var baseUrl = _config.BaseUrl;
        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = "http://localhost:8080";
        }

        _httpClient.BaseAddress = new Uri(baseUrl);

        // API Key is often optional for local servers, but we send it if configured
        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
        }
    }

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
