using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Api.Services.Core.Inference;
using Api.Services.Core.Models;
using Api.Services.Core.Models.Errors;
using Api.Services.Core.Models.Streaming;
using Api.Services.Core.Providers.Base;
using Api.DataAccess.Models.Systems;

namespace Api.Services.Core.Providers.Adapters;
// Api.Services.Core/Providers/Adapters/OpenAIAdapter.cs
public class OpenAIAdapter : BaseLLMProvider
{
    public override string ProviderName => "OpenAI";

    public override ProviderCapability Capabilities => new()
    {
        SupportsStreaming = true,
        SupportsVision = true,
        SupportsFunctionCalling = true,
        SupportsSystemPrompt = true,
        MaxContextTokens = GetMaxContextTokens(128000),
        SupportedModels = _modelRegistry.GetAvailableModels(_config.ProviderType)
            .Select(m => m.ModelId).ToList()
    };

    public OpenAIAdapter(
        HttpClient http,
        ProviderConfig config,
        IModelRegistry modelRegistry,
        ILogger<OpenAIAdapter> logger)
        : base(http, config, modelRegistry, logger) { }

    protected override void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_config.BaseUrl ?? "https://api.openai.com");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

        // For local providers (Ollama/Llama), model loading can take time.
        // Increase timeout to avoid Orchestrator retrying prematurely.
        if (_config.ProviderType.Equals("local", StringComparison.OrdinalIgnoreCase))
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(_config.TimeoutSeconds, 300));
        }
        else
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        }
    }

    protected virtual string DefaultEndpoint => "/v1/chat/completions";

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

    protected override object MapToProviderRequest(InferenceRequest request)
    {
        var messages = new List<object>();

        // Add system prompt as first message if provided
        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new
            {
                role = "system",
                content = request.SystemPrompt
            });
        }

        foreach (var msg in request.Messages)
        {
            if (msg.Parts != null && msg.Parts.Any())
            {
                // Multimodal message
                var content = new List<object>();

                foreach (var part in msg.Parts)
                {
                    if (part.Type == ContentPartType.Text)
                    {
                        content.Add(new
                        {
                            type = "text",
                            text = part.Text
                        });
                    }
                    else if (part.Type == ContentPartType.Image)
                    {
                        var imageUrl = part.Image.Url ??
                            $"data:{part.Image.MimeType};base64,{part.Image.Base64Data}";

                        content.Add(new
                        {
                            type = "image_url",
                            image_url = new
                            {
                                url = imageUrl
                            }
                        });
                    }
                }

                messages.Add(new
                {
                    role = msg.Role,
                    content
                });
            }
            else
            {
                messages.Add(new
                {
                    role = msg.Role,
                    content = msg.Content
                });
            }
        }

        var payload = new Dictionary<string, object>
        {
            ["model"] = request.Model ?? _config.DefaultModel ?? "gpt-4o",
            ["messages"] = messages,
            ["stream"] = request.Stream
        };

        if (request.Temperature.HasValue)
        {
            payload["temperature"] = request.Temperature.Value;
        }

        if (request.MaxTokens.HasValue)
        {
            payload["max_tokens"] = request.MaxTokens.Value;
        }

        if (request.Tools != null && request.Tools.Any())
        {
            payload["tools"] = request.Tools.Select(t => new
            {
                type = "function",
                function = new
                {
                    name = t.Function.Name,
                    description = t.Function.Description,
                    parameters = t.Function.Parameters
                }
            }).ToList();
        }

        if (request.ToolChoice != null)
        {
            payload["tool_choice"] = request.ToolChoice;
        }

        if (request.JsonMode == true)
        {
            payload["response_format"] = new { type = "json_object" };
        }

        return payload;
    }

    protected override object ParseProviderResponse(string json)
    {
        return JsonSerializer.Deserialize<OpenAIResponse>(json);
    }

    public async Task<EmbeddingResponse> EmbedAsync(
            EmbeddingRequest request,
            CancellationToken ct = default)
    {
        var modelInfo = _modelRegistry.GetModel(_config.ProviderType, request.Model);

        if (modelInfo == null || !modelInfo.Capabilities.SupportsEmbedding)
        {
            throw new ProviderException(
                ProviderName,
                $"Model '{request.Model}' does not support embeddings",
                errorType: ProviderErrorType.InvalidRequest);
        }

        // Prepare inputs
        var inputs = request.Inputs?.Any() == true
            ? request.Inputs
            : new List<string> { request.Input };

        var payload = new Dictionary<string, object>
        {
            ["model"] = request.Model ?? "text-embedding-3-large",
            ["input"] = inputs.Count == 1 ? (object)inputs[0] : inputs
        };

        if (request.Dimensions.HasValue)
        {
            payload["dimensions"] = request.Dimensions.Value;
        }

        if (!string.IsNullOrEmpty(request.EncodingFormat))
        {
            payload["encoding_format"] = request.EncodingFormat;
        }

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/embeddings", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var embeddingResponse = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);

        // Calculate cost
        decimal? estimatedCost = null;
        if (modelInfo != null && embeddingResponse.Usage != null)
        {
            estimatedCost = (embeddingResponse.Usage.TotalTokens / 1_000_000m) * modelInfo.Pricing.InputPer1MTokens;
        }

        return new EmbeddingResponse
        {
            Model = embeddingResponse.Model,
            Embeddings = embeddingResponse.Data.Select(d => new EmbeddingData
            {
                Index = d.Index,
                Vector = d.Embedding
            }).ToList(),
            Usage = new UsageInfo
            {
                PromptTokens = embeddingResponse.Usage?.PromptTokens ?? 0,
                TotalTokens = embeddingResponse.Usage?.TotalTokens ?? 0,
                EstimatedCost = estimatedCost
            },
            RawResponse = responseJson
        };
    }

    protected override async Task<InferenceResponse> MapToUnifiedResponse(object providerResponse)
    {
        var openai = (OpenAIResponse)providerResponse;
        var choice = openai.Choices?.FirstOrDefault();

        var toolCalls = choice?.Message?.ToolCalls?
            .Select(tc => new ToolCall
            {
                Id = tc.Id,
                Type = tc.Type,
                Function = new FunctionCall
                {
                    Name = tc.Function.Name,
                    Arguments = tc.Function.Arguments
                }
            })
            .ToList();

        // Calculate cost
        var modelInfo = _modelRegistry.GetModel(_config.ProviderType, openai.Model);
        decimal? estimatedCost = null;

        if (modelInfo != null && openai.Usage != null)
        {
            var inputCost = (openai.Usage.PromptTokens / 1_000_000m) * modelInfo.Pricing.InputPer1MTokens;
            var outputCost = (openai.Usage.CompletionTokens / 1_000_000m) * modelInfo.Pricing.OutputPer1MTokens;
            estimatedCost = inputCost + outputCost;
        }

        return new InferenceResponse
        {
            Id = openai.Id,
            Model = openai.Model,
            Content = choice?.Message?.Content,
            FinishReason = choice?.FinishReason,
            ToolCalls = toolCalls,
            Usage = new UsageInfo
            {
                PromptTokens = openai.Usage?.PromptTokens ?? 0,
                CompletionTokens = openai.Usage?.CompletionTokens ?? 0,
                TotalTokens = openai.Usage?.TotalTokens ?? 0,
                EstimatedCost = estimatedCost
            }
        };
    }

    public override async IAsyncEnumerable<StreamChunk> StreamAsync(
        InferenceRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // if (_config.ProviderType != "local")
        // {
        //     ValidateRequest(request);
        // }

        var providerRequest = MapToProviderRequest(request);
        var json = JsonSerializer.Serialize(providerRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _config.Endpoint ?? DefaultEndpoint)
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);

        await foreach (var line in StreamParser.ParseSSEStream(stream, ct))
        {
            var chunks = new List<StreamChunk>();

            try
            {
                var chunk = JsonSerializer.Deserialize<OpenAIStreamChunk>(line);
                var delta = chunk?.Choices?.FirstOrDefault()?.Delta;

                if (delta?.Content != null)
                {
                    chunks.Add(new StreamChunk
                    {
                        Content = delta.Content,
                        Type = StreamChunkType.ContentDelta,
                        IsComplete = false
                    });
                }

                if (delta?.ToolCalls != null && delta.ToolCalls.Any())
                {
                    foreach (var toolCall in delta.ToolCalls)
                    {
                        if (!string.IsNullOrEmpty(toolCall.Function?.Name))
                        {
                            chunks.Add(new StreamChunk
                            {
                                Type = StreamChunkType.ToolCallStart,
                                Metadata = new Dictionary<string, object>
                                {
                                    ["tool_id"] = toolCall.Id ?? "",
                                    ["tool_name"] = toolCall.Function.Name
                                }
                            });
                        }

                        if (!string.IsNullOrEmpty(toolCall.Function?.Arguments))
                        {
                            chunks.Add(new StreamChunk
                            {
                                Content = toolCall.Function.Arguments,
                                Type = StreamChunkType.ToolCallDelta,
                                IsComplete = false
                            });
                        }
                    }
                }

                var finishReason = chunk?.Choices?.FirstOrDefault()?.FinishReason;
                if (!string.IsNullOrEmpty(finishReason))
                {
                    chunks.Add(new StreamChunk
                    {
                        Type = StreamChunkType.Complete,
                        IsComplete = true
                    });
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "[OpenAI] Failed to parse stream chunk: {Line}", line);
            }

            foreach (var c in chunks)
            {
                yield return c;
            }
        }
    }

    private class OpenAIEmbeddingResponse
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("data")]
        public List<EmbeddingDataItem> Data { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }
    }

    private class EmbeddingDataItem
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("embedding")]
        public List<float> Embedding { get; set; }
    }

    // OpenAI-specific DTOs
    private class OpenAIResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }
    }

    private class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OpenAIToolCall> ToolCalls { get; set; }
    }

    private class OpenAIToolCall
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("function")]
        public OpenAIFunction Function { get; set; }
    }

    private class OpenAIFunction
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; }
    }

    private class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    private class OpenAIStreamChunk
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("choices")]
        public List<StreamChoice> Choices { get; set; }
    }

    private class StreamChoice
    {
        [JsonPropertyName("delta")]
        public Delta Delta { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }
    }

    private class Delta
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<OpenAIToolCall> ToolCalls { get; set; }
    }
}
