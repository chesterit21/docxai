using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.SemanticKernel.Embeddings;
namespace Api.DataAccess.Models.RAG
{
	public class DocumentChunk
	{
		public string DocumentId { get; set; }
		public string Text { get; set; }
		public ReadOnlyMemory<float> Vector { get; set; }
	}
}
