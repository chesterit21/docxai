using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Api.DataAccess.Models.Dms
{
	public class Dropdown : BaseEntity
	{
		public string Text { get; set; }
		public EnumPrivillege Value { get; set; }

	}

	public class DropdownUsers
	{
		public string Photo { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public int UserId { get; set; }
	}

	public class DropdownUsersAndGroups
	{
		public string Photo { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public int UserId { get; set; }
		public string Type { get; set; }
		public int GroupId { get; set; }
		public string GroupName { get; set; }
	}

	public class DropdownTextValue
	{
		public string Text { get; set; }
		public int Value { get; set; }
	}

	public class DropdownTextValueString
	{
		public string Text { get; set; }
		public string Value { get; set; }
	}

}
