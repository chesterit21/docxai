namespace Shuba.Worker.AI.Workflows.Prompts;

/// <summary>
/// Prompt templates untuk multi-upload chunked documents.
/// Tone natural — AI Web punya kapasitas gede & history, jadi gak perlu kaku.
/// </summary>
public static class ChunkedUploadPrompt
{
    /// <summary>
    /// Prompt untuk chunk pertama (intro).
    /// </summary>
    public static string BuildIntroPrompt(int partNumber, int totalParts) =>
        $"Hai, gua mau kirim dokumen yang agak besar nih. " +
        $"Karena ukurannya gede, gua bagi jadi {totalParts} bagian ya. " +
        $"Ini baru bagian {partNumber} dari {totalParts}. " +
        $"Belum perlu dianalisis dulu — tunggu sampai semua bagian selesai gua kirim ya.";

    /// <summary>
    /// Prompt untuk chunk tengah (middle).
    /// </summary>
    public static string BuildMiddlePrompt(int partNumber, int totalParts)
    {
        var remaining = totalParts - partNumber;
        return $"Lanjut ya, ini bagian ke-{partNumber} dari {totalParts}. " +
               $"Masih ada {remaining} bagian lagi setelah ini. " +
               $"Terima dulu aja, nanti baru gua minta analisis setelah semua bagian lengkap.";
    }

    /// <summary>
    /// Prompt untuk chunk terakhir — di sini baru minta analisis.
    /// </summary>
    public static string BuildFinalPrompt(int partNumber, int totalParts) =>
        $"Oke, ini bagian terakhir — bagian {partNumber} dari {totalParts}. " +
        $"Sekarang semua bagian dokumennya sudah lengkap. " +
        $"Tolong analisis keseluruhan isi dokumen dari bagian 1 sampai {totalParts} " +
        $"dan klasifikasikan sesuai instruksi yang sudah gua kasih di awal ya.\n\n";

    /// <summary>
    /// User message untuk chunk non-final.
    /// </summary>
    public static string ChunkUserMessage(int partNumber, int totalParts) =>
        partNumber < totalParts
            ? $"Ini bagian {partNumber}, terima dulu ya. Nanti gua lanjut kirim sisanya."
            : "Semua bagian sudah lengkap. Tolong analisis dan sertakan JSON klasifikasinya ya.";
}
