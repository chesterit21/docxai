using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.DataAccess.Models.Masters
{
	[Table("TblMsUserMedia")]
	[PrimaryKey(nameof(UserId))]
	public class UserMedia : BaseEntityDefault, IEquatable<UserMedia>
	{
		[Required]
		public int UserId { get; set; }

		public string PhotoName { get; set; }

		public byte[] PhotoImage { get; set; }

		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; }

		//[NotMapped]
		//public string Base64String { get; set; }

		//public string DateFormat { get; set; }

		public string PhotoUrl { get; set; }

		public bool Equals(UserMedia other)
		{
			return UserId.Equals(other?.UserId);
		}
	}
}
