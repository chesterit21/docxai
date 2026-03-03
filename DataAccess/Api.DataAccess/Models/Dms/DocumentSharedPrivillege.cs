using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblDocumentSharedPrivillege")]
    public class DocumentSharedPrivillege : BaseEntitySoftDelete
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int DocumentSharedID { get; set; }
        [Required]
        [Column(TypeName = "varchar(5)")]
        public string ShareType { get; set; } = "user";// fill with "user" or "group"
		//[Required]
        [Column(TypeName = "integer")]
        public int? UserID { get; set; }
		//[Required]
		[Column(TypeName = "integer")]
		public int? GroupID { get; set; }

		[Column(TypeName = "boolean")]
        public bool IsView { get; set; }

        [Column(TypeName = "boolean")]
        public bool IsEdit { get; set; }

        [Column(TypeName = "boolean")]
        public bool IsDelete { get; set; }

        [ForeignKey(nameof(DocumentSharedID))]
        public virtual DocumentShared DocumentShared { get; set; }

        [ForeignKey(nameof(UserID))]
        public virtual User UsersInfo { get; set; }

		[ForeignKey(nameof(GroupID))]
		public virtual Group GroupInfo { get; set; }
	}
}
