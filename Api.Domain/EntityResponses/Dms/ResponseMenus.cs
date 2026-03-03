using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseMenus
	{
		public string ParentMenuId { get; set; }
		public string MenuId { get; set; }
		public string Description { get; set; }
		public string Id { get; set; }
		public string En { get; set; }
		public int Sequence { get; set; }
		public int Level { get; set; }
		public string Url { get; set; }
		public string Icon { get; set; }
		public bool IsInsert { get; set; }
		public bool IsUpdate { get; set; }
		public bool IsDelete { get; set; }
		public bool IsRead { get; set; }
	}
}
