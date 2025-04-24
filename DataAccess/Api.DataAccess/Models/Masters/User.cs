using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsUser")]
    [PrimaryKey(nameof(UserId))]
    public class User : BaseEntityDefault, IEquatable<User>
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string UserName { get; set; }

        [Required]
        [Column(TypeName = "varchar(15)")]
        public string CompanyId { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string FullName { get; set; }

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string EmailAddress { get; set; }

        [Column(TypeName = "varchar(64)")]
        [JsonIgnore]
        //[DataType(DataType.Text)]
        public string UserPassword { get; set; }

        public bool EmailVerified { get; set; }

        public bool IsADUser { get; set; }

        public bool? IsLogin { get; set; }

        public DateTime? LastLogin { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public virtual Company Company { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserCompany> UserCompanies { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserMatrix> UserMatrices { get; set; }

        [JsonIgnore]
        //[JsonPropertyName("Roles")]
        public virtual ICollection<UserRole> UserRoles { get; set; }

        public bool Equals(User other)
        {
            return UserId.Equals(other?.UserId);
        }
    }
}
