using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Api.Domain.Enum;

namespace Api.DataAccess.Models.Dms
{
	[Table("TblApproval")]
	public class Approvals : BaseEntitySoftDelete
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }// = Guid.NewGuid()
		
		[Column(TypeName = "integer")]
		public int? CategoryID { get; set; }
		[Column(TypeName = "integer")]
		public int? DocumentID { get; set; }
		[Column(TypeName = "integer")]
		public Domain.Enum.ApprovalStatusEnum Status { get; set; }
		[Required]
		[Column(TypeName = "varchar(2000)")]
		public string Notes { get; set; }
		[Required]
		[Column(TypeName = "integer")]
		public int MaxStep { get; set; }

		[Column(TypeName = "integer")]
		public int? CurrentStep { get; set; }
		public int? CurrentApproverUserID { get; set; }
		public int? NextApproverUserID { get; set; }
		public DateTime? LastActivityDate { get; set; }
		[Column(TypeName = "varchar(2000)")]
		public string? LastRemark { get; set; }

		[ForeignKey(nameof(CategoryID))]
		public Categories Categories { get; set; }

		[ForeignKey(nameof(DocumentID))]
		public Documents Documents { get; set; }
		public virtual ICollection<ApprovalFlows> ApprovalFlows { get; set; }
		public virtual ICollection<ApprovalActivities> ApprovalActivities { get; set; }
		[ForeignKey(nameof(CurrentApproverUserID))]
		public virtual User CurrentApproverUser { get; set; }
		[ForeignKey(nameof(NextApproverUserID))]
		public virtual User NextApproverUser { get; set; }

		public new int InsertedBy { get; set; }
		[ForeignKey(nameof(InsertedBy))]
		public virtual User InsertedUser { get; set; }
	}
}
