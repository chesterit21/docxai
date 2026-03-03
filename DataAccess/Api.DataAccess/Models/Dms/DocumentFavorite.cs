using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblDocumentFavorite")]
    public class DocumentFavorite : BaseEntitySoftDelete
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
        [Column(TypeName = "integer")]
        public int DocumentId { get; set; }

		[Required]
		[Column(TypeName = "integer")]
		public int Owner { get; set; }

	}
}
