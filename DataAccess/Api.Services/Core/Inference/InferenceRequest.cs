namespace Api.Services.Core.Inference;
// Api.Services.Core/Inference/InferenceRequest.cs
public class InferenceRequest
{
    public string Model { get; set; }
    public List<Message> Messages { get; set; } = new();
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public bool Stream { get; set; } = true;
    public string SystemPrompt { get; set; }
    public List<Tool> Tools { get; set; }
    public object ToolChoice { get; set; } // "auto", "none", or specific tool
    public bool? JsonMode { get; set; }

    // Metadata
    public string SessionId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class Message
{
    public string Role { get; set; } // system, user, assistant, tool
    public string Content { get; set; }
    public List<ContentPart> Parts { get; set; } // For multimodal
    public string Name { get; set; } // For tool results
    public string ToolCallId { get; set; } // For tool results
}

public class ContentPart
{
    public ContentPartType Type { get; set; }
    public string Text { get; set; }
    public ImageContent Image { get; set; }
    public AudioContent Audio { get; set; }
}

public enum ContentPartType
{
    Text,
    Image,
    Audio
}

public class ImageContent
{
    public string Base64Data { get; set; }
    public string MimeType { get; set; } // image/png, image/jpeg, etc
    public string Url { get; set; } // Alternative to base64
}

public class AudioContent
{
    public string Base64Data { get; set; }
    public string MimeType { get; set; } // audio/wav, audio/mp3, etc
}

public class Tool
{
    public string Type { get; set; } = "function";
    public FunctionDefinition Function { get; set; }
}

public class FunctionDefinition
{
    public string Name { get; set; }
    public string Description { get; set; }
    public object Parameters { get; set; } // JSON Schema
}
