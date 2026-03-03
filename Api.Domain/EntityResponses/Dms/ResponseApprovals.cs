using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseApprovals
    {
        public int Id { get; set; }
        public int? CategoryID { get; set; }
        public int? DocumentID { get; set; }
		public string Notes { get; set; }
        public int MaxStep { get; set; }
		public List<ResponseApprovalFlow> ApprovalFlows { get; set; }

        //public Categories Categories { get; set; }
        //public Documents Documents { get; set; }
    }
}
