using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
    [Table("Documents")]
    public class Documents : BaseEntityDefault
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int CategoryID { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string DocumentTitle { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string DocumentDesc { get; set; }

        [Column(TypeName = "integer")]
        public int Owner { get; set; }

        [Column(TypeName = "integer")]
        public int FileSize { get; set; }

        public DateTime ExpiryDate { get; set; }

        [Column(TypeName = "integer")]
        public int RemindderDays { get; set; }

        public DateTime RemandireDateTime { get; set; }

        [Column(TypeName = "integer")]
        public int RelatedDocumentId { get; set; }
    }
}
