using System.ComponentModel.DataAnnotations;

namespace Api.Domain.EntityRequests.Authentications
{
    public record RequestLoginAttempt
    {
        public string UserName { get; set; }
        public DateTime LastLogin { get; set; }
        public int LoginCount { get; set; }
    }

    public class RequestUserIdUpdatePassword
    {
        [Required]
        public int UserId { get; set; }

        [MaxLength(20)]
        [Required]
        public string OldPassword { get; set; }

        [MaxLength(20)]
        [Required]
        public string NewPassword { get; set; }
    }

    public class RequestUserResetPassword
    {
        //[Required]
        //[MaxLength(50)]
        //public string Email { get; set; }

        [MaxLength(20)]
        [Required]
        public string Password { get; set; }

        [MaxLength(20)]
        [Required]
        public string RepeatPassword { get; set; }
    }

    public class RequestUserLogin
    {
        [MaxLength(50)]
        [Required]
        public string UserName { get; set; }

        [MaxLength(20)]
        [Required]
        public string Password { get; set; }
    }

    public class RequestUserCreate
    {
        [MaxLength(50)]
        [Required]
        public string UserName { get; set; }

        [MaxLength(100)]
        [Required]
        public string FullName { get; set; }

        [MaxLength(15)]
        [Required]
        public string CompanyId { get; set; }

        [Required]
        public List<int> Roles { get; set; } = [];

        public List<string> Companies { get; set; } = [];

        [Required]
        public bool IsADUser { get; set; }
    }

    public class RequestUserUpdate
    {
        [Required]
        public int UserId { get; set; }

        [MaxLength(50)]
        [Required]
        public string UserName { get; set; }

        //[MaxLength(30)]
        //public string Password { get; set; }

        [MaxLength(100)]
        [Required]
        public string FullName { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(15)]
        [Required]
        public string CompanyId { get; set; }

        [Required]
        public List<int> Roles { get; set; } = [];

        public List<string> Companies { get; set; } = [];

        [Required]
        public bool IsADUser { get; set; }
    }
}
