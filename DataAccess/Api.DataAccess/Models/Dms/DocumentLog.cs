using Api.DataAccess.Models.Systems;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
	[Table("TblDocumentLogs")]
	public class DocumentLog : BaseEntitySoftDelete
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public Guid Id { get; set; } = Guid.NewGuid();		
		public Guid? LogAuditTrailID { get; set; }
		[Required]
		public int DocumentID { get; set; }
		[Required]
		[Column(TypeName = "varchar(150)")]
		public string ActionLogDocument { get; set; }
		[ForeignKey(nameof(LogAuditTrailID))]
		public virtual AuditTrail AuditTrls { get; set; }
		[ForeignKey(nameof(DocumentID))]
		public virtual Documents Documents { get; set; }
	}
}
