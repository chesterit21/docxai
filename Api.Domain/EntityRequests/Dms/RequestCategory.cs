using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
    public class RequestCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDesc { get; set; }
        public int? ParentId { get; set; }
        public virtual RequestCategory ParentCategory { get; set; }
        public virtual List<RequestCategory> ChildCategories { get; set; }
        public bool IsNeedApproval { get; set; }
    }
}
