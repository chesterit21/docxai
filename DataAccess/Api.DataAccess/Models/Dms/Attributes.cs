using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblAttributes")]
    public class Attributes : BaseEntitySoftDelete
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string AttributeName { get; set; }

        [Column(TypeName = "json")]
        public string AttributeElement { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string AttributeType { get; set; }

		public bool IsSystem { get; set; } = false;

		//[Required]
		//[Column(TypeName = "integer")]
		//public int AttributeMaxLength { get; set; }

		//[ForeignKey(nameof(AttributeID))]
		//public virtual DocumentAttributes DocumentAttributes { get; set; }
	}
}
