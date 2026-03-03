using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblApprovalFlows")]
    public class ApprovalFlows : BaseEntitySoftDelete
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
		public int ApprovalID { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(ApprovalID))]
		public virtual Approvals Approvals { get; set; }

		public int Step { get; set; }
		public bool IsFinalStep { get; set; }

		[Required]
		public int ApproverUserID { get; set; }

		[ForeignKey(nameof(ApproverUserID))]
		public virtual User User { get; set; }
		
		//[Required]
		//public int RoleID { get; set; }

		//[ForeignKey(nameof(RoleID))]
		//public virtual Group Role { get; set; }

        //[ForeignKey(nameof(CategoryID))]
        //public virtual Categories Categories { get; set; }
        //[ForeignKey(nameof(DocumentIDs))]
        //public virtual Documents Documents { get; set; }
    }
}
