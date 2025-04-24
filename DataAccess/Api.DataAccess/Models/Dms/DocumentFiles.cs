using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
	[Table("DocumentFiles")]
	public class DocumentFiles : BaseEntityDefault
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int DocumentID { get; set; }

		[ForeignKey(nameof(DocumentID))]
		public virtual Documents Documents { get; set; }

		[Column(TypeName = "varchar(100)")]
		public string DocumentType { get; set; }

		[Column(TypeName = "varchar(1000)")]
		public string DocumentFileName { get; set; }

		public int DocumentFileSize { get; set; }

		[Column(TypeName = "varchar(1000)")]
		public string DocumentFileContent { get; set; }

		[Column(TypeName = "varchar(1000)")]
		public string DocumentFilePath { get; set; }

		public bool IsMainDocumentFile { get; set; }

	}
}
