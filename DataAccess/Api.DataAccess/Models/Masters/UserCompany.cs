using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsUserCompany")]
    [PrimaryKey(nameof(UserId), nameof(CompanyId))]
    public class UserCompany : BaseEntityDefault, IEquatable<UserCompany>
    {
        [Required]
        //[Column(TypeName = "varchar(50)")]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string CompanyId { get; set; }

        [ForeignKey(nameof(UserId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual User User { get; set; }

        [ForeignKey(nameof(CompanyId))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual Company Company { get; set; }

        public bool Equals(UserCompany other)
        {
            return UserId.Equals(other?.UserId) && CompanyId.Equals(other?.CompanyId);
        }
    }
}
