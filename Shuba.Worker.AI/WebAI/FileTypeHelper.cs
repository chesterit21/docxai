namespace Shuba.Worker.AI.WebAI;

/// <summary>
/// Helper untuk handle file extension yang mungkin di-block oleh Web AI providers.
/// Beberapa provider tidak menerima file .cs, .js, .py dll — rename ke .txt.
/// </summary>
public static class FileTypeHelper
{
    private static readonly HashSet<string> _blockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".js", ".ts", ".py", ".rb", ".go", ".rs", ".java", ".kt",
        ".swift", ".cpp", ".c", ".h", ".hpp", ".sh", ".bash", ".ps1",
        ".bat", ".cmd", ".php", ".lua", ".r", ".m", ".scala", ".groovy",
        ".pl", ".ex", ".exs", ".zig", ".nim", ".d", ".v", ".vb",
        ".jsx", ".tsx", ".vue", ".svelte", ".astro"
    };

    /// <summary>
    /// Cek apakah file extension perlu di-rename (programming files).
    /// </summary>
    public static bool NeedsRename(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        return _blockedExtensions.Contains(ext);
    }

    /// <summary>
    /// Buat renamed copy: file.cs → file.cs.txt (di temp dir).
    /// </summary>
    public static string CreateRenamedCopy(string originalPath)
    {
        var dir = Path.GetDirectoryName(originalPath) ?? Path.GetTempPath();
        var newName = Path.GetFileName(originalPath) + ".txt";
        var newPath = Path.Combine(dir, newName);

        File.Copy(originalPath, newPath, overwrite: true);
        Console.WriteLine($"[FileTypeHelper] 📝 Renamed copy: {newName}");
        return newPath;
    }

    /// <summary>
    /// Cleanup renamed file.
    /// </summary>
    public static void CleanupRenamedFile(string renamedPath)
    {
        try
        {
            if (File.Exists(renamedPath))
            {
                File.Delete(renamedPath);
                Console.WriteLine($"[FileTypeHelper] 🧹 Cleaned: {Path.GetFileName(renamedPath)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileTypeHelper] ⚠️ Cleanup failed: {ex.Message}");
        }
    }
}
