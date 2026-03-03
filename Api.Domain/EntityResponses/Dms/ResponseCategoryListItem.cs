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
	public class ResponseCategoriesAndCount
	{
		public List<ResponseCategoryListItem> Records { get; set; }
		public long TotalRecord { get; set; }
	}

    public class ResponseCategoryListItem
    {
        public int Id { get; set; }
		public string CategoryName { get; set; }
		public string CategoryDesc { get; set; }
		public int Owner { get; set; }
		public string OwnerFullName { get; set; }
		public string OwnerUserName { get; set; }
		public DateTime? LastUpdateDate { get; set; }
		public DateTime? InsertedAt { get; set; }
		public string InsertedByFullName { get; set; }
		public string UpdatedByUserName { get; set; }
		public string UpdatedByFullName { get; set; }
		public int? ParentId { get; set; }
		public List<ParentCategory> Parents { get; set; } = new List<ParentCategory>();
		public bool IsFavorite { get; set; }
		public RCategorySharedPrivillege Privillege { get; set; }
		public bool IsShared { get; set; }
		public bool IsNeedApproval { get; set; }
		public bool IsOwned { get; set; }
		public string SharedByFullName { get; set; }
		public DateTime? SharedAt { get; set; }
	}

	public class RCategorySharedPrivillege
	{
		public bool? IsView { get; set; }
		public bool? IsEdit { get; set; }
		public bool? IsDelete { get; set; }
	}

	public class ParentCategory
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
