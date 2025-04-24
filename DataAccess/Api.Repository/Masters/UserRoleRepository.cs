using Api.DataAccess;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Masters
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        Task<int> DeleteInsert(int userId, List<UserRole> items);
        Task<List<int>> GetRolesByUserId(int userId);
    }

    public class UserRoleRepository(DataContext context, IHttpContextAccessor accessor) : Repository<UserRole>(context, accessor), IUserRoleRepository
    {
        public async Task<List<int>> GetRolesByUserId(int userId)
        {
            return await context.UserRole.Where(x => x.UserId == userId).Select(x => x.RoleId).ToListAsync();
        }

        public async Task<int> DeleteInsert(int userId, List<UserRole> items)
        {
            await context.UserRole.Where(x => x.UserId == userId).ExecuteDeleteAsync();
            var e = await InsertManyAsync(items);
            return e.Count;
        }
    }
}
