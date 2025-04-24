using Api.DataAccess;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Masters
{
    public interface IUserCompanyRepository : IRepository<UserCompany>
    {
        Task<int> DeleteInsert(int userId, List<UserCompany> items);
        Task<List<string>> GetCompanyIdsByUserId(int userId);
        Task<List<Company>> GetCompaniesByUserId(int userId);
    }

    public class UserCompanyRepository(DataContext context, IHttpContextAccessor accessor) : Repository<UserCompany>(context, accessor), IUserCompanyRepository
    {
        public async Task<List<string>> GetCompanyIdsByUserId(int userId)
        {
            return await context.UserCompany.Where(x => x.UserId == userId).Select(x => x.CompanyId).ToListAsync();
        }

        public async Task<List<Company>> GetCompaniesByUserId(int userId)
        {
            return await context.UserCompany
                .Include(x => x.CompanyId)
                .Where(x => x.UserId == userId)
                .Select(x => x.Company)
                .ToListAsync();
        }

        public async Task<int> DeleteInsert(int userId, List<UserCompany> items)
        {
            await context.UserCompany.Where(x => x.UserId == userId).ExecuteDeleteAsync();
            var e = await InsertManyAsync(items);
            return e.Count;
        }
    }
}
