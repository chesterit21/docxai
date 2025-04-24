using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
    [Table("Attributtes")]
    public class Attributtes : BaseEntityDefault
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string AttributteName { get; set; }

        [Column(TypeName = "json")]
        public string AttributeElement { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string AttributteType { get; set; }

        [Column(TypeName = "integer")]
        public int AttributeMaxLength { get; set; }

    }
}
