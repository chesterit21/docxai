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
	}
}
