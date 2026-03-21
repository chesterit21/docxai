namespace Api.Services.Core.Inference;

/// <summary>
/// Request model for generating embeddings.
/// </summary>
public class EmbeddingRequest
{
    public string? Model { get; set; }
    public List<string> Inputs { get; set; } = new(); // Support batch embedding
    public string? Input { get; set; } // Single input convenience
    public int? Dimensions { get; set; } // For models that support custom dimensions
    public string? EncodingFormat { get; set; } // "float" or "base64"
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Response model for embedding results.
/// </summary>
public class EmbeddingResponse
{
    public string? Model { get; set; }
    public List<EmbeddingData> Embeddings { get; set; } = new();
    public UsageInfo? Usage { get; set; }
    public string? RawResponse { get; set; }
}

public class EmbeddingData
{
    public int Index { get; set; }
    public List<float> Vector { get; set; } = new();
    public string? Base64 { get; set; } // If requested in base64 format
}