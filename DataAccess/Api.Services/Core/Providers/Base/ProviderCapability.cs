namespace Api.Services.Core.Providers.Base;

/// <summary>
/// Capability flags per provider - describes what features each LLM provider supports.
/// </summary>
public class ProviderCapability
{
    public bool SupportsStreaming { get; init; }
    public bool SupportsVision { get; init; }
    public bool SupportsAudio { get; init; }
    public bool SupportsEmbedding { get; init; }
    public bool SupportsFunctionCalling { get; init; }
    public bool SupportsSystemPrompt { get; init; }
    public int MaxContextTokens { get; init; }
    public List<string> SupportedModels { get; init; } = new();
}