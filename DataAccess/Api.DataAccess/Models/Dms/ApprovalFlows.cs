using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Api.DataAccess.Models.Dms
{
	[Table("ApprovalFlows")]
	public class ApprovalFlows : BaseEntityDefault
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int ApprovalID { get; set; }

		[ForeignKey(nameof(ApprovalID))]
		public virtual Approvals Approvals { get; set; }

		public string Step { get; set; }

		public string MaxStep { get; set; }
		
		[Required]
		public int UserID { get; set; }

		[ForeignKey(nameof(UserID))]
		public virtual User User { get; set; }
		
		[Required]
		public int GroupID { get; set; }

		[ForeignKey(nameof(GroupID))]
		public virtual Role Role { get; set; }


		
	}
}
