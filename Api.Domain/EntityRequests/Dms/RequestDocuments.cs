using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestDocuments
	{
		[Required]
		public int Id { get; set; }
		[Required]
		public int CategoryID { get; set; }
		[Required]
		public string DocumentTitle { get; set; }
		[Required]
		public string DocumentDesc { get; set; }
		[Required]
		public int Owner { get; set; }

		public DateTime? ExpiryDate { get; set; }
		public Int16? ReminderDays { get; set; }

		public DateTime? ReminderDateTime { get; set; }
		public int? RelatedDocumentID { get; set; }
		[Required]
		public string DocumentAttributes { get; set; }
		public RequestDocumentFiles DocumentFiles { get; set; }
		public RequestDocumentShared RequestDocumentShared { get; set; }
		public List<RequestApprovalFlows> ReqApprovalFlows { get; set; }


		//public virtual Categories Categories { get; set; }
		//public virtual User OwnerInfo { get; set; }
		//public virtual List<DocumentFiles> DocumentFiles { get; set; }
	}

	public class RequestUpdateDocuments
	{
		[Required]
		public int Id { get; set; }
		[Required]
		public int CategoryID { get; set; }
		[Required]
		public string DocumentTitle { get; set; }
		[Required]
		public string DocumentDesc { get; set; }
		[Required]
		public int Owner { get; set; }

		//public DateTime? ExpiryDate { get; set; }
		//public Int16? ReminderDays { get; set; }

		//public DateTime? ReminderDateTime { get; set; }
		//public int? RelatedDocumentID { get; set; }
		public List<int> RelatedDocumentIDs { get; set; }
		[Required]
		public string DocumentAttributes { get; set; }
		public int? WatermarkID { get; set; }
		//public virtual Categories Categories { get; set; }
		//public virtual User OwnerInfo { get; set; }
		//public virtual List<DocumentFiles> DocumentFiles { get; set; }
	}
}
