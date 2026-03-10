using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    [Table("TmAttributeSynonyms")]
    public class TmAttributeSynonyms : BaseEntityDefault
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string AttributeName { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string Synonym { get; set; }

        [Required]
        [Column(TypeName = "varchar(5)")]
        public string Language { get; set; }
    }
}
