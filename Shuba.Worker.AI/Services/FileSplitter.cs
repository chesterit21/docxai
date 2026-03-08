namespace Shuba.Worker.AI.Services;

/// <summary>
/// Splits large files into chunks for multi-upload to Web AI providers.
/// Temp files stored in /tmp/shuba-worker/{taskId}/
/// </summary>
public static class FileSplitter
{
    private const string TEMP_BASE_DIR = "/tmp/shuba-worker";

    /// <summary>
    /// Split a file into chunks of specified size.
    /// Returns list of temp file paths.
    /// </summary>
    public static async Task<List<string>> SplitFileAsync(string sourcePath, long chunkSizeBytes, Guid taskId)
    {
        var taskDir = Path.Combine(TEMP_BASE_DIR, taskId.ToString());
        Directory.CreateDirectory(taskDir);

        var chunks = new List<string>();
        var fileInfo = new FileInfo(sourcePath);

        if (!fileInfo.Exists)
            throw new FileNotFoundException($"Source file not found: {sourcePath}");

        await using var sourceStream = File.OpenRead(sourcePath);
        var buffer = new byte[81920]; // 80KB buffer
        int chunkIndex = 1;
        long bytesRemaining = fileInfo.Length;

        while (bytesRemaining > 0)
        {
            var chunkPath = Path.Combine(taskDir, $"chunk_{chunkIndex:D3}{Path.GetExtension(sourcePath)}");
            long chunkBytesWritten = 0;

            await using (var chunkStream = File.Create(chunkPath))
            {
                while (chunkBytesWritten < chunkSizeBytes && bytesRemaining > 0)
                {
                    var toRead = (int)Math.Min(buffer.Length, Math.Min(chunkSizeBytes - chunkBytesWritten, bytesRemaining));
                    var bytesRead = await sourceStream.ReadAsync(buffer.AsMemory(0, toRead));
                    if (bytesRead == 0) break;

                    await chunkStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    chunkBytesWritten += bytesRead;
                    bytesRemaining -= bytesRead;
                }
            }

            chunks.Add(chunkPath);
            Console.WriteLine($"[FileSplitter] 📦 Chunk {chunkIndex}: {chunkPath} ({chunkBytesWritten / (1024 * 1024)}MB)");
            chunkIndex++;
        }

        Console.WriteLine($"[FileSplitter] ✅ Split '{Path.GetFileName(sourcePath)}' into {chunks.Count} chunks.");
        return chunks;
    }

    /// <summary>
    /// Cleanup temp files for a task.
    /// </summary>
    public static void CleanupTempFiles(Guid taskId)
    {
        var taskDir = Path.Combine(TEMP_BASE_DIR, taskId.ToString());
        try
        {
            if (Directory.Exists(taskDir))
            {
                Directory.Delete(taskDir, true);
                Console.WriteLine($"[FileSplitter] 🧹 Cleaned up temp dir: {taskDir}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileSplitter] ⚠️ Cleanup failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the chunk size in bytes based on provider name.
    /// Qwen: 19MB, DeepSeek/ZAI: 45MB
    /// </summary>
    public static long GetChunkSizeForProvider(string providerName)
    {
        if (providerName.Contains("Qwen", StringComparison.OrdinalIgnoreCase))
            return 19L * 1024 * 1024; // 19MB

        return 45L * 1024 * 1024; // 45MB for DeepSeek/ZAI
    }
}
