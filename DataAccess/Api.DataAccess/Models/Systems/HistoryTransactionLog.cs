using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Systems
{
    [Table("TblHistoryLogTransaction")]
    [PrimaryKey(nameof(Id))]
    public class HistoryTransactionLog : BaseEntity
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column(TypeName = "varchar(39)")]
        public string IPAddress { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string UserAgent { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Action { get; set; }

        [Column(TypeName = "text")]
        public string Path { get; set; }

        [Column(TypeName = "text")]
        public string Description { get; set; }
    }
}
