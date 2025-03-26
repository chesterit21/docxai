using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Systems;
using Api.Domain.EntityResponses.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Systems
{
    public interface IAuditTrailRepository : IRepository<AuditTrail>
    {
        Task<List<ResponseAuditTrail>> GetAllAsync(DateTime startDate, DateTime endDate, int page, int limit);
        Task<List<AuditTrail>> GetAsync(DateTime startDate, DateTime endDate, int page, int limit);
    }

    public class AuditTrailRepository(DataContext context, IHttpContextAccessor accessor) : Repository<AuditTrail>(context, accessor), IAuditTrailRepository
    {
        public async Task<List<AuditTrail>> GetAsync(DateTime startDate, DateTime endDate, int page, int limit)
        {
            return await GetAsync(x => x.InsertedAt.Date >= startDate && x.InsertedAt.Date <= endDate, page, limit);
        }

        public async Task<List<ResponseAuditTrail>> GetAllAsync(DateTime startDate, DateTime endDate, int page, int limit)
        {
            var skip = Skip(page, limit);
            return await context.AuditTrail
            .Where(x => x.InsertedAt >= startDate && x.InsertedAt <= endDate)
            .LeftJoin(
                context.User,
                at => at.InsertedBy,
                us => us.UserId,
                (at, us) => new ResponseAuditTrail
                {
                    Id = at.Id,
                    TransactionLogId = at.TransactionLogId,
                    TableName = at.TableName,
                    Command = at.Command,
                    Before = at.Before,
                    After = at.After,
                    InsertedBy = at.InsertedBy,
                    InsertedAt = at.InsertedAt,
                    InsertedByUserName = us.UserName,
                    InsertedByFullName = us.FullName
                })
            .OrderByDescending(x => x.InsertedAt)
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
        }
    }
}
