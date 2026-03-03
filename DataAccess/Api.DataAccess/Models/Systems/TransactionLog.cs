using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.DataAccess.Models.Systems
{
    [Table("TblLogTransaction")]
    [PrimaryKey(nameof(Id))]
    public class TransactionLog : BaseEntity
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
        public string Description { get; set; }

        [Column(TypeName = "text")]
        public string Path { get; set; }

        [Column(TypeName = "text")]
        public string Parameter { get; set; }

		[JsonIgnore]
		public AuditTrail Audit { get; set; }
	}
}
