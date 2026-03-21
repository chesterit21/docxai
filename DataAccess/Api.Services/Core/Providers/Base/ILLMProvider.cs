using Api.Services.Core.Inference;
using Api.Services.Core.Models.Streaming;

namespace Api.Services.Core.Providers.Base;

/// <summary>
/// Interface for LLM provider adapters.
/// </summary>
public interface ILLMProvider
{
    string ProviderName { get; }
    ProviderCapability Capabilities { get; }

    Task<InferenceResponse> InferNonStreamingAsync(
        InferenceRequest request,
        CancellationToken ct = default
    );

    IAsyncEnumerable<StreamChunk> StreamAsync(
        InferenceRequest request,
        CancellationToken ct = default
    );

    Task<bool> ValidateAsync(CancellationToken ct = default);

    Task<EmbeddingResponse> EmbedAsync(
        EmbeddingRequest request,
        CancellationToken ct = default
    );
}
