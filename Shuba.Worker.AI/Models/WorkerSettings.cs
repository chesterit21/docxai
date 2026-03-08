namespace Shuba.Worker.AI.Models;

public class WorkerSettings
{
    public string CdpUrl { get; set; } = string.Empty;
    public ProviderConfig DeepSeek { get; set; } = new();
    public ProviderConfig Qwen { get; set; } = new();
    public ProviderConfig ZAi { get; set; } = new();
    public int SessionResetAfterNRequests { get; set; } = 10;
    public int MaxRetryPerExtraction { get; set; } = 3;
}

public class ProviderConfig
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
