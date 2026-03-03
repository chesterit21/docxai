using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblDocumentShared")]
    public class DocumentShared : BaseEntitySoftDelete
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[Required]
		[Column(TypeName = "integer")]
		public int DocumentID { get; set; }
		//[Required]
		//public int UserId { get; set; }
		[ForeignKey(nameof(DocumentID))]
		public virtual Documents Document { get; set; }
		public virtual List<DocumentSharedPrivillege> DocumentSharePrivs { get; set; }
	}
}
