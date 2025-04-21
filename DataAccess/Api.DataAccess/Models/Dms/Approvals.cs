using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
	[Table("Approvals")]
	public class Approvals : BaseEntityDefault
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int CategoryID { get; set; }

		[ForeignKey(nameof(CategoryID))]
		public virtual List<Categories> Categories { get; set; }

		[Required]
		public int DocumentID { get; set; }

		[ForeignKey(nameof(DocumentID))]
		public virtual Documents Documents { get; set; }

	}
}
