using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Masters
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<bool> CheckIfExist(List<int> roleIds);
        Task<int> DeleteWithChildren(List<int> roleIds);
        Task<Role> GetByName(string roleName);
        Task<object> GetByIdAndMembers(int roleId);
        bool UpdateRoleMembers(RequestRoleUpdate request);
        Task<List<DropdownTextValue>> GetDropdownRoles();
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
            //var roleMatrices = await context.RoleMatrix.Where(x => roleIds.Contains(x.RoleId)).ToListAsync();
            var userRoles = await context.UserRole.Where(x => roleIds.Contains(x.RoleId)).ToListAsync();
            var roles = await context.Role.Where(x => roleIds.Contains(x.RoleId)).ToListAsync();

            //context.RemoveRange(roleMatrices.ToArray());
            context.RemoveRange(userRoles.ToArray());
            context.RemoveRange(roles.ToArray());

            return await context.SaveChangesAsync();

            //await context.RoleMatrix.Where(x => ids.Contains(x.RoleId)).ExecuteDeleteAsync();
            //await context.UserRole.Where(x => ids.Contains(x.RoleId)).ExecuteDeleteAsync();

            //return await context.Group.Where(x => ids.Contains(x.RoleId)).ExecuteDeleteAsync();
        }

        public async Task<Role> GetByName(string roleName)
        {
            return await context.Role.FirstOrDefaultAsync(x => x.Name.Contains(roleName));
        }

        public async Task<object> GetByIdAndMembers(int roleId)
        {
            return await context.Role.Include(x => x.UserRoles).Where(x => x.RoleId == roleId).Select(x => new { x.Name, x.Description, x.UserRoles }).ToListAsync();
        }

        public async Task<List<DropdownTextValue>> GetDropdownRoles()
        {
            var users = await context.Role.Select(x => new DropdownTextValue { Text = x.Name, Value = x.RoleId }).ToListAsync();
            return users;

        }

        public bool UpdateRoleMembers(RequestRoleUpdate request)
        {
            {
                var executionStrategy = context.Database.CreateExecutionStrategy();
                executionStrategy.Execute(
                () =>
                {
                    using var transaction = context.Database.BeginTransaction();
                    {
                        try
                        {
                            var name = accessor?.HttpContext?.User?.Identity?.Name ?? "0";
                            int userId = int.Parse(name);

                            var role = new Role();
                            role.RoleId = request.RoleId;
                            role.Name = request.Name;
                            role.Description = request.Description;
                            role.UpdatedBy = userId;
                            role.UpdatedAt = DateTime.Now;

                            context.Update<Role>(role);
                            context.SaveChanges();

                            if (request.Users.Count > 0)
                            {
                                //var listUserRole = new List<UserRole>();
                                foreach (var a in request.Users)
                                {
                                    var userRole = new UserRole();
                                    userRole.RoleId = request.RoleId;
                                    userRole.UserId = a.UserId;
                                    userRole.InsertedBy = userId;
                                    userRole.InsertedAt = DateTime.Now;
                                    userRole.UpdatedBy = userId;
                                    userRole.UpdatedAt = DateTime.Now;

                                    //listUserRole.Add(userRole);
                                    var anyExist = context.UserRole.Count(u => u.RoleId == userRole.RoleId && u.UserId == userRole.UserId);
                                    if (anyExist > 0)
                                    {
                                        context.UserRole.Update(userRole);
                                    }
                                    else 
                                    {
                                        context.UserRole.AddAsync(userRole);
                                    }
                                }
                                
                                //context.BulkInsertOrUpdateAsync(listUserRole);
                                context.SaveChanges();
                            }

                            transaction.Commit();

                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                });
                return true;
            }
        }
    }
}