using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Masters
{
    public interface IMenuRepository : IRepository<Menu>
    {
        Task<bool> CheckIfExist(List<string> menuIds);
        List<Menu> CreateNestedMenu(List<Menu> parents, List<Menu> allMenus);
        Task<int> DeleteWithChildren(List<string> ids);
        Task<List<Menu>> GetNestedMenu();
        //Task<List<Menu>> GetWithLanguage();
    }

    public class MenuRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Menu>(context, accessor), IMenuRepository
    {
        public async Task<bool> CheckIfExist(List<string> ids)
        {
            var count = await context.Menu.CountAsync(x => ids.Contains(x.MenuId));
            return count == ids.Count;
        }

        //public async Task<List<Menu>> GetWithLanguage() => await context.Menu
        //        .LeftJoin(context.Language,
        //            menu => menu.MenuId,
        //            lang => lang.Code,
        //            (menu, lang) => new Menu
        //            {
        //                MenuId = menu.MenuId,
        //                ParentMenuId = menu.ParentMenuId,
        //                Sequence = menu.Sequence,
        //                Level = menu.Level,
        //                Description = menu.Description,
        //                Icon = menu.Icon,
        //                Url = menu.Url,
        //                Id = lang.Id,
        //                En = lang.En
        //            })
        //        .ToListAsync();

        public List<Menu> CreateNestedMenu(List<Menu> parents, List<Menu> allMenus)
        {
            var result = new List<Menu>();
            foreach (var p in parents)
            {
                var children = allMenus.Where(c => c.ParentMenuId == p.MenuId).ToList();
                p.Children = children;

                result.Add(p);

                CreateNestedMenu(children, allMenus);

            }

            return result.OrderBy(x => x.Sequence).ToList();
        }

        public async Task<List<Menu>> GetNestedMenu()
        {
            var menu = await context.Menu.ToListAsync();
            var lookup = menu.ToLookup(x => x.ParentMenuId)[null].ToList();
            return CreateNestedMenu(lookup, menu).OrderBy(x => x.Sequence).ToList();
        }

        public async Task<int> DeleteWithChildren(List<string> ids)
        {
            await context.RoleMatrix.Where(x => ids.Contains(x.MenuId)).ExecuteDeleteAsync();
            await context.UserMatrix.Where(x => ids.Contains(x.MenuId)).ExecuteDeleteAsync();
            return await context.Menu.Where(x => ids.Contains(x.MenuId)).ExecuteDeleteAsync();
        }
    }
}