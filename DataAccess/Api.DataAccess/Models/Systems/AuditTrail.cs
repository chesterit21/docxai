using Api.DataAccess.Models.Masters;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Systems
{
	//[Table(nameof(AuditTrail), Schema = "log")]
	[Table("TblLogAuditTrail")]
	[PrimaryKey(nameof(Id))]
	public class AuditTrail : BaseEntity
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid? TransactionLogId { get; set; }

		[Column(TypeName = "varchar(50)")]
		public string TableName { get; set; }

		[Column(TypeName = "varchar(50)")]
		public string Command { get; set; }

		[Column(TypeName = "text")]
		public string Before { get; set; }

		[Column(TypeName = "text")]
		public string After { get; set; }

		[ForeignKey(nameof(TransactionLogId))]
		public TransactionLog TransactionLog { get; set; }

		public new int InsertedBy { get; set; } = 0;

		[ForeignKey(nameof(InsertedBy))]
		public User InsertedByUser { get; set; }
	}
}
