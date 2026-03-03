using Api.DataAccess;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityResponses.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Masters
{
    public interface IUserGroupRepository : IRepository<UserGroup>
    {
        Task<int> DeleteInsert(int userId, List<UserGroup> items);
        Task<List<int>> GetGroupByUserId(int userId);
		Task<List<UserGroup>> GetUsersByGroupId(int groupId);
	}

    public class UserGroupRepository(DataContext context, IHttpContextAccessor accessor) : Repository<UserGroup>(context, accessor), IUserGroupRepository
	{
        public async Task<List<int>> GetGroupByUserId(int userId)
        {
            return await context.UserGroup.Where(x => x.UserId == userId).Select(x => x.GroupId).ToListAsync();
        }

		public async Task<List<UserGroup>> GetUsersByGroupId(int groupId)
		{
			return await context.UserGroup.Include(x => x.User).Where(x => x.GroupId == groupId).ToListAsync();
		}

		public async Task<int> DeleteInsert(int userId, List<UserGroup> items)
        {
            await context.UserGroup.Where(x => x.UserId == userId && x.Group.GroupName.ToLower() != "everyone").ExecuteDeleteAsync();
            var e = await InsertManyAsync(items);
            return e.Count;
        }
    }
}
