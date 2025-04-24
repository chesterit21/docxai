using Api.DataAccess;
using Api.DataAccess.Models.Systems;
using Microsoft.AspNetCore.Http;

namespace Api.Repository.Systems
{
    public interface IEmailRepository : IRepository<Email>
    {
    }

    public class EmailRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Email>(context, accessor), IEmailRepository
    {
    }
}
