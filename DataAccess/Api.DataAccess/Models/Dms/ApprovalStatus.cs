using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Api.DataAccess.Models.Dms
{
	[Table("ApprovalStatus")]
	public class ApprovalStatus : BaseEntityDefault
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int ApprovalID { get; set; }

		[ForeignKey(nameof(ApprovalID))]
		public virtual Approvals Approvals { get; set; }

		[Column(TypeName = "varchar(255)")]
		public string ApprovalStatusDesc { get; set; }

		public DateTime ApprovalDate { get; set; }

		[Column(TypeName = "varchar(255)")]
		public string Remark { get; set; }

		[Column(TypeName = "varchar(255)")]
		public string Reason { get; set; }

		public string CurrentStep { get; set; }

		public string NextStep { get; set; }
		
	}
}
