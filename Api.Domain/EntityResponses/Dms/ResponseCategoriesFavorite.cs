using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseCategoriesFavorite
	{
		public int Id { get; set; }
		public string CategoryName { get; set; }
		public string CategoryDesc { get; set; }
		public int Owner { get; set; }
		public string OwnerFullname { get; set; }
		public DateTime? UpdateAt { get; set; }
		public string Size { get; set; }
		public DateTime InsertedAt { get; set; }
		public string UpdatedByFullName { get; set; }
		public bool IsFavorite { get; set; }
	}

	public class ResponseCategoriesFavoriteAndCount
	{
		public long TotalRecord { get; set; }
		public List<ResponseCategoriesFavorite> Records { get; set; }
	}
}
