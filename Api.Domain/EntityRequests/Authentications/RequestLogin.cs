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

		[MaxLength(35)]
		[Required]
		public string NewPassword { get; set; }

		[MaxLength(35)]
		[Required]
		public string ConfirmNewPassword { get; set; }
	}

	public class RequestUserChangePasswordProfile
	{
		[Required]
		public int UserId { get; set; }

		[MaxLength(20)]
		[Required]
		public string OldPassword { get; set; }

		[MaxLength(35)]
		[Required]
		public string NewPassword { get; set; }

		[MaxLength(35)]
		[Required]
		public string ConfirmNewPassword { get; set; }
	}

	public class RequestUserResetPassword
	{
		//[Required]
		//[MaxLength(50)]
		//public string MailServer { get; set; }

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

		[MaxLength(150)]
		[Required]
		public string FullName { get; set; }

		[MaxLength(15)]
		//[Required]
		public string CompanyId { get; set; }

		//[Required]
		//public int GroupId { get; set; }

		[MaxLength(18)]
		//[Required]
		public string PhoneNumber { get; set; }

		[MaxLength(150)]
		[Required]
		public string EmailAddress { get; set; }

		public string UserType { get; set; } = "user";

		public List<int> Groups { get; set; } = [];

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

		//[Required]
		//public int GroupId { get; set; }

		[MaxLength(150)]
		[Required]
		public string EmailAddress { get; set; }

		public string UserType { get; set; } = "user";

		public List<int> Groups { get; set; } = [];

		[MaxLength(15)]
		[Required]
		public string CompanyId { get; set; }

		//[Required]
		//public List<int> Roles { get; set; } = [];

		[MaxLength(18)]
		//[Required]
		public string PhoneNumber { get; set; }

		public List<string> Companies { get; set; } = [];

		[Required]
		public bool IsADUser { get; set; }
	}

	public class RequestNewUserCreate
	{
		[MaxLength(50)]
		[Required]
		public string UserName { get; set; }

		[MaxLength(100)]
		[Required]
		public string FullName { get; set; }

		[MaxLength(100)]
		[Required]
		public string EmailAddress { get; set; }

        [Required]
        public string UserPassword { get; set; }

        [MaxLength(15)]
		[Required]
		public string CompanyId { get; set; }

		public bool IsActive { get; set; }

		[Required]
		public List<int> RolesId { get; set; }
        public bool EmailVerified { get; set; }
		public bool IsADUser { get; set; }
        public string UserType { get; set; }
	}

	public class RequestUpdateNewUser
	{
		public int UserId { get; set; }

		[MaxLength(50)]
		[Required]
		public string UserName { get; set; }

		[MaxLength(100)]
		[Required]
		public string FullName { get; set; }

		[MaxLength(100)]
		[Required]
		public string EmailAddress { get; set; }

		[MaxLength(15)]
		[Required]
		public string CompanyId { get; set; }

		public bool IsActive { get; set; }

		public List<int> RolesId { get; set; }
	}

	public class RequestUpdateUserInfo
	{
		[Required]
		public int UserId { get; set; }

		[MaxLength(150)]
		[Required]
		public string FullName { get; set; }

		[MaxLength(150)]
		[Required]
		public string EmailAddress { get; set; }

		//[MaxLength(100)]
		//[Required]
		public string UserPassword { get; set; }

		[MaxLength(18)]
		[Required]
		public string PhoneNumber { get; set; }

		//[Required]
		//public string DateFormat { get; set; }
	}

	public class RequestResetPassword
	{

		[MaxLength(150)]
		[Required]
		public string EmailAddress { get; set; }
	}
}
