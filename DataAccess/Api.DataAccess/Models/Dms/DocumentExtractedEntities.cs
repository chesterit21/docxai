using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("DocumentExtractedEntities", Schema = "public")]
    public class DocumentExtractedEntities : BaseEntityDefault
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column(TypeName = "integer")]
        public int DocumentId { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string AttributeName { get; set; }

        [Column(TypeName = "text")]
        public string ValueText { get; set; }

        [Column(TypeName = "integer")]
        public int? ValueNumber { get; set; }

        [Column(TypeName = "numeric")]
        public decimal? ValueDecimal { get; set; }

        [Column(TypeName = "boolean")]
        public bool? ValueBoolean { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? ValueDate { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Documents Document { get; set; }
    }
}
