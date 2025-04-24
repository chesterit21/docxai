using Api.DataAccess;
using Api.DataAccess.Models.Systems;
using Microsoft.AspNetCore.Http;

namespace Api.Repository.Systems
{
    public interface IHistoryTransactionLogRepository : IRepository<HistoryTransactionLog>
    {
    }

    public class HistoryTransactionLogRepository(DataContext context, IHttpContextAccessor accessor) : Repository<HistoryTransactionLog>(context, accessor), IHistoryTransactionLogRepository
    {
    }
}
