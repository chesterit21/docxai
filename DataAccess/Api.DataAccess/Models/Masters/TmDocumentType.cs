using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    [Table("TmDocumentType")]
    public class TmDocumentType : BaseEntityDefault
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string CategoryName { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string SubCategoryName { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string DocumentType { get; set; }

        public virtual ICollection<TmDocumentTypeAttributes> Attributes { get; set; }
    }
}
