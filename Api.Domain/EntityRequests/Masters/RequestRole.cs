using System.ComponentModel.DataAnnotations;

namespace Api.Domain.EntityRequests.Masters
{
    public class RequestRoleCreate
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class RequestRoleUpdate
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class RequestRoleMatrix
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public List<RoleMatrix> Matrices { get; set; }

        public class RoleMatrix
        {
            [Required]
            [MaxLength(30)]
            public string MenuId { get; set; }

            [Required]
            public bool IsInsert { get; set; }

            [Required]
            public bool IsUpdate { get; set; }

            [Required]
            public bool IsDelete { get; set; }

            [Required]
            public bool IsRead { get; set; }
        }
    }

}
