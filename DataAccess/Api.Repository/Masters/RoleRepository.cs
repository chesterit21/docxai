using Api.DataAccess;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Masters
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<bool> CheckIfExist(List<int> roleIds);
        Task<int> DeleteWithChildren(List<int> roleIds);
        Task<Role> GetByName(string roleName);
    }

    public class RoleRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Role>(context, accessor), IRoleRepository
    {
        public async Task<bool> CheckIfExist(List<int> roleIds)
        {
            var count = await context.Role.CountAsync(x => roleIds.Contains(x.RoleId));
            return count == roleIds.Count;
        }

        public async Task<int> DeleteWithChildren(List<int> roleIds)
        {
            var roleMatrices = await context.RoleMatrix.Where(x => roleIds.Contains(x.RoleId)).ToListAsync();
            var userRoles = await context.UserRole.Where(x => roleIds.Contains(x.RoleId)).ToListAsync();
            var roles = await context.Role.Where(x => roleIds.Contains(x.RoleId)).ToListAsync();

            context.RemoveRange(roleMatrices.ToArray());
            context.RemoveRange(userRoles.ToArray());
            context.RemoveRange(roles.ToArray());

            return await context.SaveChangesAsync();

            //await context.RoleMatrix.Where(x => ids.Contains(x.RoleId)).ExecuteDeleteAsync();
            //await context.UserRole.Where(x => ids.Contains(x.RoleId)).ExecuteDeleteAsync();

            //return await context.Role.Where(x => ids.Contains(x.RoleId)).ExecuteDeleteAsync();
        }

        public async Task<Role> GetByName(string roleName)
        {
            return await context.Role.FirstOrDefaultAsync(x => x.Name.Contains(roleName));
        }
    }
}
