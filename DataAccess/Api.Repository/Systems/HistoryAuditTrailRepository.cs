using Api.DataAccess;
using Api.DataAccess.Models.Systems;
using Microsoft.AspNetCore.Http;

namespace Api.Repository.Systems
{
    public interface IHistoryAuditTrailRepository : IRepository<HistoryAuditTrail>
    {
    }

    public class HistoryAuditTrailRepository(DataContext context, IHttpContextAccessor accessor) : Repository<HistoryAuditTrail>(context, accessor), IHistoryAuditTrailRepository
    {
    }
}
