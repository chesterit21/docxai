using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblDocuments")]
    public class Documents : BaseEntitySoftDelete, IHasOwner
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int CategoryID { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string DocumentTitle { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string DocumentDesc { get; set; }

        [Column(TypeName = "integer")]
        public int Owner { get; set; }

        [Column(TypeName = "integer")]
        public int? FileSize { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Column(TypeName = "smallint")]
        public Int16? ReminderDays { get; set; }

        public DateTime? ReminderDateTime { get; set; }

		[Column(TypeName = "integer")]
		public int? WatermarkID { get; set; }

		[ForeignKey(nameof(CategoryID))]
        public virtual Categories Categories { get; set; }

        [ForeignKey(nameof(Owner))]
        public virtual User OwnerInfo { get; set; }

        public virtual List<DocumentFiles> DocumentFiles { get; set; }
		public virtual List<DocumentRelated> RelatedDocuments { get; set; }

		[ForeignKey(nameof(WatermarkID))]
		public virtual Watermarks Watermark { get; set; }
	}
}