using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityResponses.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IApprovalFlowsRepository : IRepository<ApprovalFlows>
    {
		Task<bool> CheckIfEExist(List<int> ids);
		Task<int> Delete(List<int> ids);		
	}

    public class ApprovalFlowsRepository(DataContext context, IHttpContextAccessor accessor) : Repository<ApprovalFlows>(context, accessor), IApprovalFlowsRepository
	{
		public async Task<bool> CheckIfEExist(List<int> ids)
		{
			var count = await context.ApprovalFlows.CountAsync(x => ids.Contains(x.Id));
			return count == ids.Count;
		}

		public async Task<int> Delete(List<int> ids)
		{
			return await context.ApprovalFlows.Where(x => ids.Contains(x.Id)).ExecuteDeleteAsync();
		}
    }
}
