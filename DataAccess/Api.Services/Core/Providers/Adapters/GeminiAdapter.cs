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
// Api.Services.Core/Providers/Adapters/GeminiAdapter.cs
public class GeminiAdapter : BaseLLMProvider
{
    public override string ProviderName => "Gemini";

    public override ProviderCapability Capabilities => new()
    {
        SupportsStreaming = true,
        SupportsVision = true,
        SupportsAudio = true,
        SupportsFunctionCalling = true,
        SupportsSystemPrompt = true,
        MaxContextTokens = GetMaxContextTokens(2000000),
        SupportedModels = _modelRegistry.GetAvailableModels(_config.ProviderType)
            .Select(m => m.ModelId).ToList()
    };

    public GeminiAdapter(
        HttpClient http,
        ProviderConfig config,
        IModelRegistry modelRegistry,
        ILogger<GeminiAdapter> logger)
        : base(http, config, modelRegistry, logger) { }

    protected override void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_config.BaseUrl ?? "https://generativelanguage.googleapis.com");
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
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

    protected override object MapToProviderRequest(InferenceRequest request)
    {
        var contents = new List<object>();

        // Gemini system instruction
        object systemInstruction = null;
        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            systemInstruction = new
            {
                parts = new[] { new { text = request.SystemPrompt } }
            };
        }

        // Map messages
        foreach (var msg in request.Messages)
        {
            var role = msg.Role == "assistant" ? "model" : "user";

            if (msg.Parts != null && msg.Parts.Any())
            {
                // Multimodal message
                var parts = new List<object>();

                foreach (var part in msg.Parts)
                {
                    if (part.Type == ContentPartType.Text)
                    {
                        parts.Add(new { text = part.Text });
                    }
                    else if (part.Type == ContentPartType.Image)
                    {
                        parts.Add(new
                        {
                            inline_data = new
                            {
                                mime_type = part.Image.MimeType,
                                data = part.Image.Base64Data
                            }
                        });
                    }
                    else if (part.Type == ContentPartType.Audio)
                    {
                        parts.Add(new
                        {
                            inline_data = new
                            {
                                mime_type = part.Audio.MimeType,
                                data = part.Audio.Base64Data
                            }
                        });
                    }
                }

                contents.Add(new { role, parts });
            }
            else
            {
                contents.Add(new
                {
                    role,
                    parts = new[] { new { text = msg.Content } }
                });
            }
        }

        var payload = new Dictionary<string, object>
        {
            ["contents"] = contents
        };

        if (systemInstruction != null)
        {
            payload["system_instruction"] = systemInstruction;
        }

        var generationConfig = new Dictionary<string, object>();

        if (request.Temperature.HasValue)
        {
            generationConfig["temperature"] = request.Temperature.Value;
        }

        if (request.MaxTokens.HasValue)
        {
            generationConfig["maxOutputTokens"] = request.MaxTokens.Value;
        }

        if (request.JsonMode == true)
        {
            generationConfig["response_mime_type"] = "application/json";
        }

        if (generationConfig.Any())
        {
            payload["generationConfig"] = generationConfig;
        }

        // Function calling (tools)
        if (request.Tools != null && request.Tools.Any())
        {
            payload["tools"] = new[]
            {
                new
                {
                    function_declarations = request.Tools.Select(t => new
                    {
                        name = t.Function.Name,
                        description = t.Function.Description,
                        parameters = t.Function.Parameters
                    }).ToList()
                }
            };
        }

        return payload;
    }

    protected override async Task<HttpResponseMessage> SendRequestAsync(
        object request,
        CancellationToken ct)
    {
        var model = _config.DefaultModel ?? "gemini-1.5-pro";
        if (model.StartsWith("models/")) model = model.Substring(7);
        var endpoint = $"/v1beta/models/{model}:generateContent?key={_config.ApiKey}";

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        _logger.LogDebug("[{Provider}] Request: {Json}", ProviderName, json);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await _httpClient.PostAsync(endpoint, content, ct);
    }

    protected override object ParseProviderResponse(string json)
    {
        return JsonSerializer.Deserialize<GeminiResponse>(json);
    }

    protected override async Task<InferenceResponse> MapToUnifiedResponse(object providerResponse)
    {
        var gemini = (GeminiResponse)providerResponse;

        var candidate = gemini.Candidates?.FirstOrDefault();
        if (candidate == null)
        {
            throw new ProviderException(
                ProviderName,
                "No candidates in response",
                errorType: ProviderErrorType.ParseError);
        }

        // Extract text content
        var textParts = candidate.Content?.Parts?
            .Where(p => !string.IsNullOrEmpty(p.Text))
            .Select(p => p.Text)
            .ToList();

        var content = textParts != null && textParts.Any()
            ? string.Join("", textParts)
            : null;

        // Extract function calls
        var toolCalls = candidate.Content?.Parts?
            .Where(p => p.FunctionCall != null)
            .Select(p => new ToolCall
            {
                Id = Guid.NewGuid().ToString(),
                Type = "function",
                Function = new FunctionCall
                {
                    Name = p.FunctionCall.Name,
                    Arguments = JsonSerializer.Serialize(p.FunctionCall.Args)
                }
            })
            .ToList();

        // Calculate cost
        var modelInfo = _modelRegistry.GetModel(_config.ProviderType, _config.DefaultModel);
        decimal? estimatedCost = null;

        if (modelInfo != null && gemini.UsageMetadata != null)
        {
            var inputCost = (gemini.UsageMetadata.PromptTokenCount / 1_000_000m) * modelInfo.Pricing.InputPer1MTokens;
            var outputCost = (gemini.UsageMetadata.CandidatesTokenCount / 1_000_000m) * modelInfo.Pricing.OutputPer1MTokens;
            estimatedCost = inputCost + outputCost;
        }

        return new InferenceResponse
        {
            Id = Guid.NewGuid().ToString(),
            Model = _config.DefaultModel,
            Content = content,
            FinishReason = candidate.FinishReason,
            ToolCalls = toolCalls,
            Usage = new UsageInfo
            {
                PromptTokens = gemini.UsageMetadata?.PromptTokenCount ?? 0,
                CompletionTokens = gemini.UsageMetadata?.CandidatesTokenCount ?? 0,
                TotalTokens = gemini.UsageMetadata?.TotalTokenCount ?? 0,
                EstimatedCost = estimatedCost
            }
        };
    }

    public override async IAsyncEnumerable<StreamChunk> StreamAsync(
        InferenceRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        //ValidateRequest(request);

        var model = _config.DefaultModel ?? "gemini-1.5-pro";
        if (model.StartsWith("models/")) model = model.Substring(7);
        var endpoint = $"/v1beta/models/{model}:streamGenerateContent?key={_config.ApiKey}&alt=sse";

        var providerRequest = MapToProviderRequest(request);
        var json = JsonSerializer.Serialize(providerRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
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
                var chunk = JsonSerializer.Deserialize<GeminiStreamChunk>(line);

                var candidate = chunk?.Candidates?.FirstOrDefault();
                if (candidate != null)
                {
                    // Text content
                    var textPart = candidate.Content?.Parts?.FirstOrDefault(p => p.Text != null);
                    if (!string.IsNullOrEmpty(textPart?.Text))
                    {
                        chunks.Add(new StreamChunk
                        {
                            Content = textPart.Text,
                            Type = StreamChunkType.ContentDelta,
                            IsComplete = false
                        });
                    }

                    // Function calls
                    var functionCallPart = candidate.Content?.Parts?.FirstOrDefault(p => p.FunctionCall != null);
                    if (functionCallPart?.FunctionCall != null)
                    {
                        chunks.Add(new StreamChunk
                        {
                            Type = StreamChunkType.ToolCallStart,
                            Metadata = new Dictionary<string, object>
                            {
                                ["tool_name"] = functionCallPart.FunctionCall.Name ?? "",
                                ["tool_args"] = functionCallPart.FunctionCall.Args ?? new Dictionary<string, object>()
                            }
                        });
                    }

                    if (!string.IsNullOrEmpty(candidate.FinishReason))
                    {
                        chunks.Add(new StreamChunk
                        {
                            Type = StreamChunkType.Complete,
                            IsComplete = true
                        });
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "[Gemini] Failed to parse stream chunk: {Line}", line);
            }

            foreach (var c in chunks)
            {
                yield return c;
            }
        }
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

        var model = request.Model ?? "text-embedding-004";

        // Gemini embedding is different - batch or single
        var inputs = request.Inputs?.Any() == true
            ? request.Inputs
            : new List<string> { request.Input };

        var embeddings = new List<EmbeddingData>();

        // Gemini processes one at a time or in batch
        if (inputs.Count == 1)
        {
            var embedding = await GetSingleEmbedding(model, inputs[0], ct);
            embeddings.Add(new EmbeddingData
            {
                Index = 0,
                Vector = embedding
            });
        }
        else
        {
            var batchResult = await GetBatchEmbeddings(model, inputs, ct);
            embeddings.AddRange(batchResult.Select((emb, idx) => new EmbeddingData
            {
                Index = idx,
                Vector = emb
            }));
        }

        return new EmbeddingResponse
        {
            Model = model,
            Embeddings = embeddings,
            Usage = new UsageInfo
            {
                PromptTokens = inputs.Sum(i => i.Length / 4), // Rough estimate
                TotalTokens = inputs.Sum(i => i.Length / 4),
                EstimatedCost = 0m // Free for now
            }
        };
    }

    private async Task<List<float>> GetSingleEmbedding(string model, string text, CancellationToken ct)
    {
        if (model.StartsWith("models/")) model = model.Substring(7);
        var payload = new
        {
            model = $"models/{model}",
            content = new
            {
                parts = new[] { new { text } }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var endpoint = $"/v1beta/models/{model}:embedContent?key={_config.ApiKey}";
        var response = await _httpClient.PostAsync(endpoint, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<GeminiEmbeddingResponse>(responseJson);

        return result.Embedding.Values.ToList();
    }

    private async Task<List<List<float>>> GetBatchEmbeddings(
        string model,
        List<string> texts,
        CancellationToken ct)
    {
        var payload = new
        {
            requests = texts.Select(text => new
            {
                model = $"models/{model}",
                content = new
                {
                    parts = new[] { new { text } }
                }
            }).ToList()
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var endpoint = $"/v1beta/models/{model}:batchEmbedContents?key={_config.ApiKey}";
        var response = await _httpClient.PostAsync(endpoint, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponse(response);
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<GeminiBatchEmbeddingResponse>(responseJson);

        return result.Embeddings.Select(e => e.Values.ToList()).ToList();
    }

    private class GeminiEmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public EmbeddingValues Embedding { get; set; }
    }

    private class GeminiBatchEmbeddingResponse
    {
        [JsonPropertyName("embeddings")]
        public List<EmbeddingValues> Embeddings { get; set; }
    }

    private class EmbeddingValues
    {
        [JsonPropertyName("values")]
        public List<float> Values { get; set; }
    }

    // Gemini-specific DTOs
    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public UsageMetadata UsageMetadata { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string FinishReason { get; set; }
    }

    private class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("functionCall")]
        public FunctionCallData FunctionCall { get; set; }
    }

    private class FunctionCallData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("args")]
        public Dictionary<string, object> Args { get; set; }
    }

    private class UsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }

        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }

    private class GeminiStreamChunk
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }
}