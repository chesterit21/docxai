using Api.Repository;
using Shuba.Worker.AI;
using Shuba.Worker.AI.Models;
using Shuba.Worker.AI.Services;

var builder = Host.CreateApplicationBuilder(args);

// Bind Worker Settings
var workerSettings = new WorkerSettings
{
    CdpUrl = builder.Configuration.GetValue<string>("Cdp:Url") ?? "http://localhost:9222",
    SessionResetAfterNRequests = builder.Configuration.GetValue<int>("Settings:SessionResetAfterNRequests"),
    MaxRetryPerExtraction = builder.Configuration.GetValue<int>("Settings:MaxRetryPerExtraction")
};

// Bind providers (including Selectors arrays from appsettings.json)
builder.Configuration.GetSection("Providers:DeepSeek").Bind(workerSettings.DeepSeek);
builder.Configuration.GetSection("Providers:Qwen").Bind(workerSettings.Qwen);
builder.Configuration.GetSection("Providers:ZAi").Bind(workerSettings.ZAi);

// Fallback defaults for Name/Url if not set
if (string.IsNullOrEmpty(workerSettings.DeepSeek.Name)) workerSettings.DeepSeek.Name = "DeepSeek";
if (string.IsNullOrEmpty(workerSettings.DeepSeek.Url)) workerSettings.DeepSeek.Url = "https://chat.deepseek.com/";
if (string.IsNullOrEmpty(workerSettings.Qwen.Name)) workerSettings.Qwen.Name = "Qwen";
if (string.IsNullOrEmpty(workerSettings.Qwen.Url)) workerSettings.Qwen.Url = "https://chat.qwen.ai/";
if (string.IsNullOrEmpty(workerSettings.ZAi.Name)) workerSettings.ZAi.Name = "Z-AI";
if (string.IsNullOrEmpty(workerSettings.ZAi.Url)) workerSettings.ZAi.Url = "https://chat.z.ai/";

if (workerSettings.SessionResetAfterNRequests == 0) workerSettings.SessionResetAfterNRequests = 10;
if (workerSettings.MaxRetryPerExtraction == 0) workerSettings.MaxRetryPerExtraction = 3;

// Register IHttpContextAccessor (required by repositories, not auto-registered in worker)
builder.Services.AddHttpContextAccessor();

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
