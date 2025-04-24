using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
	[Table("DocumentShared")]
	public class DocumentShared : BaseEntityDefault
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int DocumentID { get; set; }

		[ForeignKey(nameof(DocumentID))]
		public virtual Documents Documents { get; set; }

		[Required]
		public int DocumentFilesID { get; set; }

		[ForeignKey(nameof(DocumentFilesID))]
		public virtual DocumentFiles DocumentFiles { get; set; }

		[Required]
		public int UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public virtual User UserDocumentFiles { get; set; }

	}
}
