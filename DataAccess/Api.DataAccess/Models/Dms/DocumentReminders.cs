using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Dms
{
	[Table("TblDocumentReminders")]
	public class DocumentReminders : BaseEntitySoftDelete
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[Column(TypeName = "integer")]
		public int DocumentID { get; set; }

		[Required]
		public DateTime ReminderDateTime { get; set; }

		[Required]
		[Column(TypeName = "text")]
		public string ReminderDesc { get; set; }
	}
}
