using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Systems
{
    public interface ITransactionLogRepository : IRepository<TransactionLog>
    {
        Task<object> GetAllAsync(DateTime startDate, DateTime endDate, int page, int limit);
        Task<List<TransactionLog>> GetAsync(DateTime startDate, DateTime endDate, int page, int limit);
    }

    public class TransactionLogRepository(DataContext context, IHttpContextAccessor accessor) : Repository<TransactionLog>(context, accessor), ITransactionLogRepository
    {
        public async Task<List<TransactionLog>> GetAsync(DateTime startDate, DateTime endDate, int page, int limit)
        {
            return await GetAsync(x => x.InsertedAt.Date >= startDate && x.InsertedAt.Date <= endDate, page, limit);
        }

        public async Task<object> GetAllAsync(DateTime startDate, DateTime endDate, int page, int limit)
        {
            var skip = Skip(page, limit);
            return await context.TransactionLog
            .Where(x => x.InsertedAt >= startDate && x.InsertedAt <= endDate)
            .LeftJoin(
                context.User,
                at => at.InsertedBy,
                us => us.UserId,
                (at, us) => new
                {
                    at.Id,
                    at.IPAddress,
                    at.UserAgent,
                    at.Action,
                    at.Description,
                    at.Path,
                    at.Parameter,
                    at.InsertedBy,
                    at.InsertedAt,
                    us.UserName,
                    us.FullName
                })
            .OrderByDescending(x => x.InsertedAt)
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
        }
    }
}
