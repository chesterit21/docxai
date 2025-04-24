using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Systems
{
    [Table("TblLogApplication")]
    [PrimaryKey(nameof(Id))]
    public class ApplicationLog : BaseEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        //[Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column(TypeName = "varchar(39)")]
        public string IPAddress { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string UserAgent { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string Type { get; set; }

        [Column(TypeName = "text")]
        public string Message { get; set; }

        [Column(TypeName = "text")]
        public string StackTrace { get; set; }

        [Column(TypeName = "text")]
        public string Endpoint { get; set; }

        [Column(TypeName = "text")]
        public string Parameter { get; set; }

        [Column(TypeName = "text")]
        public string UserName { get; set; }
    }
}
