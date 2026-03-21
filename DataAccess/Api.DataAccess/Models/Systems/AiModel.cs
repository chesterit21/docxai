using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Systems
{
    [Table("AiModel")]
    public class AiModel : BaseEntityDefault
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string ModelName { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string UrlApi { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string ApiKey { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Provider { get; set; }

        [Required]
        public int MaxToken { get; set; }
    }
}
