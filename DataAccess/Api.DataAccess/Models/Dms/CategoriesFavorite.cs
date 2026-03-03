using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblCategoriesFavorite")]
    public class CategoriesFavorite : BaseEntitySoftDelete
    {
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
        [Column(TypeName = "integer")]
        public int CategoryID { get; set; }

		[Required]
		[Column(TypeName = "integer")]
		public int Owner { get; set; }

		[ForeignKey(nameof(CategoryID))]
		public virtual Categories Categories { get; set; }
	}
}
