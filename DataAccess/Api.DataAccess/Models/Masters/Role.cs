using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsRole")]
    [PrimaryKey(nameof(RoleId))]
    public class Role : BaseEntityDefault, IEquatable<Role>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

		[Column(TypeName = "varchar(100)")]
		public string Description { get; set; }

		[JsonIgnore]
        public virtual ICollection<UserRole> UserRoles { get; set; }

        [JsonIgnore]
        public virtual ICollection<RoleMatrix> RoleMatrices { get; set; }

        public bool Equals(Role other)
        {
            return RoleId.Equals(other?.RoleId);
        }
    }
}
