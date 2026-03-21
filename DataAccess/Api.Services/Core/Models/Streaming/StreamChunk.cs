// Api.Services.Core/Models/Streaming/StreamChunk.cs
namespace Api.Services.Core.Models.Streaming;

public class StreamChunk
{
    public string Content { get; set; }
    public bool IsComplete { get; set; }
    public string Delta { get; set; }
    public StreamChunkType Type { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

public enum StreamChunkType
{
    ContentDelta,
    ToolCallStart,
    ToolCallDelta,
    Complete,
    Error
}
