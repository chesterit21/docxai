namespace Shuba.Worker.AI.Workflows.Prompts;

/// <summary>
/// Template Prompt 1: Klasifikasi dokumen ke dalam taksonomi 14 kategori.
/// Output JSON: { Category, SubCategory, DocumentType, Summary, Points }
/// </summary>
public static class ClassificationPrompt
{
    public static string Build(string documentContent, IEnumerable<Api.DataAccess.Models.Masters.TmDocumentType> documentTypes)
    {
        var categoryGroups = documentTypes
            .GroupBy(d => d.CategoryName)
            .OrderBy(g => g.Key)
            .ToList();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Anda adalah asisten AI khusus untuk Document Management System (DMS) yang bertugas mengklasifikasikan dokumen ke dalam taksonomi yang telah ditentukan.");
        sb.AppendLine();
        sb.AppendLine($"## 📋 TAKSONOMI DOKUMEN ({categoryGroups.Count} KATEGORI)");
        sb.AppendLine();

        int catIndex = 1;
        foreach (var catGroup in categoryGroups)
        {
            sb.AppendLine($"### {catIndex}. {catGroup.Key}");

            var subCategoryGroups = catGroup
                .GroupBy(c => c.SubCategoryName)
                .OrderBy(s => s.Key)
                .ToList();

            foreach (var subGroup in subCategoryGroups)
            {
                if (!string.IsNullOrWhiteSpace(subGroup.Key))
                {
                    sb.AppendLine($"#### {subGroup.Key}");
                }

                var docTypes = subGroup.Select(s => s.DocumentType).Where(d => !string.IsNullOrWhiteSpace(d)).OrderBy(d => d).ToList();
                if (docTypes.Any())
                {
                    sb.AppendLine($"- {string.Join(", ", docTypes)}");
                }
            }
            sb.AppendLine();
            catIndex++;
        }

        sb.AppendLine(@"## 🎯 YANG PERLU KAMU LAKUKAN

Tolong analisis dokumen yang di-upload, lalu cocokkan dengan kategori taksonomi di atas.

Di dalam jawaban kamu, sertakan JSON dengan format seperti ini ya:

```json
{
  ""Category"": ""Nama Kategori Utama"",
  ""SubCategory"": ""Nama Subkategori"",
  ""DocumentType"": ""Nama Jenis Dokumen"",
  ""Summary"": ""Ringkasan isi dokumen, minimal 1 paragraf (kalau dokumen besar boleh lebih). Pakai format markdown."",
  ""Points"": ""Poin-poin penting dari dokumen, dalam format markdown""
}
```

Pastikan JSON-nya valid ya. Kamu boleh kasih penjelasan tambahan di luar JSON kalau memang perlu — yang penting JSON-nya tetap ada di jawaban.

## DOKUMEN UNTUK DIANALISIS:

");
        sb.AppendLine(documentContent);

        return sb.ToString();
    }

    public static string UserMessage => "Tolong analisis dan klasifikasikan dokumen yang sudah di-upload ya. Sertakan hasil dalam format JSON sesuai struktur yang diminta.";
}
