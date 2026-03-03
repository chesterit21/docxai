using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Enum
{
    public enum ApprovalStatusEnum
    {
		Draft = 0,
		Pending = 1,
        Approved = 2,
        Rejected = 3,
        None = 4
    }
    
    /*
     * pending approval = 2, approve = 3, reject = 4 it will affect css in FE
     * 
     */
    public enum ApprovalActivity
    {
        Draft = 0,
        Submit = 1,
        PendingApproval = 2,
        Approve = 3,
        Reject = 4
    }
}
