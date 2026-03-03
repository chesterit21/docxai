using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Api.Domain.EntityResponses.Dms.ResponseSearchNotification;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseSharedWorkspaceList
    {
		public List<ResponseSharedWorkspace> Records { get; set; }
		public long TotalRecord { get; set; }
	}
	public class ResponseSharedWorkspace
	{
        public int? DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentDescription { get; set; }
        public string LastUpdateName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
		//[JsonIgnore]
		//public long? SizeNumber { get; set; }
		public string Size { get; set; }
        public string Type { get; set; }
		public string FileType { get; set; }
		public int Owner { get; set; }
		public string OwnerFullname { get; set; }
		public int SharedWithUsersCount { get; set; }
		public bool IsFavorite { get; set; }
		public bool IsView { get; set; }
		public bool IsEdit { get; set; }
		public bool IsDelete { get; set; }
		[JsonIgnore]
		public long TotalSizeBytes { get; set; }
	}

	public class ResponseSharedWorkspaceIntial
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Type { get; set; }
		public string LastUpdateName { get; set; }
		public string LastUpdateDate { get; set; }
		public string OwnerFullname { get; set; }
		public int SharedWithUsersCount { get; set; }
	}
}
