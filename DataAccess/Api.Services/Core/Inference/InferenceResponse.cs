namespace Api.Services.Core.Inference;

// Api.Services.Core/Inference/InferenceResponse.cs
public class InferenceResponse
{
    public string Id { get; set; }
    public string Model { get; set; }
    public string Content { get; set; }
    public string FinishReason { get; set; }
    public UsageInfo Usage { get; set; }
    public List<ToolCall> ToolCalls { get; set; }
    public string RawResponse { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class UsageInfo
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public decimal? EstimatedCost { get; set; }
}

public class ToolCall
{
    public string Id { get; set; }
    public string Type { get; set; } = "function";
    public FunctionCall Function { get; set; }
}

public class FunctionCall
{
    public string Name { get; set; }
    public string Arguments { get; set; } // JSON string
}