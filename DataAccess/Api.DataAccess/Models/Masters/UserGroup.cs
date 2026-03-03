using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsUserGroups")]
    [PrimaryKey(nameof(UserId), nameof(GroupId))]
    public class UserGroup : BaseEntityUpdate, IEquatable<UserGroup>
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int GroupId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual Group Group { get; set; }

        public bool Equals(UserGroup other)
        {
            return UserId.Equals(other?.UserId);
        }
    }
}
