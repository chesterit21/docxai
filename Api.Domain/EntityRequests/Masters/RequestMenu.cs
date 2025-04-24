using System.ComponentModel.DataAnnotations;

namespace Api.Domain.EntityRequests.Masters
{
    public class RequestMenu
    {
        [Required]
        [MaxLength(30)]
        public string MenuId { get; set; }

        [MaxLength(30)]
        public string ParentMenuId { get; set; }

        [Required]
        public int Sequence { get; set; }

        [Required]
        public int Level { get; set; }

        [MaxLength(50)]
        public string ID { get; set; }

        [MaxLength(50)]
        public string EN { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string Icon { get; set; }

        [MaxLength(200)]
        public string Url { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
