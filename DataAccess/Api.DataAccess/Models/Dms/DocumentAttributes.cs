using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblDocumentAttributes")]
    public class DocumentAttributes : BaseEntitySoftDelete
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int DocumentID { get; set; }

		[ForeignKey(nameof(DocumentID))]
		public virtual Documents Documents { get; set; }

		[Column(TypeName = "text")]
		public string AttributeValues { get; set; }

        //[Required]
        //[Column(TypeName = "integer")]
        //public int AttributeID { get; set; }
        //[ForeignKey(nameof(DocumentIDs))]
        //public virtual Categories Categories { get; set; }

        //[ForeignKey(nameof(DocumentIDs))]
        //public virtual Attributes Attributes { get; set; }
    }
}
