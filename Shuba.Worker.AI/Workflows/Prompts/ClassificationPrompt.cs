namespace Shuba.Worker.AI.Workflows.Prompts;

/// <summary>
/// Template Prompt 1: Klasifikasi dokumen ke dalam taksonomi 14 kategori.
/// Output JSON: { Category, SubCategory, DocumentType, Summary, Points }
/// </summary>
public static class ClassificationPrompt
{
    public static string Build(string session_id, IEnumerable<Api.DataAccess.Models.Masters.TmDocumentType> documentTypes)
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

        sb.AppendLine($@"## 🎯 YANG PERLU KAMU LAKUKAN

Tolong analisis dokumen yang di-upload, lalu cocokkan dengan kategori taksonomi di atas.
Jika dokumen yang di-upload tidak adalam dalan kategori taksonomi, maka masukan ke dalam kategori yang mendekati nya saja.

Di dalam jawaban kamu, sertakan JSON dengan format seperti ini ya:

```json
{{
   ""session_id"": ""{session_id}"",
   ""data"": 
    {{
      ""Category"": ""Nama Kategori Utama"",
      ""SubCategory"": ""Nama Subkategori"",
      ""DocumentType"": ""Nama Jenis Dokumen"",
      ""Summary"": ""Ringkasan isi dokumen, minimal 1 - 2 paragraf (kalau dokumen besar boleh lebih). Pakai format markdown."",
      ""Points"": ""Poin-poin penting dari dokumen, dalam format markdown..jangan dalam bentuk array, tetapi bungkus dalam tag <ul> <li></li> </ul> untuk memisahkan poin-poin.""
    }}
}}
```

Pastikan JSON-nya valid ya. Kamu boleh kasih penjelasan tambahan di luar JSON kalau memang perlu — yang penting JSON-nya tetap ada di jawaban.
Dan pastikan session_id yang kamu kirim adalah session_id yang sama dengan session_id yang kamu terima (""{session_id}"").

");

        return sb.ToString();
    }

    public static string SystemMessagePhase2(string session_id, IEnumerable<Api.DataAccess.Models.Masters.TmDocumentTypeAttributes> attributes)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($@"Tolong bantu saya untuk mengekstrak data spesifik dari dokumen yang sudah di-upload sebelumnya.

Gunakan daftar atribut di bawah ini sebagai panduan ekstraksi:
");

        foreach (var attr in attributes)
        {
            sb.AppendLine($"- **{attr.AttributeName}** (Tipe Data: {attr.DataType})\n");
        }

        sb.AppendLine($@"
Sertakan hasil ekstraksi dalam format JSON sesuai struktur di bawah ini. 
Jika ada atribut dalam dokumen tetapi tidak ada dalam daftar atribut di atas, maka kamu boleh menuliskan nya ke dalam jawaban di dalam output JSON.
Daftar atribut di atas sebagai acuan dasar, bisa jadi ada atribut yang tidak ada di daftar atribut di atas, maka tidak perlu di masukan ke dalam jawaban.
Pastikan yang relevan saja.

PENTING: Masukkan nilai ke salah satu property yang sesuai dengan tipe datanya (`ValueText`, `ValueNumber`, `ValueDecimal`, `ValueBoolean`, atau `ValueDate`). Property lainnya biarkan null atau jangan disertakan.

```json
{{
  ""session_id"": ""{session_id}"",
  ""data"": [
");

        int count = 0;
        var attrList = attributes.ToList();
        foreach (var attr in attrList)
        {
            count++;
            sb.AppendLine($@"    {{
      ""AttributeName"": ""{attr.AttributeName}"",
      ""ValueText"": ""..."",
      ""ValueNumber"": null,
      ""ValueDecimal"": null,
      ""ValueBoolean"": null,
      ""ValueDate"": null
    }}{(count < attrList.Count ? "," : "")}");
        }

        sb.AppendLine($@"  ]
}}
```

Pastikan JSON valid dan `session_id` tetap sama (""{session_id}"").");

        return sb.ToString();
    }

    public static string SystemMessagePhase2EmptyAttribute(string session_id)
    {
        return $@"Tolong bantu saya mengekstrak data spesifik dari dokumen yang sudah di-upload.

Kembalikan hasil dalam format JSON berikut. Isi HANYA property yang sesuai dengan tipe datanya, sisanya jangan disertakan atau biarkan null.

Aturan pengisian property:
- `ValueText`    → untuk teks/string (nama, alamat, kode, status, dll)
- `ValueNumber`  → untuk bilangan bulat (jumlah, tahun, nomor urut, dll)
- `ValueDecimal` → untuk bilangan desimal (harga, persentase, berat, dll)
- `ValueBoolean` → untuk nilai true/false (ya/tidak, aktif/tidak aktif, dll)
- `ValueDate`    → untuk tanggal, format ISO 8601 (yyyy-MM-dd)

Contoh output yang benar:
```json
{{
  ""session_id"": ""{session_id}"",
  ""data"": [
    {{
      ""AttributeName"": ""Nama Karyawan"",
      ""ValueText"": ""Budi Hartono"",
      ""ValueNumber"": null,
      ""ValueDecimal"": null,
      ""ValueBoolean"": null,
      ""ValueDate"": null
    }},
    {{
      ""AttributeName"": ""Tanggal Bergabung"",
      ""ValueText"": null,
      ""ValueNumber"": null,
      ""ValueDecimal"": null,
      ""ValueBoolean"": null,
      ""ValueDate"": ""2021-03-15""
    }},
    {{
      ""AttributeName"": ""Gaji Pokok"",
      ""ValueText"": null,
      ""ValueNumber"": null,
      ""ValueDecimal"": 8500000.00,
      ""ValueBoolean"": null,
      ""ValueDate"": null
    }}
  ]
}}
```

Sekarang ekstrak data dari dokumen dan kembalikan JSON dengan `session_id` yang sama (""{session_id}""). Pastikan JSON valid.";
    }

    public static string UserMessage => "Tolong analisis dan klasifikasikan dokumen yang sudah di-upload ya. Sertakan hasil dalam format JSON sesuai struktur yang diminta.";

    public static string UserMessagePhase2 =>
        $@"Tolong analisa deep-dive dan Pastikan akurat ya bro.";
}
