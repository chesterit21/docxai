using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Api.Repository.Masters
{
	public interface IUserRepository : IRepository<User>
	{
		Task<bool> CheckIfExist(List<int> userIds);
		Task<int> DeleteWithChildren(List<int> userIds);

		Task<User> GetUser(string username, string password);
		//Task<object> GetUserAsResponse(string username);
		//Task<object> GetUserAsResponse(int userId);
		//Task<object> GetUsersAsResponse(int page, int limit);

		Task<User> GetUser(string username);
		Task<User> GetUser(int userId);
		Task<List<User>> GetUsers(int page, int limit);


		Task<bool> UpdatePassword(int userId, string password);
		Task<bool> VerifyEmail(int userId);
		Task<bool> CheckIfExist(int userId);
		Task<bool> SetLastLogin(int userId);
		Task<bool> SetIsLogin(int userId, bool isLogin);
		Task<bool> SetFullName(string username, string fullName);

		Task<List<DropdownUsers>> GetDropdownUsers();
	}

	public class UserRepository(DataContext context, IHttpContextAccessor accessor) : Repository<User>(context, accessor), IUserRepository
	{
		public async Task<List<User>> GetUsers(int page, int limit)
		{
			var skip = Skip(page, limit);

			var list = await context.User
			.Include(x => x.UserRoles)
				.ThenInclude(x => x.Role)
			.Include(x => x.Company)
			.Include(x => x.UserCompanies)
				.ThenInclude(x => x.Company)
			.LeftJoin(context.User,
				left => left.InsertedBy,
				user => user.UserId,
				(left, userInsert) => new
				{
					Entity = left,
					InsertedByUserName = userInsert.UserName,
					InsertedByFullName = userInsert.FullName,
					UpdatedByUserName = "",
					UpdatedByFullName = ""
				})
			.LeftJoin(context.User,
				"Entity.UpdatedBy",//left => left.Entity.UpdatedBy,
				"UserId",//user => user.UserId,
				(left, userUpdate) => new
				{
					left.Entity,
					left.InsertedByUserName,
					left.InsertedByFullName,
					UpdatedByUserName = userUpdate.UserName,
					UpdatedByFullName = userUpdate.FullName
				})
			//.Select(x => x.Entity) // Select only the User entity
			.Skip(skip)
			.Take(limit)
			.ToListAsync();

			return list.Select(x =>
			{
				x.Entity.InsertedByUserName = x.InsertedByUserName;
				x.Entity.InsertedByFullName = x.InsertedByFullName;
				x.Entity.UpdatedByUserName = x.UpdatedByUserName;
				x.Entity.UpdatedByFullName = x.UpdatedByFullName;
				return x.Entity;
			}).ToList();
			//return await context.User
			//    .Include(x => x.UserRoles)
			//    .ThenInclude(x => x.Role)
			//    .Include(x => x.Company)
			//    .Include(x => x.UserCompanies)
			//    .ThenInclude(x => x.Company)
			//    .Skip(skip).Take(limit).ToListAsync();

		}

		public async Task<User> GetUser(int userId)
		{
			return await context.User
				.Include(x => x.UserRoles)
				.ThenInclude(x => x.Role)
				.Include(x => x.Company)
				.Include(x => x.UserCompanies)
				.ThenInclude(x => x.Company)
				.FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive == true);
		}

		public async Task<User> GetUser(string username)
		{
			return await context.User
				 .Include(x => x.UserRoles)
				 .ThenInclude(x => x.Role)
				 .Include(x => x.Company)
				 .Include(x => x.UserCompanies)
				 .ThenInclude(x => x.Company)
				 .FirstOrDefaultAsync(x => x.UserName == username && x.IsActive == true);
		}

		public async Task<User> GetUser(string username, string password)
		{
			return await context.User
				//.Include(x => x.UserRoles)
				//.ThenInclude(x => x.Role)
				.Include(x => x.Company)
				.Include(x => x.UserCompanies)
				.ThenInclude(x => x.Company)
				.FirstOrDefaultAsync(x => x.UserName == username && x.UserPassword == password && x.IsActive == true);
		}

		public async Task<bool> UpdatePassword(int userId, string password)
		{
			//using var transaction = await context.Database.BeginTransactionAsync();
			//try
			//{
			var entity = new User { UserId = userId, UserPassword = password };
			context.Attach(entity);
			context.Entry(entity).Property(x => x.UserPassword).IsModified = true;

			return await context.SaveChangesAsync() > 0;
			//}
			//catch
			//{
			//    transaction.Rollback();
			//    throw;
			//}
		}

		public async Task<bool> VerifyEmail(int userId)
		{
			var entity = new User { UserId = userId, EmailVerified = true };
			context.Attach(entity);
			context.Entry(entity).Property(x => x.EmailVerified).IsModified = true;

			return await context.SaveChangesAsync() > 0;
		}

		public async Task<bool> CheckIfExist(List<int> userIds)
		{
			var count = await context.User.CountAsync(x => userIds.Contains(x.UserId));
			return count == userIds.Count;
		}

		public async Task<bool> CheckIfExist(int userId) => await context.User.AnyAsync(x => x.UserId == userId);

		public async Task<bool> SetLastLogin(int userId)
		{
			return await context.User.Where(x => x.UserId == userId)
				.ExecuteUpdateAsync(x => x.SetProperty(p => p.LastLogin, DateTime.UtcNow).SetProperty(p => p.IsLogin, true)) > 0;
		}

		public async Task<bool> SetIsLogin(int userId, bool isLogin)
		{
			return await context.User.Where(x => x.UserId == userId).ExecuteUpdateAsync(x => x.SetProperty(p => p.IsLogin, isLogin)) > 0;
		}

		public async Task<bool> SetFullName(string username, string fullName)
		{
			return await context.User.Where(x => x.UserName == username).ExecuteUpdateAsync(x => x.SetProperty(p => p.FullName, fullName)) > 0;
		}

		public async Task<int> DeleteWithChildren(List<int> userIds)
		{
			await context.UserCompany.Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
			await context.UserMatrix.Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
			await context.UserRole.Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
			return await context.User.Where(x => userIds.Contains(x.UserId)).ExecuteDeleteAsync();
		}

		public async Task<List<DropdownUsers>> GetDropdownUsers()
		{
			var users = await context.User.Select(x => new DropdownUsers { Photo = x.EmailAddress, Email = x.EmailAddress, FullName = x.FullName }).ToListAsync();
			return users;

		}
	}
}
