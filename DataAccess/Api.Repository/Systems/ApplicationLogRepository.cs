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
    }
}
