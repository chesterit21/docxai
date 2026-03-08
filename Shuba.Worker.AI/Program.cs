using Api.Repository;
using Shuba.Worker.AI;
using Shuba.Worker.AI.Models;
using Shuba.Worker.AI.Services;

var builder = Host.CreateApplicationBuilder(args);

// Bind Worker Settings
var workerSettings = new WorkerSettings
{
    CdpUrl = builder.Configuration.GetValue<string>("Cdp:Url") ?? "http://localhost:9222",
    DeepSeek = new ProviderConfig
    {
        Name = builder.Configuration.GetValue<string>("Providers:DeepSeek:Name") ?? "DeepSeek",
        Url = builder.Configuration.GetValue<string>("Providers:DeepSeek:Url") ?? "https://chat.deepseek.com/"
    },
    Qwen = new ProviderConfig
    {
        Name = builder.Configuration.GetValue<string>("Providers:Qwen:Name") ?? "Qwen",
        Url = builder.Configuration.GetValue<string>("Providers:Qwen:Url") ?? "https://chat.qwen.ai/"
    },
    ZAi = new ProviderConfig
    {
        Name = builder.Configuration.GetValue<string>("Providers:ZAi:Name") ?? "Z-AI",
        Url = builder.Configuration.GetValue<string>("Providers:ZAi:Url") ?? "https://chat.z.ai/"
    },
    SessionResetAfterNRequests = builder.Configuration.GetValue<int>("Settings:SessionResetAfterNRequests"),
    MaxRetryPerExtraction = builder.Configuration.GetValue<int>("Settings:MaxRetryPerExtraction")
};

if (workerSettings.SessionResetAfterNRequests == 0) workerSettings.SessionResetAfterNRequests = 10;
if (workerSettings.MaxRetryPerExtraction == 0) workerSettings.MaxRetryPerExtraction = 3;

// Register DB & Repositories (reuse existing DI from Api.Repository)
builder.Services.RegisterRepository(builder.Configuration);

// Register Singletons (long-lived CDP connection)
builder.Services.AddSingleton(workerSettings);
builder.Services.AddSingleton(new ProviderManager(workerSettings));
builder.Services.AddSingleton(new WebAiOrchestrator(workerSettings.CdpUrl, workerSettings));

// Register Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
