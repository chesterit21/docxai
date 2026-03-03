using Api.DataAccess.Models.Masters;
using Api.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblApprovalActivities")]
    public class ApprovalActivities : BaseEntitySoftDelete
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "integer")]
        public int ApprovalID { get; set; }
        //[Required]
        [Column(TypeName = "integer")]
        public ApprovalActivity? ApprovalActivity { get; set; }
        //[Required]
        [Column(TypeName = "varchar(50)")]
        public string ApprovalActivityName { get; set; }
        //[Required]
        [Column(TypeName = "varchar(2000)")]
        public string Remark { get; set; }
        //[Required]
        [Column(TypeName = "varchar(2000)")]
        public string Reason { get; set; }
        [Column(TypeName = "integer")]
        public int CurrentStep { get; set; }
        [Column(TypeName = "integer")]
        public int? NextStep { get; set; }
        public int? RelatedDocumentID { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(ApprovalID))]
        public virtual Approvals Approvals { get; set; }
		[ForeignKey(nameof(RelatedDocumentID))]
		public virtual Documents RelatedDocument { get; set; }

		public new int InsertedBy { get; set; }
		[ForeignKey(nameof(InsertedBy))]
		public virtual User InsertedUser { get; set; }
	}
}
