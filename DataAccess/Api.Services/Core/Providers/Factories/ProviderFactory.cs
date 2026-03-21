using Microsoft.Extensions.Logging;
using Api.Services.Core.Models;
using Api.Services.Core.Providers.Adapters;
using Api.Services.Core.Providers.Base;
using Api.DataAccess.Models.Systems;

namespace Api.Services.Core.Providers.Factories;

public class ProviderFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IModelRegistry _modelRegistry;
    private readonly ILoggerFactory _loggerFactory;

    public ProviderFactory(
        IHttpClientFactory httpClientFactory,
        IModelRegistry modelRegistry,
        ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _modelRegistry = modelRegistry;
        _loggerFactory = loggerFactory;
    }

    public ILLMProvider CreateProvider(ProviderConfig config)
    {
        var httpClient = _httpClientFactory.CreateClient(config.ProviderType);

        return config.ProviderType.ToLower() switch
        {

            "zai" => new ZAIAdapter(
                httpClient,
                config,
                _modelRegistry,
                _loggerFactory.CreateLogger<ZAIAdapter>()),

            "openai-style" => new OpenAIStyleAdapter(
                httpClient,
                config,
                _modelRegistry,
                _loggerFactory.CreateLogger<OpenAIStyleAdapter>()),

            "gemini" => new GeminiAdapter(
                httpClient,
                config,
                _modelRegistry,
                _loggerFactory.CreateLogger<GeminiAdapter>()),

            "local" => new LocalAdapter(
                httpClient,
                config,
                _modelRegistry,
                _loggerFactory.CreateLogger<LocalAdapter>()),

            _ => throw new NotSupportedException($"Provider '{config.ProviderType}' is not supported")
        };
    }
}