using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.Enum;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseDocument
    {
        public int Id { get; set; }
        public int CategoryID { get; set; }
		public string DocumentTitle { get; set; }
        public string DocumentDesc { get; set; }
        public int Owner { get; set; }
        //public int FileSize { get; set; }
		public string FileSize { get; set; }

		public DateTime? ExpiryDate { get; set; }
        public int? RemindderDays { get; set; }
        public int? WatermarkID { get; set; }

        public DateTime? RemandireDateTime { get; set; }        

		public DateTime UpdatedAt { get; set; }
		public int UpdatedBy { get; set; }

		public int? ApprovalStatus { get; set; }
		public string ApprovalStatusDesc { get; set; }
		public ResponseDocumentFiles MainDocumentFile { get; set; }
		public ResponseUser UserofOwner { get; set; }
        public ResponseDocumentShared SharedTo { get; set; }
        public ResponseApprovals Workflow { get; set; }
        public List<ResponseDocumentFiles> DocumentFiles { get; set; }
        public ResponseDocumentAttributes DocumentAttributes { get; set; }
		public List<ResponseDocumentRelated> DocumentRelated { get; set; }
        public ResponseCategoryListItem Category { get; set; }
		public List<ParentCategory> ParentsCategory { get; set; }
	}
}
