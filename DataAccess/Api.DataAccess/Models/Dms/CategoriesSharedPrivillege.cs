using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblCategoriesSharedPrivillege")]
    public class XCategoriesSharedPrivillege : BaseEntityUpdate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int CategoriesSharedID { get; set; }

        [Column(TypeName = "boolean")]
        public bool IsView { get; set; }

        [Column(TypeName = "boolean")]
        public bool IsEdit { get; set; }

        [Column(TypeName = "boolean")]
        public bool IsDelete { get; set; }

        [ForeignKey(nameof(CategoriesSharedID))]
        public virtual CategoriesShared CategoriesShared { get; set; }
    }
}
