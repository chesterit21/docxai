using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityResponses.Matrices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Masters
{
    public interface IRoleMatrixRepository : IRepository<RoleMatrix>
    {
        Task<int> DeleteInsert(List<RoleMatrix> roles);
        Task<List<RoleMatrix>> GetAsync(List<int> roleIds);
        Task<List<ResponseUserMatrix>> GetAsync(int roleId);
    }

    public class RoleMatrixRepository(DataContext context, IHttpContextAccessor accessor) : Repository<RoleMatrix>(context, accessor), IRoleMatrixRepository
    {
        public async Task<bool> CheckIfExist(List<string> ids)
        {
            var count = await context.Menu.CountAsync(x => ids.Contains(x.MenuId));
            return count == ids.Count;
        }

        public async Task<List<ResponseUserMatrix>> GetAsync(int roleId)
        {
            /*
            var result1 = context.Menu
            .GroupJoin(
                context.RoleMatrix.Where(matrix => matrix.RoleId == roleId),
                menu => menu.MenuId,
                matrix => matrix.MenuId,
                (menu, matrixes) => new { menu, matrixes })
            .SelectMany(
                x => x.matrixes.DefaultIfEmpty(),
                (x, matrix) => new
                {
                    x.menu.MenuId,
                    x.menu.ParentMenuId,
                    x.menu.Sequence,
                    IsInsert = matrix != null ? matrix.IsInsert : false,
                    IsUpdate = matrix != null ? matrix.IsUpdate : false,
                    IsDelete = matrix != null ? matrix.IsDelete : false,
                    IsRead = matrix != null ? matrix.IsRead : false
                })
            .ToList();

            var result2 = context.Menu
            .LeftJoin(
                context.RoleMatrix.Where(matrix => matrix.RoleId == roleId),
                menu => menu.MenuId,
                matrix => matrix.MenuId,
                (menu, matrix) => new
                {
                    menu.MenuId,
                    menu.ParentMenuId,
                    menu.Sequence,
                    IsInsert = matrix != null ? matrix.IsInsert : false,
                    IsUpdate = matrix != null ? matrix.IsUpdate : false,
                    IsDelete = matrix != null ? matrix.IsDelete : false,
                    IsRead = matrix != null ? matrix.IsRead : false
                })
            .ToList();

            var role = await context.Group.FirstOrDefaultAsync(x => x.RoleId == roleId);
            var menus = await context.Menu.ToListAsync();
            var roleMenus = await context.RoleMatrix.Where(x => x.RoleId == roleId).ToListAsync();

            var matrices = new List<object>();
            foreach (var m in menus)
            {
                var rm = roleMenus.FirstOrDefault(x => x.MenuId == m.MenuId);
                var mnu = new
                {
                    m.MenuId,
                    m.ParentMenuId,
                    m.Sequence,
                    IsInsert = rm?.IsInsert ?? false,
                    IsDelete = rm?.IsDelete ?? false,
                    IsUpdate = rm?.IsUpdate ?? false,
                    IsRead = rm?.IsRead ?? false
                };

                matrices.Add(mnu);
            }
            return new
            {
                roleId,
                description = role?.Description,
                matrices
            };
            */

            /*
            var resultWithRole1 = context.Menu
            .LeftJoin(
                context.RoleMatrix.Where(matrix => matrix.RoleId == roleId),
                menu => menu.MenuId,
                matrix => matrix.MenuId,
                (menu, matrix) => new { menu, matrix })
            .LeftJoin(
                context.Group,
                temp => temp.matrix != null ? temp.matrix.RoleId : (Guid?)null,  // Ensure null safety
                role => role.RoleId,
                (temp, role) => new
                {
                    RoleId = role != null ? role.RoleId : (Guid?)null,
                    RoleDescription = role != null ? role.Description : null,
                    temp.menu.MenuId,
                    temp.menu.ParentMenuId,
                    temp.menu.Sequence,
                    IsInsert = temp.matrix != null ? temp.matrix.IsInsert : false,
                    IsUpdate = temp.matrix != null ? temp.matrix.IsUpdate : false,
                    IsDelete = temp.matrix != null ? temp.matrix.IsDelete : false,
                    IsRead = temp.matrix != null ? temp.matrix.IsRead : false
                })
            .ToList();

            var resultWithRole2 = context.Menu
            .GroupJoin(
                context.RoleMatrix.Where(matrix => matrix.RoleId == roleId),
                menu => menu.MenuId,
                matrix => matrix.MenuId,
                (menu, matrixes) => new { menu, matrixes })
            .SelectMany(
                x => x.matrixes.DefaultIfEmpty(),
                (x, matrix) => new { x.menu, matrix })
            .GroupJoin(
                context.Group,
                x => x.matrix != null ? x.matrix.RoleId : (Guid?)null,  // Check for null
                role => role.RoleId,
                (x, roles) => new { x.menu, x.matrix, roles })
            .SelectMany(
                x => x.roles.DefaultIfEmpty(),
                (x, role) => new
                {
                    RoleId = role != null ? role.RoleId : (Guid?)null,
                    RoleDescription = role != null ? role.Description : null,
                    MenuId = x.menu.MenuId,
                    ParentMenuId = x.menu.ParentMenuId,
                    Sequence = x.menu.Sequence,
                    IsInsert = x.matrix != null ? x.matrix.IsInsert : (bool?)null,
                    IsUpdate = x.matrix != null ? x.matrix.IsUpdate : (bool?)null,
                    IsDelete = x.matrix != null ? x.matrix.IsDelete : (bool?)null,
                    IsRead = x.matrix != null ? x.matrix.IsRead : (bool?)null
                })
            .ToList();
            */

            return await context.Menu
            .LeftJoin(
                context.RoleMatrix.Where(matrix => matrix.RoleId == roleId),
                menu => menu.MenuId,
                matrix => matrix.MenuId,
                (menu, matrix) => new ResponseUserMatrix
                {
                    MenuId = menu.MenuId,
                    ParentMenuId = menu.ParentMenuId,
                    Description = menu.Description,
                    Sequence = menu.Sequence,
                    IsInsert = matrix != null ? matrix.IsInsert : false,
                    IsUpdate = matrix != null ? matrix.IsUpdate : false,
                    IsDelete = matrix != null ? matrix.IsDelete : false,
                    IsRead = matrix != null ? matrix.IsRead : false
                })
            .ToListAsync();
        }

        public async Task<List<RoleMatrix>> GetAsync(List<int> roleIds)
        {
            return await context.RoleMatrix
                .Include(x => x.Menu)
                .Include(x => x.Role)
                .Where(x => x.Menu.IsActive && roleIds.Contains(x.RoleId)).ToListAsync();
        }

        public async Task<int> DeleteInsert(List<RoleMatrix> roles)
        {
            await DeleteManyAsync(roles);
            var e = await InsertManyAsync(roles);
            return e.Count;
        }
    }
}
