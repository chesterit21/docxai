using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Systems
{
    [Table("TblTrEmail")]
    [PrimaryKey(nameof(Id))]
    public class Email : BaseEntityDefault
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Subject { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string To { get; set; }

        [Column(TypeName = "text")]
        public string Cc { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Body { get; set; }

        public bool IsHtml { get; set; } = false;

        public int SentStatus { get; set; } = 0;

        [Column(TypeName = "varchar(1000)")]
        public string StatusMessage { get; set; }
    }
}
