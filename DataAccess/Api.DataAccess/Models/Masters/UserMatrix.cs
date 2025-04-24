using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsUserMatrix")]
    [PrimaryKey(nameof(UserId), nameof(MenuId))]
    public class UserMatrix : BaseEntityUpdate, IEquatable<UserMatrix>
    {
        [Required]
        //[Column(TypeName = "varchar(50)")]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string MenuId { get; set; }

        public bool IsInsert { get; set; }

        public bool IsUpdate { get; set; }

        public bool IsDelete { get; set; }

        public bool IsRead { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        [ForeignKey(nameof(MenuId))]
        public virtual Menu Menu { get; set; }

        public bool Equals(UserMatrix other)
        {
            return UserId.Equals(other?.UserId) && MenuId.Equals(other?.MenuId);
        }
    }
}
