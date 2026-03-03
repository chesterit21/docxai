using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseDocumentFileApproval
    {
        public int Id { get; set; }
        public int CategoryID { get; set; }
        public int DocumentID { get; set; }
        public string DocumentTitle { get; set; }
        public List<ResponseApprovalFlow> ApprovalFlows { get; set; }
        public List<ResponseDocumentFiles> DocumentFiles { get; set; }
    }
}
