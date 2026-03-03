using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblCategories")]
    public class Categories : BaseEntitySoftDelete, IHasOwner
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string CategoryName { get; set; }

        [Column(TypeName = "text")]
        public string CategoryDesc { get; set; }

		[Column(TypeName = "integer")]
		public int Owner { get; set; }

		public int? ParentId { get; set; }

		[ForeignKey(nameof(ParentId))]
        public virtual Categories ParentCategory { get; set; }
        
        public virtual List<Categories> ChildCategories { get; set; }
        public bool IsNeedApproval { get; set; }		

		[ForeignKey(nameof(Owner))]
		public virtual User OwnerInfo { get; set; }

		//[NotMapped]
		public ICollection<CategoriesShared> CategoriesShareds { get; set; }

		//[NotMapped]
		public ICollection<Documents> Documents { get; set; }

		//[NotMapped]
		//public virtual Approvals Approval { get; set; }
		public ICollection<Approvals> Approvals { get; set; }

		public ICollection<CategoriesFavorite> CategoriesFavorite { get; set; }
	}
}
