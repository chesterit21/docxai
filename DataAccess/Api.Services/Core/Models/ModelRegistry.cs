namespace Api.Services.Core.Models;

// Api.Services.Core/Models/ModelRegistry.cs
public class ModelRegistry : IModelRegistry
{
    private readonly Dictionary<string, List<ModelInfo>> _models;

    public ModelRegistry()
    {
        _models = InitializeModels();
    }

    private Dictionary<string, List<ModelInfo>> InitializeModels()
    {
        return new Dictionary<string, List<ModelInfo>>
        {
            ["claude"] = new List<ModelInfo>
            {
                new()
                {
                    ModelId = "claude-opus-4-20250514",
                    DisplayName = "Claude Opus 4",
                    ProviderType = "claude",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = true,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = true,
                        SupportsJsonMode = false,
                        SupportedImageFormats = new() { "png", "jpg", "jpeg", "webp", "gif" }
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 200000,
                        MaxOutputTokens = 16384,
                        MaxImagesPerRequest = 20,
                        MaxImageSizeBytes = 10 * 1024 * 1024, // 10MB
                        RpmLimit = 50,
                        TpmLimit = 400000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 15.0m,
                        OutputPer1MTokens = 75.0m
                    },
                    IsAvailable = true,
                    Description = "Most powerful Claude model for complex tasks"
                },
                new()
                {
                    ModelId = "claude-sonnet-4-20250514",
                    DisplayName = "Claude Sonnet 4",
                    ProviderType = "claude",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = true,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = true,
                        SupportsJsonMode = false,
                        SupportedImageFormats = new() { "png", "jpg", "jpeg", "webp", "gif" }
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 200000,
                        MaxOutputTokens = 16384,
                        MaxImagesPerRequest = 20,
                        MaxImageSizeBytes = 10 * 1024 * 1024,
                        RpmLimit = 50,
                        TpmLimit = 400000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 3.0m,
                        OutputPer1MTokens = 15.0m
                    },
                    IsAvailable = true,
                    Description = "Balanced performance and cost"
                },
                new()
                {
                    ModelId = "claude-haiku-4-20250115",
                    DisplayName = "Claude Haiku 4",
                    ProviderType = "claude",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = true,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = true,
                        SupportsJsonMode = false,
                        SupportedImageFormats = new() { "png", "jpg", "jpeg", "webp", "gif" }
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 200000,
                        MaxOutputTokens = 8192,
                        MaxImagesPerRequest = 20,
                        MaxImageSizeBytes = 10 * 1024 * 1024,
                        RpmLimit = 50,
                        TpmLimit = 500000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.8m,
                        OutputPer1MTokens = 4.0m
                    },
                    IsAvailable = true,
                    Description = "Fast and cost-effective"
                }
            },

            ["openai"] = new List<ModelInfo>
            {
                new()
                {
                    ModelId = "gpt-4o",
                    DisplayName = "GPT-4o",
                    ProviderType = "openai",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = true,
                        SupportsAudio = true,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = true,
                        SupportsJsonMode = true,
                        SupportedImageFormats = new() { "png", "jpg", "jpeg", "webp", "gif" }
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 128000,
                        MaxOutputTokens = 16384,
                        MaxImagesPerRequest = 10,
                        MaxImageSizeBytes = 20 * 1024 * 1024,
                        RpmLimit = 500,
                        TpmLimit = 800000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 2.5m,
                        OutputPer1MTokens = 10.0m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "gpt-4-turbo",
                    DisplayName = "GPT-4 Turbo",
                    ProviderType = "openai",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = true,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = true,
                        SupportsJsonMode = true,
                        SupportedImageFormats = new() { "png", "jpg", "jpeg", "webp", "gif" }
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 128000,
                        MaxOutputTokens = 4096,
                        MaxImagesPerRequest = 10,
                        MaxImageSizeBytes = 20 * 1024 * 1024,
                        RpmLimit = 500,
                        TpmLimit = 600000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 10.0m,
                        OutputPer1MTokens = 30.0m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "gpt-3.5-turbo",
                    DisplayName = "GPT-3.5 Turbo",
                    ProviderType = "openai",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = false,
                        SupportsJsonMode = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 16385,
                        MaxOutputTokens = 4096,
                        RpmLimit = 3500,
                        TpmLimit = 200000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.5m,
                        OutputPer1MTokens = 1.5m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "text-embedding-3-large",
                    DisplayName = "Text Embedding 3 Large",
                    ProviderType = "openai",
                    Capabilities = new()
                    {
                        SupportsChat = false,
                        SupportsStreaming = false,
                        SupportsEmbedding = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsFunctionCalling = false,
                        SupportsSystemPrompt = false
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 8191,
                        RpmLimit = 5000,
                        TpmLimit = 5000000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.13m,
                        OutputPer1MTokens = 0m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "text-embedding-3-small",
                    DisplayName = "Text Embedding 3 Small",
                    ProviderType = "openai",
                    Capabilities = new()
                    {
                        SupportsChat = false,
                        SupportsStreaming = false,
                        SupportsEmbedding = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsFunctionCalling = false,
                        SupportsSystemPrompt = false
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 8191,
                        RpmLimit = 5000,
                        TpmLimit = 5000000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.02m,
                        OutputPer1MTokens = 0m
                    },
                    IsAvailable = true
                }
            },

            ["gemini"] = new List<ModelInfo>
            {
                new()
                {
                    ModelId = "gemini-2.0-flash-exp",
                    DisplayName = "Gemini 2.0 Flash",
                    ProviderType = "gemini",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = true,
                        SupportsAudio = true,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = true,
                        SupportsJsonMode = true,
                        SupportedImageFormats = new() { "png", "jpg", "jpeg", "webp" },
                        SupportedAudioFormats = new() { "wav", "mp3", "aiff", "aac", "ogg", "flac" }
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 1000000,
                        MaxOutputTokens = 8192,
                        MaxImagesPerRequest = 16,
                        MaxImageSizeBytes = 4 * 1024 * 1024,
                        RpmLimit = 1000,
                        TpmLimit = 4000000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0m, // Free for now
                        OutputPer1MTokens = 0m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "gemini-1.5-pro",
                    DisplayName = "Gemini 1.5 Pro",
                    ProviderType = "gemini",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = true,
                        SupportsAudio = true,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = true,
                        SupportsJsonMode = true,
                        SupportedImageFormats = new() { "png", "jpg", "jpeg", "webp" },
                        SupportedAudioFormats = new() { "wav", "mp3", "aiff", "aac", "ogg", "flac" }
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 2000000,
                        MaxOutputTokens = 8192,
                        MaxImagesPerRequest = 16,
                        MaxImageSizeBytes = 4 * 1024 * 1024,
                        RpmLimit = 1000,
                        TpmLimit = 4000000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 1.25m,
                        OutputPer1MTokens = 5.0m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "gemini-1.5-flash",
                    DisplayName = "Gemini 1.5 Flash",
                    ProviderType = "gemini",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = true,
                        SupportsAudio = true,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = true,
                        SupportsJsonMode = true,
                        SupportedImageFormats = new() { "png", "jpg", "jpeg", "webp" },
                        SupportedAudioFormats = new() { "wav", "mp3", "aiff", "aac", "ogg", "flac" }
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 1000000,
                        MaxOutputTokens = 8192,
                        MaxImagesPerRequest = 16,
                        MaxImageSizeBytes = 4 * 1024 * 1024,
                        RpmLimit = 1500,
                        TpmLimit = 4000000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.075m,
                        OutputPer1MTokens = 0.30m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "text-embedding-004",
                    DisplayName = "Text Embedding 004",
                    ProviderType = "gemini",
                    Capabilities = new()
                    {
                        SupportsChat = false,
                        SupportsEmbedding = true,
                        SupportsStreaming = false
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 2048,
                        RpmLimit = 1500
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0m, // Free
                        OutputPer1MTokens = 0m
                    },
                    IsAvailable = true
                }
            },

            ["deepseek"] = new List<ModelInfo>
            {
                new()
                {
                    ModelId = "deepseek-chat",
                    DisplayName = "DeepSeek Chat",
                    ProviderType = "deepseek",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsJsonMode = false
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 64000,
                        MaxOutputTokens = 4096,
                        RpmLimit = 60,
                        TpmLimit = 200000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.14m,
                        OutputPer1MTokens = 0.28m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "deepseek-coder",
                    DisplayName = "DeepSeek Coder",
                    ProviderType = "deepseek",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsJsonMode = false
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 64000,
                        MaxOutputTokens = 4096,
                        RpmLimit = 60,
                        TpmLimit = 200000
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.14m,
                        OutputPer1MTokens = 0.28m
                    },
                    IsAvailable = true
                }
            },

            ["qwen"] = new List<ModelInfo>
            {
                new()
                {
                    ModelId = "qwen-max",
                    DisplayName = "Qwen Max",
                    ProviderType = "qwen",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = true,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsMultimodal = true,
                        SupportedImageFormats = new() { "png", "jpg", "jpeg", "webp" }
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 32000,
                        MaxOutputTokens = 6000,
                        MaxImagesPerRequest = 10,
                        RpmLimit = 60
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.4m,
                        OutputPer1MTokens = 1.2m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "qwen-plus",
                    DisplayName = "Qwen Plus",
                    ProviderType = "qwen",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 32000,
                        MaxOutputTokens = 6000,
                        RpmLimit = 60
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.08m,
                        OutputPer1MTokens = 0.24m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "qwen-turbo",
                    DisplayName = "Qwen Turbo",
                    ProviderType = "qwen",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 8000,
                        MaxOutputTokens = 2000,
                        RpmLimit = 60
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.03m,
                        OutputPer1MTokens = 0.06m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "text-embedding-v2",
                    DisplayName = "Text Embedding V2",
                    ProviderType = "qwen",
                    Capabilities = new()
                    {
                        SupportsChat = false,
                        SupportsEmbedding = true,
                        SupportsStreaming = false
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 2048,
                        RpmLimit = 100
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.007m,
                        OutputPer1MTokens = 0m
                    },
                    IsAvailable = true
                }
            },

            ["perplexity"] = new List<ModelInfo>
            {
                new()
                {
                    ModelId = "llama-3.1-sonar-large-128k-online",
                    DisplayName = "Sonar Large Online",
                    ProviderType = "perplexity",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = false,
                        SupportsSystemPrompt = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 127000,
                        MaxOutputTokens = 4096,
                        RpmLimit = 50
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 1.0m,
                        OutputPer1MTokens = 1.0m
                    },
                    IsAvailable = true,
                    Description = "With web search capabilities"
                },
                new()
                {
                    ModelId = "llama-3.1-sonar-small-128k-online",
                    DisplayName = "Sonar Small Online",
                    ProviderType = "perplexity",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = false,
                        SupportsSystemPrompt = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 127000,
                        MaxOutputTokens = 4096,
                        RpmLimit = 50
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.2m,
                        OutputPer1MTokens = 0.2m
                    },
                    IsAvailable = true,
                    Description = "With web search capabilities"
                }
            },

            ["kimi"] = new List<ModelInfo>
            {
                new()
                {
                    ModelId = "moonshot-v1-128k",
                    DisplayName = "Moonshot 128K",
                    ProviderType = "kimi",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 128000,
                        MaxOutputTokens = 4096,
                        RpmLimit = 60
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.14m,
                        OutputPer1MTokens = 0.14m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "moonshot-v1-32k",
                    DisplayName = "Moonshot 32K",
                    ProviderType = "kimi",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 32000,
                        MaxOutputTokens = 4096,
                        RpmLimit = 60
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.07m,
                        OutputPer1MTokens = 0.07m
                    },
                    IsAvailable = true
                },
                new()
                {
                    ModelId = "moonshot-v1-8k",
                    DisplayName = "Moonshot 8K",
                    ProviderType = "kimi",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 8000,
                        MaxOutputTokens = 4096,
                        RpmLimit = 60
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.014m,
                        OutputPer1MTokens = 0.014m
                    },
                    IsAvailable = true
                }
            },

            ["groq"] = new List<ModelInfo>
            {
                new()
                {
                    ModelId = "llama-3.3-70b-versatile",
                    DisplayName = "Llama 3.3 70B",
                    ProviderType = "groq",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsJsonMode = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 32768,
                        MaxOutputTokens = 8192,
                        RpmLimit = 30,
                        TpmLimit = 14400
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.59m,
                        OutputPer1MTokens = 0.79m
                    },
                    IsAvailable = true,
                    Description = "Very fast inference speed"
                },
                new()
                {
                    ModelId = "mixtral-8x7b-32768",
                    DisplayName = "Mixtral 8x7B",
                    ProviderType = "groq",
                    Capabilities = new()
                    {
                        SupportsChat = true,
                        SupportsStreaming = true,
                        SupportsVision = false,
                        SupportsAudio = false,
                        SupportsEmbedding = false,
                        SupportsFunctionCalling = true,
                        SupportsSystemPrompt = true,
                        SupportsJsonMode = true
                    },
                    Limits = new()
                    {
                        MaxContextTokens = 32768,
                        MaxOutputTokens = 8192,
                        RpmLimit = 30,
                        TpmLimit = 14400
                    },
                    Pricing = new()
                    {
                        InputPer1MTokens = 0.27m,
                        OutputPer1MTokens = 0.27m
                    },
                    IsAvailable = true,
                    Description = "Very fast inference speed"
                }
            }
        };
    }

    public ModelInfo GetModel(string providerType, string modelId)
    {
        if (!_models.ContainsKey(providerType.ToLower()))
            return null;

        return _models[providerType.ToLower()]
            .FirstOrDefault(m => m.ModelId == modelId);
    }

    public List<ModelInfo> GetAvailableModels(string providerType)
    {
        if (!_models.ContainsKey(providerType.ToLower()))
            return new List<ModelInfo>();

        return _models[providerType.ToLower()]
            .Where(m => m.IsAvailable)
            .ToList();
    }

    public List<ModelInfo> GetModelsByCapability(ModelCapabilityFilter filter)
    {
        var allModels = _models.Values.SelectMany(m => m).Where(m => m.IsAvailable);

        if (filter.RequiresVision.HasValue)
            allModels = allModels.Where(m => m.Capabilities.SupportsVision == filter.RequiresVision.Value);

        if (filter.RequiresEmbedding.HasValue)
            allModels = allModels.Where(m => m.Capabilities.SupportsEmbedding == filter.RequiresEmbedding.Value);

        if (filter.RequiresFunctionCalling.HasValue)
            allModels = allModels.Where(m => m.Capabilities.SupportsFunctionCalling == filter.RequiresFunctionCalling.Value);

        if (filter.RequiresStreaming.HasValue)
            allModels = allModels.Where(m => m.Capabilities.SupportsStreaming == filter.RequiresStreaming.Value);

        if (filter.MinContextTokens.HasValue)
            allModels = allModels.Where(m => m.Limits.MaxContextTokens >= filter.MinContextTokens.Value);

        return allModels.ToList();
    }

    public bool IsModelAvailable(string providerType, string modelId)
    {
        var model = GetModel(providerType, modelId);
        return model?.IsAvailable ?? false;
    }
}