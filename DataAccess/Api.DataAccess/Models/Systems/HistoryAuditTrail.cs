using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Systems
{
    [Table("TblHistoryLogAuditTrail")]
    [PrimaryKey(nameof(Id))]
    public class HistoryAuditTrail : BaseEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TransactionLogId { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string TableName { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Command { get; set; }

        [Column(TypeName = "text")]
        public string Before { get; set; }

        [Column(TypeName = "text")]
        public string After { get; set; }
    }
}
