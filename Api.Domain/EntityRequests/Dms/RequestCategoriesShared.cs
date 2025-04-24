using Api.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
    public class RequestCategoriesShared
	{
        public int Id { get; set; }
        public int CategoryID { get; set; }
        public List<Users> User { get; set; }
    }

    public class Users { 

        public int UserID { get; set; }
        public Dropdowns UserPrivillege { get; set; }
    }

    public class Dropdowns
	{
		public string Text { get; set; }
		public EnumPrivillege Value { get; set; }
	}
}
