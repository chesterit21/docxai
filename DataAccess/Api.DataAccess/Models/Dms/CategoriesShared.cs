using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("CategoriesShared")]
    public class CategoriesShared : BaseEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public string CategoryID { get; set; }

        [Column(TypeName = "integer")]
        public string UserID { get; set; }

    }
}
