using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
	[Table("DocumentSharedPrivillege")]
	public class DocumentSharedPrivillege : BaseEntityDefault
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int DocumentSharedID { get; set; }

		[ForeignKey(nameof(DocumentSharedID))]
		public virtual DocumentShared DocumentShared { get; set; }
		
		public bool IsView { get; set; }

		public bool IsEdit { get; set; }

		public bool IsDelete { get; set; }
		
	}
}
