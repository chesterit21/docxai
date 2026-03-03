using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.DataAccess.Models.Masters
{
    [Table("TblMsMenu")]
    [PrimaryKey(nameof(MenuId))]
    public class Menu : BaseEntityDefault, IEquatable<Menu>
    {
        [Column(TypeName = "varchar(30)")]
        public string MenuId { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string ParentMenuId { get; set; }

        [Required]
        public int Sequence { get; set; }

        [Required]
        public int Level { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Icon { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string Url { get; set; }

        [NotMapped]
        public List<Menu> Children { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserMatrix> UserMatrices { get; set; }

        [JsonIgnore]
        public virtual ICollection<RoleMatrix> RoleMatrices { get; set; }

        public bool Equals(Menu other)
        {
            return MenuId.Equals(other?.MenuId);
        }
    }
}
