using Api.DataAccess;
using Api.DataAccess.Models.Systems;
using Microsoft.AspNetCore.Http;

namespace Api.Repository.Systems
{
    public interface IHistoryEmailRepository : IRepository<HistoryEmail>
    {
    }

    public class HistoryEmailRepository(DataContext context, IHttpContextAccessor accessor) : Repository<HistoryEmail>(context, accessor), IHistoryEmailRepository
    {
    }
}
