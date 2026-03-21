namespace Api.Services.Core.Models;
// Api.Services.Core/Models/IModelRegistry.cs
public interface IModelRegistry
{
    ModelInfo? GetModel(string providerType, string modelId);
    List<ModelInfo> GetAvailableModels(string providerType);
    List<ModelInfo> GetModelsByCapability(ModelCapabilityFilter filter);
    bool IsModelAvailable(string providerType, string modelId);
}

public class ModelCapabilityFilter
{
    public bool? RequiresVision { get; set; }
    public bool? RequiresEmbedding { get; set; }
    public bool? RequiresFunctionCalling { get; set; }
    public bool? RequiresStreaming { get; set; }
    public int? MinContextTokens { get; set; }
}
