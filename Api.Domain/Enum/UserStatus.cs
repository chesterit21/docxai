using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Enum
{
    public enum UserStatus
	{
		EmailVerifying = 0,
		Active = 1,
        InActive = 2,
        RecycleBin = 3,
        Deleted = 4
    }
}
