using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("AgentPollingTaskDocument", Schema = "public")]
    public class AgentPollingTaskDocument : BaseEntityAgent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column(TypeName = "integer")]
        public int DocumentId { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string TaskCode { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Status { get; set; }

        [Column(TypeName = "text")]
        public string FullPath { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual Documents Document { get; set; }
    }
}
