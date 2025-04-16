using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("CategoriesSharedPrivillege")]
    public class CategoriesSharedPrivillege : BaseEntityUpdate
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
    }
}
