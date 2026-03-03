using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.DataAccess.Models.Systems
{
    [Table("TblLogActivityLogin")]
    [PrimaryKey(nameof(Id))]
    public class LoginActivityLog : BaseEntity
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column(TypeName = "varchar(39)")]
        public string IPAddress { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string UserAgent { get; set; }
        public DateTime LoginTime { get; set; }
        public int ActorUserId { get; set; }
        public string UserAction { get; set; }
	}
}
