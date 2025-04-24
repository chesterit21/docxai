using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsRoleMatrix")]
    [PrimaryKey(nameof(RoleId), nameof(MenuId))]
    public class RoleMatrix : BaseEntityUpdate, IEquatable<RoleMatrix>
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MenuId { get; set; }

        public bool IsInsert { get; set; }

        public bool IsUpdate { get; set; }

        public bool IsDelete { get; set; }

        public bool IsRead { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; }

        [ForeignKey(nameof(MenuId))]
        public virtual Menu Menu { get; set; }

        public bool Equals(RoleMatrix other)
        {
            return RoleId.Equals(other?.RoleId) && MenuId.Equals(other?.MenuId);
        }
    }
}
