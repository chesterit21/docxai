using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblNotification")]
    public class Notifications : BaseEntitySoftDelete
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public Guid Id { get; set; } = Guid.NewGuid();
		public int? DocumentID { get; set; }

		[Required]
        [Column(TypeName = "smallint")]
        public short NotificationType { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string NotifDescription { get; set; }

		[Column(TypeName = "varchar(50)")]
		public string NotifAction { get; set; }

		public int TargetActor { get; set; }

		[Column(TypeName = "text")]
		public string NotifContent { get; set; }

		[System.Text.Json.Serialization.JsonIgnore]
		[ForeignKey(nameof(DocumentID))]
		public virtual Documents Document { get; set; }
	}
}
