using NPOI.POIFS.Properties;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.DataAccess.Models.Masters;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblCategoriesShared")]
    public class CategoriesShared : BaseEntitySoftDelete
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Required]
        [Column(TypeName = "integer")]
        public int? CategoryID { get; set; }

		[Required]
		[Column(TypeName = "varchar(5)")]
		public string ShareType { get; set; } = "user";// fill with "user" or "group"
		
		[Column(TypeName = "integer")]
		public int? UserID { get; set; }
		//[Required]
		[Column(TypeName = "integer")]
		public int? GroupID { get; set; }

        [ForeignKey(nameof(CategoryID))]
        public virtual Categories Category { get; set; }

        [ForeignKey(nameof(UserID))]
        public virtual User user { get; set; }

		[Column(TypeName = "boolean")]
		public bool IsView { get; set; }

		[Column(TypeName = "boolean")]
		public bool IsEdit { get; set; }

		[Column(TypeName = "boolean")]
		public bool IsDelete { get; set; }

		[ForeignKey(nameof(GroupID))]
		public virtual Group GroupInfo { get; set; }

		//public virtual CategoriesSharedPrivillege Priv { get; set; }

	}
}
