using Api.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
	public class RequestCreateCategoriesShared
	{
		[Required]
		public int CategoryID { get; set; }
		//[Required]
		public List<ReqUserCategoryPrivillege> ReqUsersCategoryPriv { get; set; }
	}

	public class ReqUserCategoryPrivillege
	{
		public string ShareType { get; set; } // fill with "user" or "group"
		public int? GroupID { get; set; }
		public int? UserID { get; set; }
		public bool IsView { get; set; }
		public bool IsEdit { get; set; }
		public bool IsDelete { get; set; }
	}

	public class RequestCategoriesShared
	{
        public int Id { get; set; }
        public int CategoryID { get; set; }
        public List<Users> User { get; set; }
    }

    public class Users 
    {
        public int UserID { get; set; }
        public Dropdowns UserPrivillege { get; set; }
    }

    public class Dropdowns
	{
		public string Text { get; set; }
		public EnumPrivillege Value { get; set; }
	}
}
