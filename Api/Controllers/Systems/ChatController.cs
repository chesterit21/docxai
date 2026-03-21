using System.Text;
using System.Text.Json;
using Api.Domain.EntityRequests.Systems;
using Api.Repository.Systems;
using Api.Repository.Masters;
using Api.Services.Core.Inference;
using Api.Services.Core.Models.Streaming;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Docubase.api.Controllers.Systems
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly InferenceEngine _inferenceEngine;
        private readonly IAiModelRepository _aiModelRepo;
        private readonly IDocumentFilesRepository _documentFilesRepo;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            InferenceEngine inferenceEngine,
            IAiModelRepository aiModelRepo,
            IDocumentFilesRepository documentFilesRepo,
            ILogger<ChatController> logger)
        {
            _inferenceEngine = inferenceEngine;
            _aiModelRepo = aiModelRepo;
            _documentFilesRepo = documentFilesRepo;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("stream")]
        public async Task Stream([FromBody] ChatRequest request)
        {
            if (request.document_ids == null || request.document_ids.Count == 0)
            {
                Response.StatusCode = 400;
                Response.ContentType = "application/json";
                await Response.WriteAsync(JsonSerializer.Serialize(new { message = "Silakan pilih dokumen terlebih dahulu sebelum memulai chat." }));
                return;
            }

            // 1. Get Document Content & Summary for Context
            StringBuilder contextBuilder = new StringBuilder();
            var docFiles = await _documentFilesRepo.GetAsync(d => request.document_ids.Contains(d.Id));

            foreach (var doc in docFiles)
            {
                contextBuilder.AppendLine($"### NAMA DOKUMEN: {doc.DocumentFileName}");
                if (!string.IsNullOrEmpty(doc.DocumentSummary))
                {
                    contextBuilder.AppendLine("#### RINGKASAN DOKUMEN:");
                    contextBuilder.AppendLine(doc.DocumentSummary);
                }
                if (!string.IsNullOrEmpty(doc.DocumentFileContent))
                {
                    contextBuilder.AppendLine("#### ISI DOKUMEN:");
                    contextBuilder.AppendLine(doc.DocumentFileContent);
                }
                contextBuilder.AppendLine(new string('-', 30));
                contextBuilder.AppendLine();
            }

            string documentContext = contextBuilder.ToString();
            string systemPrompt = GetSystemPrompt(documentContext);

            Response.ContentType = "text/event-stream";
            var responseStream = Response.Body;

            // 2. Determine available models and retry limit
            var allModels = await _aiModelRepo.GetAsync();
            if (allModels == null || allModels.Count == 0)
            {
                Response.StatusCode = 500;
                await Response.WriteAsync(JsonSerializer.Serialize(new { message = "Tidak ada konfigurasi AI Model yang tersedia." }));
                return;
            }

            HashSet<Guid> triedModelIds = new();
            bool hasStartedStreaming = false;

            // 3. Retry Loop: Iterate through providers until success or exhaustion
            for (int attempt = 0; attempt < allModels.Count; attempt++)
            {
                var modelEntity = allModels
                    .Where(m => !triedModelIds.Contains(m.Id))
                    .OrderBy(m => m.UpdatedAt)
                    .FirstOrDefault();

                if (modelEntity == null) break;

                triedModelIds.Add(modelEntity.Id);
                modelEntity.UpdatedAt = DateTime.UtcNow;
                await _aiModelRepo.UpdateAsync(modelEntity);

                _logger.LogInformation("Attempting inference with provider {Provider}, model {Model} (Attempt {Attempt})",
                    modelEntity.Provider, modelEntity.ModelName, attempt + 1);

                var inferenceRequest = new InferenceRequest
                {
                    Model = modelEntity.ModelName,
                    Messages = new List<Message>
                    {
                        new Message { Role = "system", Content = systemPrompt },
                        new Message { Role = "user", Content = request.message }
                    },
                    Stream = true
                };

                try
                {
                    await foreach (var chunk in _inferenceEngine.StreamAsync(inferenceRequest, providerName: modelEntity.Provider))
                    {
                        if (chunk.Type == StreamChunkType.ContentDelta && !string.IsNullOrEmpty(chunk.Content))
                        {
                            hasStartedStreaming = true;
                            var data = JsonSerializer.Serialize(new { delta = chunk.Content });
                            byte[] bytes = Encoding.UTF8.GetBytes($"event: message\ndata: {data}\n\n");
                            await responseStream.WriteAsync(bytes);
                            await responseStream.FlushAsync();
                        }
                    }

                    if (hasStartedStreaming)
                    {
                        byte[] doneBytes = Encoding.UTF8.GetBytes("event: done\ndata: {}\n\n");
                        await responseStream.WriteAsync(doneBytes);
                        await responseStream.FlushAsync();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Provider {Provider} failed on attempt {Attempt}. Retrying if possible...",
                        modelEntity.Provider, attempt + 1);

                    if (hasStartedStreaming)
                    {
                        var errorData = JsonSerializer.Serialize(new { delta = "\n[Layanan terputus di tengah jalan. Silakan coba lagi.]" });
                        byte[] errorBytes = Encoding.UTF8.GetBytes($"event: message\ndata: {errorData}\n\n");
                        await responseStream.WriteAsync(errorBytes);
                        await responseStream.FlushAsync();
                        break;
                    }
                }
            }

            if (!hasStartedStreaming)
            {
                _logger.LogError("All AI providers were exhausted or failed for request.");
                var finalErrorData = JsonSerializer.Serialize(new { delta = "Maaf, semua layanan AI sedang sibuk atau tidak tersedia. Silakan coba lagi nanti." });
                byte[] finalErrorBytes = Encoding.UTF8.GetBytes($"event: message\ndata: {finalErrorData}\n\n");
                await responseStream.WriteAsync(finalErrorBytes);
                await responseStream.FlushAsync();

                byte[] doneBytes = Encoding.UTF8.GetBytes("event: done\ndata: {}\n\n");
                await responseStream.WriteAsync(doneBytes);
                await responseStream.FlushAsync();
            }
        }

        private string GetSystemPrompt(string documentContent)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("Anda adalah asisten AI yang cerdas, profesional, dan ahli hukum.");
            prompt.AppendLine("Tugas Anda adalah membantu pengguna menganalisis dokumen yang diberikan secara akurat.");
            prompt.AppendLine();
            prompt.AppendLine("### DATA KONTEKS DOKUMEN:");
            prompt.AppendLine(" Berikut adalah konten dan ringkasan dari dokumen yang dipilih oleh pengguna:");
            prompt.AppendLine("==================================================");
            prompt.AppendLine(string.IsNullOrEmpty(documentContent) ? "[Peringatan: Tidak ada konten dokumen yang terbaca]" : documentContent);
            prompt.AppendLine("==================================================");
            prompt.AppendLine();
            prompt.AppendLine("### PANDUAN JAWABAN:");
            prompt.AppendLine("1. Jawablah HANYA berdasarkan konteks dokumen di atas.");
            prompt.AppendLine("2. Jika jawaban tidak ditemukan dalam dokumen, sampaikan: 'Maaf, saya tidak menemukan informasi tersebut dalam dokumen yang Anda pilih.'");
            prompt.AppendLine("3. Gunakan bahasa Indonesia yang formal, sopan, dan mudah dipahami.");
            prompt.AppendLine("4. Jika dokumen dalam bahasa asing, berikan penjelasan intisarinya dalam bahasa Indonesia.");
            prompt.AppendLine("5. Jangan memberikan informasi di luar konteks dokumen kecuali sangat relevan untuk menjelaskan istilah hukum yang ada di dalam dokumen.");

            return prompt.ToString();
        }
    }
}


