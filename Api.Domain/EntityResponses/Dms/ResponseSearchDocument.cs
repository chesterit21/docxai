using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Domain.EntityRequests.Dms;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseSearchDocument
	{
		public List<DocumentList> Records{ get; set; }
		public long TotalRecord { get; set; }

		public class DocumentList
		{
			public int DocumentId { get; set; }
			public int CategoryID { get; set; }
			public string CategoryName { get; set; }
			public string DocumentName { get; set; }
			public string Description { get; set; }
			public int Owner { get; set; }
			public string OwnerName { get; set; }
			public string FilesSize { get; set; }

			public DateTime? ExpiryDate { get; set; }
			public int? ReminderDays { get; set; }
			public int? DateDiff { get; set; }

			public int? UpdatedBy { get; set; } = 0;
			public DateTime UpdatedAt { get; set; }
			public int InsertedBy { get; set; } = 0;
			public DateTime InsertedAt { get; set; }

			public DateTime? LastUpdateDate { get; set; }
			public bool IsFavorite { get; set; }
			public string InsertedByUserName { get; set; }
			public string InsertedByFullName { get; set; }
			public string UpdatedByUserName { get; set; }
			public string UpdatedByFullName { get; set; }
			public string FileType { get; set; }
			public RDocumentSharedPrivillege Privillege { get; set; }			
		}

		public class RDocumentSharedPrivillege {
			public bool? IsView { get; set; }
			public bool? IsEdit { get; set; }
			public bool? IsDelete { get; set; }
		}
	}
}
