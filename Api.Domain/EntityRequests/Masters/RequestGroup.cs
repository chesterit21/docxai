using Api.Domain.EntityRequests.Dms;
using System.ComponentModel.DataAnnotations;
using static Api.Domain.EntityRequests.Masters.RequestRoleMatrix;

namespace Api.Domain.EntityRequests.Masters
{
	public class RequestGroupList : RequestPagination
	{
		public string search { get; set; }
	}

	public class RequestGroupCreate
	{
		[Required]
		[MaxLength(100)]
		public string GroupName { get; set; }

		[MaxLength(500, ErrorMessage = "Group description cannot exceed 500 characters.")]
		public string GroupDescription { get; set; }
	}

	public class RequestGroupUpdate
	{
		[Required]
		public int GroupId { get; set; }

		[Required]
		[MaxLength(150)]
		public string GroupName { get; set; }

		[MaxLength(500)]
		public string GroupDescription { get; set; }

	}

	public class RequestGroupUpdateUser : RequestGroupUpdate
	{
		public List<RequestGroupUser> Users { get; set; }
	}

	public class RequestGroupUser
	{
		public int UserId { get; set; }
	}

}
