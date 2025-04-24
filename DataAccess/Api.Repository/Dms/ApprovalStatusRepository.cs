using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IApprovalStatusRepository : IRepository<ApprovalStatus>
    {
		
	}

    public class ApprovalStatusRepository(DataContext context, IHttpContextAccessor accessor) : Repository<ApprovalStatus>(context, accessor), IApprovalStatusRepository
	{
		
	}
}
