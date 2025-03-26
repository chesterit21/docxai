using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Systems
{
    public interface IApplicationLogRepository : IRepository<ApplicationLog>
    {
        Task<List<ApplicationLog>> GetAsync(DateTime startDate, DateTime endDate, int page, int limit);
    }

    public class ApplicationLogRepository(DataContext context, IHttpContextAccessor accessor) : Repository<ApplicationLog>(context, accessor), IApplicationLogRepository
    {
        //public async Task<long> CountAsync(DateTime startDate, DateTime endDate)
        //{
        //    return await CountAsync(x => x.InsertedAt.Value.Date >= startDate && x.InsertedAt.Value.Date <= endDate);
        //}

        public async Task<List<ApplicationLog>> GetAsync(DateTime startDate, DateTime endDate, int page, int limit)
        {
            return await GetAsync(x => x.InsertedAt.Date >= startDate && x.InsertedAt.Date <= endDate, page, limit);
        }

        public async Task<object> GetAllAsync(DateTime startDate, DateTime endDate, int page, int limit)
        {
            var skip = Skip(page, limit);
            return await context.ApplicationLog
            .LeftJoin(
                context.User,
                at => at.InsertedBy,
                us => us.UserId,
                (at, us) => new
                {
                    at.Id,
                    at.IPAddress,
                    at.UserAgent,
                    at.Type,
                    at.Message,
                    at.StackTrace,
                    at.Endpoint,
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
