using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("Categories")]
    public class Categories : BaseEntityDefault
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string CategoryName { get; set; }

        [Column(TypeName = "text")]
        public string CategoryDesc { get; set; }
        
        public int? ParentId { get; set; }

        //[NotMapped]
        [ForeignKey(nameof(ParentId))]
        public virtual Categories ParentCategory { get; set; }
        //[NotMapped]
        public virtual List<Categories> ChildCategories { get; set; }
        public bool IsNeedApproval { get; set; }

    }
}
