using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsLanguage")]
    [PrimaryKey(nameof(Code))]
    public class Language : BaseEntity, IEquatable<Language>
    {
        [Required]
        [Column(TypeName = "varchar(50)")]
        public required string Code { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string En { get; set; }

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Id { get; set; }

        /// <summary>
        /// Message, Label, Column
        /// </summary>
        [Column(TypeName = "varchar(10)")]
        public string Type { get; set; }

        public bool Equals(Language other)
        {
            return Code.Equals(other?.Code);
        }
    }
}
