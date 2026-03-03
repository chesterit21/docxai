using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IApprovalActivityRepository : IRepository<ApprovalActivities>
    {
		
	}

    public class ApprovalActivityRepository(DataContext context, IHttpContextAccessor accessor) : Repository<ApprovalActivities>(context, accessor), IApprovalActivityRepository
	{
		
	}
}
