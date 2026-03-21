namespace Api.Services.Core.Models;
// Api.Services.Core/Models/ModelInfo.cs
public class ModelInfo
{
    public string ModelId { get; set; }
    public string DisplayName { get; set; }
    public string ProviderType { get; set; }
    public ModelCapabilities Capabilities { get; set; }
    public ModelPricing Pricing { get; set; }
    public ModelLimits Limits { get; set; }
    public bool IsAvailable { get; set; }
    public string Description { get; set; }
}

public class ModelCapabilities
{
    public bool SupportsChat { get; set; } = true;
    public bool SupportsStreaming { get; set; }
    public bool SupportsVision { get; set; }
    public bool SupportsAudio { get; set; }
    public bool SupportsEmbedding { get; set; }
    public bool SupportsFunctionCalling { get; set; }
    public bool SupportsSystemPrompt { get; set; }
    public bool SupportsMultimodal { get; set; }
    public bool SupportsJsonMode { get; set; }
    public List<string> SupportedImageFormats { get; set; } = new();
    public List<string> SupportedAudioFormats { get; set; } = new();
}

public class ModelLimits
{
    public int MaxContextTokens { get; set; }
    public int MaxOutputTokens { get; set; }
    public int MaxImagesPerRequest { get; set; }
    public long MaxImageSizeBytes { get; set; }
    public int RpmLimit { get; set; } // Requests per minute
    public int TpmLimit { get; set; } // Tokens per minute
}

public class ModelPricing
{
    public decimal InputPer1MTokens { get; set; }
    public decimal OutputPer1MTokens { get; set; }
    public decimal ImageInputPrice { get; set; }
    public string Currency { get; set; } = "USD";
}

