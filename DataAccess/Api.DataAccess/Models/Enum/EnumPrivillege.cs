using Api.DataAccess.Models.Masters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Api.DataAccess.Models.Dms
{
	public enum EnumPrivillege
	{
		All,
		View,
		Edit,
		Delete
	}
}
