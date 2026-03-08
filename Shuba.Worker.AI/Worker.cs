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
    private const int POLLING_INTERVAL_MS = 3000; // 3 detik

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("╔══════════════════════════════════════════════╗");
        logger.LogInformation("║  Shuba.Worker.AI — Document Extractor       ║");
        logger.LogInformation("║  Max Concurrent: 3  |  Poll: {Poll}ms        ║", POLLING_INTERVAL_MS);
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
                await PollAndDispatchAsync(stoppingToken);
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

    /// <summary>
    /// Setiap 3 detik: cek berapa provider idle → ambil task sebanyak itu → dispatch tanpa menunggu.
    /// Task yang selesai duluan langsung release slot, poll berikutnya bisa isi lagi.
    /// </summary>
    private async Task PollAndDispatchAsync(CancellationToken ct)
    {
        // Cek berapa slot provider yang lagi idle
        var idleCount = providerManager.IdleCount;
        if (idleCount == 0)
        {
            logger.LogDebug("[Worker] ⏸️ All providers busy, skip polling.");
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var pollingRepo = scope.ServiceProvider.GetRequiredService<IAgentPollingTaskDocumentRepository>();

        // Ambil task sebanyak provider yang idle
        var pendingTasks = await pollingRepo.GetAsync(
            x => x.Status == "inqueue" && x.TaskCode == "Init-New-Doc");

        if (pendingTasks == null || !pendingTasks.Any())
            return;

        var tasksToProcess = pendingTasks.Take(idleCount).ToList();

        logger.LogInformation("[Worker] 📋 Found {Count} inqueue | Idle providers: {Idle} | Dispatching: {Take}",
            pendingTasks.Count, idleCount, tasksToProcess.Count);

        // Dispatch masing-masing task secara independen (fire-and-forget)
        foreach (var task in tasksToProcess)
        {
            // Baca file size untuk menentukan provider yang cocok
            long fileSize = 0;
            if (!string.IsNullOrEmpty(task.FullPath) && File.Exists(task.FullPath))
                fileSize = new FileInfo(task.FullPath).Length;

            var providerSlot = providerManager.AcquireProviderForFileSize(fileSize);
            if (providerSlot == null)
            {
                logger.LogWarning("[Worker] ⚠️ No suitable provider for TaskId={Id} (FileSize={Size}MB). Will retry next cycle.",
                    task.Id, fileSize / (1024 * 1024));
                break; // Tunggu cycle berikutnya
            }

            // Update ke "running" sebelum dispatch
            task.Status = "running";
            task.UpdatedAt = DateTime.Now;
            await pollingRepo.UpdateAsync(task);

            // Fire-and-forget: dispatch tanpa await, jalan di background
            _ = ProcessTaskAsync(task, providerSlot, ct);

            logger.LogInformation("[Worker] 🚀 Dispatched TaskId={Id} → {Provider} (FileSize={Size}MB)",
                task.Id, providerSlot.Provider.WebAiName, fileSize / (1024 * 1024));
        }
    }

    /// <summary>
    /// Proses 1 task secara independen. Setelah selesai, release provider slot otomatis.
    /// </summary>
    private async Task ProcessTaskAsync(
        AgentPollingTaskDocument task,
        ProviderSlot providerSlot,
        CancellationToken ct)
    {
        try
        {
            logger.LogInformation("[Worker] � Start processing TaskId={Id}, DocId={DocId}, Provider={Provider}",
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

            try
            {
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
            catch (Exception retryEx)
            {
                logger.LogError(retryEx, "[Worker] ✗ Failed to re-queue TaskId={Id}", task.Id);
            }
        }
        finally
        {
            // Release provider slot → langsung tersedia untuk task berikutnya
            providerManager.ReleaseProvider(providerSlot.Provider.WebAiName);
            logger.LogInformation("[Worker] 🔓 Provider {Provider} released, ready for next task.",
                providerSlot.Provider.WebAiName);
        }
    }
}
