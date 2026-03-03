using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math;
using System.Transactions;

namespace Api.Repository.Dms
{
	public interface ICategoriesSharedRepository : IRepository<CategoriesShared>
	{
		Task<bool> InsertCategoriesShared(RequestCreateCategoriesShared request);
		Task<List<ResponseCategoryShared>> GetSharedUsersByCategoryIDAsync(int categoryId);
		Task<int> DeleteSharedCategory(int categoryId);
	}

	public class CategoriesSharedRepository(DataContext context, IHttpContextAccessor accessor) : Repository<CategoriesShared>(context, accessor), ICategoriesSharedRepository
	{

		public async Task<bool> InsertCategoriesShared(RequestCreateCategoriesShared request)
		{
			var executionStrategy = context.Database.CreateExecutionStrategy();
			var isSuccess = false;
			int result = 0;
			await executionStrategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						List<CategoriesShared> newCome = new List<CategoriesShared>();
						List<CategoriesShared> existing = await context.CategoriesShared.Where(y => y.CategoryID == request.CategoryID).ToListAsync();

						CategoriesShared modelCategoriesShared = null;
						foreach (var x in request.ReqUsersCategoryPriv)
						{
							modelCategoriesShared = new CategoriesShared()
							{
								CategoryID = request.CategoryID,
								ShareType = x.ShareType,
								UserID = x.UserID,
								GroupID = x.GroupID,
								IsView = x.IsView,
								IsEdit = x.IsEdit,
								IsDelete = x.IsDelete,
								InsertedBy = UserId,
								InsertedAt = DateTime.Now,
								UpdatedBy = UserId,
								UpdatedAt = DateTime.Now,
							};
							newCome.Add(modelCategoriesShared);
						}


						var toDelete = existing.Where(x => !newCome.Contains(x));
						if (toDelete.Count() > 0)
						{
							context.CategoriesShared.RemoveRange(toDelete);
							result += context.SaveChanges();
						}

						if (newCome.Count() > 0)
						{
							context.CategoriesShared.AddRange(newCome);
							result += context.SaveChanges();
						}

						transaction.Commit();
						isSuccess = true;
						return isSuccess;
					}

					catch (Exception ex)
					{
						transaction.Rollback();
						isSuccess = false;
						throw;

					}
				}
			});
			return isSuccess;
		}

		public async Task<List<ResponseSharedWorkspace>> ListCategoryShared()
		{
			var name = accessor?.HttpContext?.User?.Identity?.Name ?? "0";
			int userId = int.Parse(name);
			var categorySharedList = await context.CategoriesShared.Include(x => x.Category).ThenInclude(x => x.Documents)
				.Where(x => x.UserID == userId).Select(x => new ResponseSharedWorkspace
				{
					DocumentId = x.Id,
					DocumentName = x.Category.CategoryName,
					DocumentDescription = x.Category.CategoryDesc,
					Type = "Category",
					LastUpdateName = x.UpdatedByFullName,
					LastUpdateDate = DateTime.Now,
					Size = x.Category.Documents.Sum(x => x.FileSize).ToString()
				}).ToListAsync();
			return categorySharedList;

		}

		public async Task<List<ResponseCategoryShared>> GetSharedUsersByCategoryIDAsync(int categoryId)
		{
			var result = await context.CategoriesShared.Include(b => b.user).Include(c=>c.GroupInfo).Include(e => e.Category)
				.Where(d => d.CategoryID == categoryId)
				.Select(d => new ResponseCategoryShared()
				{
					Id = d.Id,
					CategoryID = d.CategoryID,
					ShareType = d.ShareType,
					UserID = d.UserID,
					GroupID = d.GroupID,
					IsView = d.IsView,
					IsDelete = d.IsDelete,
					IsEdit = d.IsEdit,
					FullName = d.user.FullName,
					Email = d.user.EmailAddress,
					GroupName = d.GroupInfo.GroupName
					//User = new ResponseUser { ApproverUserID = d.ApproverUserID, UserName = d.user == null ? "" : d.user.UserName, FullName = d.user == null ? "" : d.user.FullName, GroupID = d.GroupID, GroupName = d.GroupInfo == null ? null : d.GroupInfo.GroupName },
					//Priv = new ResponseCategorySharedPrivilege { CategorySharedID = d.Id, IsView = d.IsView, IsEdit = d.IsEdit, IsDelete = d.IsDelete }
				}).ToListAsync();
			return result;
		}

		public async Task<int> DeleteSharedCategory(int categoryId)
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						var categoryShared = await context.CategoriesShared.Where(x => x.CategoryID == categoryId).ToListAsync();
						if (categoryShared != null)
						{
							context.CategoriesShared.RemoveRange(categoryShared);
							context.SaveChanges();
						}
						await transaction.CommitAsync();
						result++;
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						throw;
					}
				}
			});
			return result;
		}
	}
}