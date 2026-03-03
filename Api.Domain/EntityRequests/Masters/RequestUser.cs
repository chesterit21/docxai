using System.ComponentModel.DataAnnotations;

namespace Api.Domain.EntityRequests.Masters
{
	public class RequestUserList : RequestPagination
	{
		public string Username { get; set; }

		public string Fname { get; set; }
	}

	public class RequestUserRoleDelete
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class RequestUserRole
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public List<int> Roles { get; set; }
    }

    public class RequestUserMatrixDelete
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(30)]
        public string MenuId { get; set; }
    }

    public class RequestUserMatrix
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public List<UserMatrix> Matrices { get; set; }

        public class UserMatrix
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
