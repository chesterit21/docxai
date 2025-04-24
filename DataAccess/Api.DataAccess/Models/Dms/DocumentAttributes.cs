using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
	[Table("DocumentAttributes")]
	public class DocumentAttributes : BaseEntityDefault
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int DocumentID { get; set; }

		[ForeignKey(nameof(DocumentID))]
		public virtual Documents Documents { get; set; }

		[Required]
		public int AttributeID { get; set; }

		[ForeignKey(nameof(AttributeID))]
		public virtual Attributtes Attributtes { get; set; }

		[Required]
		[Column(TypeName = "json")]
		public string AttributeValues { get; set; }
		
	}
}
