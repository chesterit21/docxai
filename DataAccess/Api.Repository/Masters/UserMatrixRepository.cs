using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityResponses.Matrices;
using Api.Domain.EntityResponses.Users;
using Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Xml;

namespace Api.Repository.Masters
{
    public interface IUserMatrixRepository : IRepository<UserMatrix>
    {
        Task<int> DeleteInsert(List<UserMatrix> users);
        Task<List<ResponseUserMatrix>> GetAsync(int userId);
        Task<List<UserMatrix>> GetAsync(List<int> userIds);
    }

    public class UserMatrixRepository(DataContext context, IHttpContextAccessor accessor) : Repository<UserMatrix>(context, accessor), IUserMatrixRepository
    {
        public async Task<List<ResponseUserMatrix>> GetAsync(int userId)
        {
            /*
            var user = await context.User.FirstOrDefaultAsync(x => x.UserId == userId);
            var menus = await context.Menu.ToListAsync();
            var userMenus = await context.UserMatrix.Where(x => x.UserId == userId).ToListAsync();

            var matrices = new List<object>();
            foreach (var m in menus)
            {
                var rm = userMenus.FirstOrDefault(x => x.MenuId == m.MenuId);
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
                userId,
                fullName = user?.FullName,
                matrices
            };
            */

            return await context.Menu
            .LeftJoin(
                context.UserMatrix.Where(matrix => matrix.UserId == userId),
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

        public async Task<List<UserMatrix>> GetAsync(List<int> userIds)
        {
            return await context.UserMatrix
                .Include(x => x.Menu)
                .Include(x => x.User)
                .Where(x => x.Menu.IsActive && userIds.Contains(x.UserId)).ToListAsync();
        }

        public async Task<int> DeleteInsert(List<UserMatrix> items)
        {
            await DeleteManyAsync(items);
            var e = await InsertManyAsync(items);
            return e.Count;
        }
    }
}
