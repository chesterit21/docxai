using Api.DataAccess.Models.Dms;
using Api.Repository.Dms;
using Microsoft.Extensions.DependencyInjection;
using Shuba.Worker.AI.Services;
using Shuba.Worker.AI.Workflows;

namespace Shuba.Worker.AI;

public class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory scopeFactory,
    ProviderManager providerManager,
    WebAiOrchestrator orchestrator) : BackgroundService
{
    private const int MAX_CONCURRENT_TASKS = 3;
    private const int POLLING_INTERVAL_MS = 3000; // 3 detik
    private readonly SemaphoreSlim _concurrencyLimiter = new(MAX_CONCURRENT_TASKS, MAX_CONCURRENT_TASKS);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("╔══════════════════════════════════════════════╗");
        logger.LogInformation("║  Shuba.Worker.AI — Document Extractor       ║");
        logger.LogInformation("║  Max Concurrent: {Max}  |  Poll: {Poll}ms        ║", MAX_CONCURRENT_TASKS, POLLING_INTERVAL_MS);
        logger.LogInformation("╚══════════════════════════════════════════════╝");

        // Connect CDP saat startup
        try
        {
            await orchestrator.ConnectAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Worker] ✗ Failed to connect CDP. Worker will retry on next cycle.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollAndProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Worker] ✗ Unhandled error in poll cycle.");
            }

            await Task.Delay(POLLING_INTERVAL_MS, stoppingToken);
        }

        logger.LogInformation("[Worker] 🛑 Shutting down...");
    }

    private async Task PollAndProcessAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var pollingRepo = scope.ServiceProvider.GetRequiredService<IAgentPollingTaskDocumentRepository>();

        // Ambil max 3 task yang inqueue
        var pendingTasks = await pollingRepo.GetAsync(
            x => x.Status == "inqueue" && x.TaskCode == "Init-New-Doc");

        if (pendingTasks == null || !pendingTasks.Any())
            return;

        // Limit ke max 3
        var tasksToProcess = pendingTasks.Take(MAX_CONCURRENT_TASKS).ToList();

        logger.LogInformation("[Worker] 📋 Found {Count} inqueue tasks, processing {Take}.",
            pendingTasks.Count, tasksToProcess.Count);

        // Update semua ke "running" terlebih dahulu
        foreach (var task in tasksToProcess)
        {
            task.Status = "running";
            task.UpdatedAt = DateTime.Now;
            await pollingRepo.UpdateAsync(task);
        }

        // Process secara parallel
        var processingTasks = new List<Task>();
        foreach (var task in tasksToProcess)
        {
            var providerSlot = providerManager.AcquireIdleProvider();
            if (providerSlot == null)
            {
                logger.LogWarning("[Worker] ⚠️ No idle provider for TaskId={Id}. Will retry next cycle.", task.Id);
                // Revert to inqueue
                task.Status = "inqueue";
                task.UpdatedAt = DateTime.Now;
                await pollingRepo.UpdateAsync(task);
                continue;
            }

            processingTasks.Add(ProcessTaskAsync(task, providerSlot, ct));
        }

        if (processingTasks.Count > 0)
            await Task.WhenAll(processingTasks);
    }

    private async Task ProcessTaskAsync(
        AgentPollingTaskDocument task,
        ProviderSlot providerSlot,
        CancellationToken ct)
    {
        await _concurrencyLimiter.WaitAsync(ct);
        try
        {
            logger.LogInformation("[Worker] 🚀 Processing TaskId={Id}, DocId={DocId}, Provider={Provider}",
                task.Id, task.DocumentId, providerSlot.Provider.WebAiName);

            using var scope = scopeFactory.CreateScope();
            var pollingRepo = scope.ServiceProvider.GetRequiredService<IAgentPollingTaskDocumentRepository>();

            var workflow = new DocumentClassificationWorkflow(
                scopeFactory, orchestrator,
                scope.ServiceProvider.GetRequiredService<ILogger<DocumentClassificationWorkflow>>());

            await workflow.ExecuteAsync(task, providerSlot, ct);

            // Success
            task.Status = "success";
            task.UpdatedAt = DateTime.Now;
            await pollingRepo.UpdateAsync(task);
            logger.LogInformation("[Worker] ✅ TaskId={Id} completed successfully.", task.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Worker] ✗ TaskId={Id} failed: {Msg}", task.Id, ex.Message);

            using var failScope = scopeFactory.CreateScope();
            var failRepo = failScope.ServiceProvider.GetRequiredService<IAgentPollingTaskDocumentRepository>();

            // Mark current as failed
            task.Status = "failed";
            task.UpdatedAt = DateTime.Now;
            await failRepo.UpdateAsync(task);

            // Re-queue: buat data baru untuk di-retry nanti
            var retryTask = new AgentPollingTaskDocument
            {
                DocumentId = task.DocumentId,
                TaskCode = "Init-New-Doc",
                Status = "inqueue",
                FullPath = task.FullPath,
                InsertedBy = task.InsertedBy,
                InsertedAt = DateTime.Now
            };
            await failRepo.InsertAsync(retryTask);
            logger.LogInformation("[Worker] 🔄 Re-queued TaskId={Id} as new inqueue entry.", task.Id);
        }
        finally
        {
            providerManager.ReleaseProvider(providerSlot.Provider.WebAiName);
            _concurrencyLimiter.Release();
        }
    }
}
