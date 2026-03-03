using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsGroup")]
    [PrimaryKey(nameof(GroupId))]
    public class Group : BaseEntitySoftDelete, IEquatable<Group>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupId { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string GroupName { get; set; }

		[Column(TypeName = "varchar(500)")]
		public string GroupDescription { get; set; }

		[JsonIgnore]
		public virtual ICollection<UserGroup> UserGroup { get; set; }

        public bool IsSystem { get; set; } = false;

		public bool Equals(Group other)
        {
            return GroupId.Equals(other?.GroupId);
        }
    }
}
