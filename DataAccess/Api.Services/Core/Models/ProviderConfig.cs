using System.Collections.Generic;

namespace Api.Services.Core.Models;

public class ProviderConfig
{
    public string ProviderType { get; set; }
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public string DefaultModel { get; set; }
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 2;
    public string Endpoint { get; set; } // Provider-specific endpoint
    public Dictionary<string, object> Metadata { get; set; } = new();
}
