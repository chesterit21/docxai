using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    //[Table(nameof(Company), Schema = "master")]
    [Table("TblMsCompany")]
    [PrimaryKey(nameof(CompanyId))]
    public class Company : BaseEntityDefault, IEquatable<Company>
    {
        [Required]
        [Column(TypeName = "varchar(15)")]
        public required string CompanyId { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string ParentCompanyId { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Address { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string PhoneNumber { get; set; }

        public bool Equals(Company other)
        {
            return CompanyId.Equals(other?.CompanyId);
        }
    }
}
