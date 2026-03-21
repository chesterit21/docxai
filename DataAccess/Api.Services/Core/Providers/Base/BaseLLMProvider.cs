using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Api.Services.Core.Inference;
using Api.Services.Core.Models;
using Api.Services.Core.Models.Errors;
using Api.Services.Core.Models.Streaming;
using Api.DataAccess.Models.Systems;

namespace Api.Services.Core.Providers.Base;

public abstract class BaseLLMProvider : ILLMProvider
{
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;
    protected readonly ProviderConfig _config;
    protected readonly IModelRegistry _modelRegistry;

    public abstract string ProviderName { get; }
    public abstract ProviderCapability Capabilities { get; }

    protected BaseLLMProvider(
        HttpClient httpClient,
        ProviderConfig config,
        IModelRegistry modelRegistry,
        ILogger logger)
    {
        _httpClient = httpClient;
        _config = config;
        _modelRegistry = modelRegistry;
        _logger = logger;

        ConfigureHttpClient();
    }

    protected virtual void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
    }

    public async Task<InferenceResponse> InferNonStreamingAsync(
        InferenceRequest request,
        CancellationToken ct = default)
    {
        // Validate request against model capabilities
        //ValidateRequest(request);

        var retryCount = 0;
        Exception lastException = null;

        while (retryCount <= _config.MaxRetries)
        {
            try
            {
                _logger.LogInformation(
                    "[{Provider}] Sending inference request (attempt {Attempt}/{MaxRetries})",
                    ProviderName, retryCount + 1, _config.MaxRetries + 1);

                // Convert to provider-specific format
                var providerRequest = MapToProviderRequest(request);

                // Send HTTP request
                var httpResponse = await SendRequestAsync(providerRequest, ct);

                // Check status code
                if (!httpResponse.IsSuccessStatusCode)
                {
                    await HandleErrorResponse(httpResponse);
                }

                // Parse provider-specific response
                var responseJson = await httpResponse.Content.ReadAsStringAsync(ct);
                var providerResponse = ParseProviderResponse(responseJson);

                // Map back to unified format
                var unifiedResponse = await MapToUnifiedResponse(providerResponse);
                unifiedResponse.RawResponse = responseJson;

                _logger.LogInformation(
                    "[{Provider}] Inference successful. Tokens: {Input}/{Output}",
                    ProviderName,
                    unifiedResponse.Usage?.PromptTokens,
                    unifiedResponse.Usage?.CompletionTokens);

                return unifiedResponse;
            }
            catch (ProviderException ex) when (ex.ErrorType == ProviderErrorType.RateLimited && retryCount < _config.MaxRetries)
            {
                lastException = ex;
                retryCount++;

                var delay = CalculateRetryDelay(retryCount);
                _logger.LogWarning(
                    "[{Provider}] Rate limited. Retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})",
                    ProviderName, delay, retryCount + 1, _config.MaxRetries + 1);

                await Task.Delay(delay, ct);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                retryCount++;

                if (retryCount <= _config.MaxRetries)
                {
                    var delay = CalculateRetryDelay(retryCount);
                    _logger.LogWarning(
                        "[{Provider}] HTTP request failed. Retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})",
                        ProviderName, delay, retryCount + 1, _config.MaxRetries + 1);

                    await Task.Delay(delay, ct);
                }
            }
            catch (Exception ex)
            {
                throw new ProviderException(
                    ProviderName,
                    "Inference failed",
                    ex,
                    errorType: ProviderErrorType.Unknown);
            }
        }

        throw new ProviderException(
            ProviderName,
            $"Inference failed after {_config.MaxRetries + 1} attempts",
            lastException,
            errorType: ProviderErrorType.NetworkError);
    }

    protected virtual void ValidateRequest(InferenceRequest request)
    {
        // Get model info
        var modelInfo = _modelRegistry.GetModel(
            _config.ProviderType,
            request.Model ?? _config.DefaultModel);

        if (modelInfo == null)
        {
            throw new ProviderException(
                ProviderName,
                $"Model '{request.Model ?? _config.DefaultModel}' not found in registry",
                errorType: ProviderErrorType.InvalidRequest);
        }

        if (!modelInfo.IsAvailable)
        {
            throw new ProviderException(
                ProviderName,
                $"Model '{modelInfo.ModelId}' is not available",
                errorType: ProviderErrorType.InvalidRequest);
        }

        var caps = modelInfo.Capabilities;

        // Validate streaming
        if (request.Stream && !caps.SupportsStreaming)
        {
            throw new ProviderException(
                ProviderName,
                $"Model '{modelInfo.ModelId}' does not support streaming",
                errorType: ProviderErrorType.InvalidRequest);
        }

        // Validate vision
        if (request.Messages.Any(m => m.Parts?.Any(p => p.Type == ContentPartType.Image) ?? false))
        {
            if (!caps.SupportsVision)
            {
                throw new ProviderException(
                    ProviderName,
                    $"Model '{modelInfo.ModelId}' does not support vision/image inputs",
                    errorType: ProviderErrorType.InvalidRequest);
            }

            // Validate image count
            var imageCount = request.Messages.Sum(m =>
                m.Parts?.Count(p => p.Type == ContentPartType.Image) ?? 0);

            if (imageCount > modelInfo.Limits.MaxImagesPerRequest)
            {
                throw new ProviderException(
                    ProviderName,
                    $"Too many images. Max allowed: {modelInfo.Limits.MaxImagesPerRequest}, provided: {imageCount}",
                    errorType: ProviderErrorType.InvalidRequest);
            }
        }

        // Validate system prompt
        if (!string.IsNullOrEmpty(request.SystemPrompt) && !caps.SupportsSystemPrompt)
        {
            _logger.LogWarning(
                "[{Provider}] Model '{Model}' doesn't support system prompts. Converting to user message.",
                ProviderName, modelInfo.ModelId);
        }

        // Validate function calling
        if (request.Tools?.Any() == true && !caps.SupportsFunctionCalling)
        {
            throw new ProviderException(
                ProviderName,
                $"Model '{modelInfo.ModelId}' does not support function calling",
                errorType: ProviderErrorType.InvalidRequest);
        }

        // Validate token limits
        if (request.MaxTokens.HasValue && request.MaxTokens > modelInfo.Limits.MaxOutputTokens)
        {
            _logger.LogWarning(
                "[{Provider}] Requested max_tokens ({Requested}) exceeds model limit ({Limit}). Capping to limit.",
                ProviderName, request.MaxTokens, modelInfo.Limits.MaxOutputTokens);

            request.MaxTokens = modelInfo.Limits.MaxOutputTokens;
        }
    }

    protected virtual async Task HandleErrorResponse(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        var statusCode = (int)response.StatusCode;
        var errorType = ErrorParser.ParseHttpStatusCode(statusCode);

        _logger.LogError(
            "[{Provider}] HTTP {StatusCode}: {Body}",
            ProviderName, statusCode, body);

        throw new ProviderException(
            ProviderName,
            $"HTTP {statusCode}: {response.ReasonPhrase}",
            statusCode: statusCode,
            rawResponse: body,
            errorType: errorType);
    }

    protected virtual int CalculateRetryDelay(int retryCount)
    {
        // Exponential backoff: 1s, 2s, 4s, 8s
        return (int)Math.Pow(2, retryCount - 1) * 1000;
    }

    public virtual async Task<bool> ValidateAsync(CancellationToken ct = default)
    {
        try
        {
            var testRequest = new InferenceRequest
            {
                Model = _config.DefaultModel,
                Messages = new List<Message>
                {
                    new() { Role = "user", Content = "Hi" }
                },
                MaxTokens = 10
            };

            await InferNonStreamingAsync(testRequest, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Provider}] Validation failed", ProviderName);
            return false;
        }
    }

    // Abstract methods - setiap adapter implement sendiri
    protected abstract object MapToProviderRequest(InferenceRequest request);
    protected abstract Task<InferenceResponse> MapToUnifiedResponse(object providerResponse);
    protected abstract object ParseProviderResponse(string json);

    protected virtual async Task<HttpResponseMessage> SendRequestAsync(
        object request,
        CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        _logger.LogDebug("[{Provider}] Request: {Json}", ProviderName, json);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await _httpClient.PostAsync(_config.Endpoint, content, ct);
    }

    public abstract IAsyncEnumerable<StreamChunk> StreamAsync(
        InferenceRequest request,
        CancellationToken ct = default);

    public virtual Task<EmbeddingResponse> EmbedAsync(
          EmbeddingRequest request,
          CancellationToken ct = default)
    {
        throw new NotSupportedException(
            $"Provider '{ProviderName}' does not support embedding operations");
    }
}