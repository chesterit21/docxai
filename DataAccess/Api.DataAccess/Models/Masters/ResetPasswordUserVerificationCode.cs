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
	[Table("TblMsUserResetPasswordVerificationCodes")]
	[PrimaryKey(nameof(UserId))]
	public class ResetPasswordUserVerificationCode : BaseEntity
	{
		[Required]
		public int UserId { get; set; }

		public string VerificationCode { get; set; }
	}
}
