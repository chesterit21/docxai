using Api.Domain.EntityRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseDocumentFiles
    {
        public int Id { get; set; }
        public int DocumentID { get; set; }
        public string DocumentType { get; set; }
        public string DocumentFileName { get; set; }
        public string DocumentFileSize { get; set; }
        public int DocumentFileSizeInt { get; set; }

		public string DocumentFileContent { get; set; }
        public string DocumentFilePath { get; set; }
        public bool IsMainDocumentFile { get; set; }
		public DateTime UpdateAt { get; set; }
		public string UpdateBy { get; set; }
        public string UpdatedByUserName { get; set; }
		public string UpdatedByFullName { get; set; }
		//public virtual ResponseDocument Documents { get; set; }
		public string NewDocumentFileName { get; set; }
		public int CategoryID { get; set; }
		public string CategoryName { get; set; }
		public string WaterMark { get; set; }
		public string OwnerUserName { get; set; }
		public int OwnerUserID { get; set; }
		//public List<ResponseUser> SharedUser { get; set; }
	}

	public class ResponseDocumentFilesPagination : RequestPagination
	{
		public List<ResponseDocumentFiles> Record { get; set; }
		public long TotalRecord { get; set; }
	}

	public class ResponseApprovalDocumentFiles
	{
		public int Id { get; set; }
		public int DocumentID { get; set; }
		public string DocumentType { get; set; }
		public string DocumentFileName { get; set; }
		public string DocumentFileSize { get; set; }
		public string DocumentFilePath { get; set; }
		public DateTime UpdateAt { get; set; }
		public string UpdatedByFullName { get; set; }
		public int CategoryID { get; set; }
		public string CategoryName { get; set; }
		public string WaterMark { get; set; }
	}
}
