namespace Api.Domain.EntityResponses.Users
{
    public class ResponseUserPagination
    {
		public List<ResponseUserList> UsersRecord { get; set; }
		public long TotalRecord { get; set; }
	}

    public class ResponseUserList
    {
		public int UserId { get; set; }
		public string UserName { get; set; }
		public string EmailAddress { get; set; }
		public string PhoneNumber { get; set; }
		public string FullName { get; set; }
		public string UserType { get; set; }
		public string CompanyId { get; set; }
		public string CompanyName { get; set; }
		public DateTime InsertedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public List<Groupr> Groups { get; set; }
		public bool IsActive { get; set; }
		public bool IsDeleted { get; set; } = false;
		public string UserStatus { get; set; }
		public DateTime? LastLogin { get; set; }
		//public List<Company> Companies { get; set; }
		public string InsertedByFullName { get; set; }
		public string UpdatedByFullName { get; set; }
		public class Groupr
		{
			public int GroupId { get; set; }
			public string GroupName { get; set; }
		}

		public class Company
		{
			public string CompanyId { get; set; }
			public string CompanyName { get; set; }
		}
	}
}
