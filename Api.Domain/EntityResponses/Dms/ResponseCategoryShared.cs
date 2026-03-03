using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseCategoryShared
	{
		public int Id { get; set; }
		public int? CategoryID { get; set; }
		public string ShareType { get; set; } // fill with "user" or "group"
		public int? UserID { get; set; }
		public int? GroupID { get; set; }
		public bool IsView { get; set; }
		public bool IsEdit { get; set; }
		public bool IsDelete { get; set; }

		public string Photo { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public string GroupName { get; set; }

		//public List<ResponseUser> SharedUser { get; set; }
		//public ResponseUser User { get; set; }
		//public ResponseCategorySharedPrivilege Priv { get; set; }
	}

	//public class ResponseCategorySharedPrivilege
	//{
	//	public int CategorySharedID { get; set; }
	//	public bool IsView { get; set; }
	//	public bool IsEdit { get; set; }
	//	public bool IsDelete { get; set; }

	//}
}
