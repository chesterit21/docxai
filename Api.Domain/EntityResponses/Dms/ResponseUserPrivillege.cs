using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseUserPrivillege
	{
		public string ShareType { get; set; }
		public int? UserID { get; set; }
		public int? GroupID { get; set; }
		public string GroupName { get; set; }
		public string UserName { get; set; }
        public string FullName { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public bool IsView { get; set; }
		public bool IsEdit { get; set; }
		public bool IsDelete { get; set; }
	}
}
