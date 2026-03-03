using Api.DataAccess.Models.Masters;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblDocumentRelated")]
    public class DocumentRelated : BaseEntitySoftDelete
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int DocumentID { get; set; }

        [Required]
        [Column(TypeName = "integer")]
		public int RelatedDocumentID { get; set; }

		[ForeignKey(nameof(RelatedDocumentID))]
		public virtual Documents Documents { get; set; }

		//[ForeignKey(nameof(RelatedDocumentID))]
		//public virtual Documents RelatedDocument { get; set; }
	}
}
