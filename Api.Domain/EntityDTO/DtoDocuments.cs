using Api.Domain.EntityRequests.Dms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityDTO
{
	public class DTODocuments
	{
		public int Id { get; set; }

		public int CategoryID { get; set; }

		public string DocumentTitle { get; set; }

		public string DocumentDesc { get; set; }

		public int Owner { get; set; }

		public int? FileSize { get; set; }

		public DateTime? ExpiryDate { get; set; }

		public Int16? ReminderDays { get; set; }

		public DateTime? ReminderDateTime { get; set; }

		public int? WatermarkID { get; set; }

		//public virtual Categories Categories { get; set; }
		//public virtual User OwnerInfo { get; set; }

		//public virtual List<DocumentFiles> DocumentFiles { get; set; }
		//public virtual List<DocumentRelated> RelatedDocuments { get; set; }

		//[ForeignKey(nameof(WatermarkID))]
		//public virtual Watermarks Watermark { get; set; }
	}
}
