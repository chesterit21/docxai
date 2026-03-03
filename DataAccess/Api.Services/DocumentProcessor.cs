using Api.DataAccess.Models;
using Api.DataAccess.Models.RAG;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SemanticChunkerNET;//https://www.nuget.org/packages/SemanticChunker.NET/1.0.1#show-readme-container

namespace Api.Services
{
	public class DocumentProcessor
	{
		private readonly Kernel _kernel;

		public DocumentProcessor(Kernel kernel)
		{
			_kernel = kernel;
		}

		// Fungsi utama yang dipanggil saat unggah/edit dokumen
		public async Task<List<DocumentChunk>> ProcessAndEmbedDocumentAsync(string documentId, string filePath)
		{
			// 1. Ekstraksi Teks
			var fullText = await FileProcessor.ExtractTextAsync(filePath);

			// 2. Pemecahan (Chunking)
			#pragma warning disable SKEXP0050
			var textSplitter = TextChunker.SplitPlainTextLines(fullText, 256);// 256 adalah ukuran token per chunk

			// 3. Vektorisasi (Vectorization) & Pengumpulan Hasil
			var processedChunks = new List<DocumentChunk>();
			//var embeddingService = _kernel.GetRequiredService<IEmbeddingGenerationService>();
			//var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService<float>>();
			var embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
			

			foreach (var chunk in textSplitter)
			{
				var embedding = await embeddingService.GenerateEmbeddingAsync(chunk);
				processedChunks.Add(new DocumentChunk
				{
					DocumentId = documentId,
					Text = chunk,
					Vector = embedding
				});
			}

			Console.WriteLine($"Selesai memproses {processedChunks.Count} chunks.");
			return processedChunks;
		}
	}
}

// Register the service in your dependency injection container
//services.AddSingleton<ITextEmbeddingGenerationService<float>, YourEmbeddingServiceImplementation>();
