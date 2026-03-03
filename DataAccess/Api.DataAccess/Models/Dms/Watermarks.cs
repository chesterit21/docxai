using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblWatermarks")]
    public class Watermarks : BaseEntitySoftDelete
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string Text { get; set; }

		public new int InsertedBy { get; set; }

		[ForeignKey(nameof(InsertedBy))]
		public User InsertedUser { get; set; }

		public new int? UpdatedBy { get; set; }

		[ForeignKey(nameof(UpdatedBy))]
		public User UpdatedUser { get; set; }
	}
}
