using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsUserRoles")]
    [PrimaryKey(nameof(UserId), nameof(RoleId))]
    public class UserRole : BaseEntityUpdate, IEquatable<UserRole>
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; }

        public bool Equals(UserRole other)
        {
            return UserId.Equals(other?.UserId) && RoleId.Equals(other?.RoleId);
        }
    }
}
