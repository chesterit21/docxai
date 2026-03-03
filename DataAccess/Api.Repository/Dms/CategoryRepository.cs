using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Enum;
using Api.Domain.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Api.Domain.EntityResponses.Dms.ResponseDocumentItemlist;

namespace Api.Repository.Masters
{
	public interface ICategoryRepository : IOwnerPrivilegesRepository<Categories>
	{
		Task<List<Categories>> GetNestedCategory(int? CategoryId = null);
		Task<CategoriesFavorite> AddFavorite(int categoryId);
		Task<int> RemoveFavorite(int CategoryId);
		Task<CategoriesFavorite> CheckFavorite(int CategoryId);
		Task<int> AddWorkFlow(Approvals approval, List<ApprovalFlows> flows);
		Task<int> DeleteWorkFlow(int categoryId);
		Task<ResponseCategoriesAndCount> GetCategories(string categoryName, bool isActive, int? parentcategory, int page = 0, int limit = 0);
		Task<bool> MarkAsDeletedAsync(int categoryId);
		Task<bool> MarkAsUnDeletedAsync(int categoryId);
		Task<bool> EmptyRecyclebin(int batchSize);
		Task<int> DeleteAsync(int documentId);
		Task<List<DropdownTextValue>> GetDropdownCategories(string categoryname = null);
		Task<int> PurgeDeletedCategories(int batchSize, int purgingInMonth);
	}

	public class CategoryRepository(DataContext context, IHttpContextAccessor accessor) : OwnerPrivilegesRepository<Categories>(context, accessor), ICategoryRepository
	{
		private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

		private async Task<List<int>> GetUserGroupIdsWithCaching(int userId)
		{
			string cacheKey = $"usergroups_{userId}";
			if (!_cache.TryGetValue(cacheKey, out List<int> userGroupIds))
			{
				userGroupIds = await context.UserGroup
					.Where(ug => ug.UserId == userId)
					.Select(ug => ug.GroupId)
					.ToListAsync();

				var cacheOptions = new MemoryCacheEntryOptions()
					.SetSlidingExpiration(TimeSpan.FromMinutes(30));

				_cache.Set(cacheKey, userGroupIds, cacheOptions);
			}
			return userGroupIds;
		}

		public List<Categories> CreateNestedCategory(List<Categories> parents, List<Categories> categrories)
		{
			var result = new List<Categories>();
			foreach (var p in parents)
			{
				var children = categrories.Where(c => c.ParentId == p.Id).ToList();
				p.ChildCategories = children;

				result.Add(p);

				CreateNestedCategory(children, categrories);

			}
			return result.OrderBy(x => x.InsertedAt).ToList();
		}

		public async Task<List<Categories>> GetNestedCategory(int? CategoryId = null)
		{
			List<Categories> cats = new List<Categories>();
			if (CategoryId != null)
			{
				var category = await context.Categories.Where(x => x.Owner == UserId && x.Id == CategoryId && x.IsActive == true).ToListAsync();
				var lookup = category.ToLookup(x => x.ParentId)[null].ToList();
				cats = CreateNestedCategory(lookup, category).OrderBy(x => x.InsertedAt).ToList();
			}
			else
			{
				var category = await context.Categories.Where(x => x.Owner == UserId && x.IsActive == true).ToListAsync();
				var lookup = category.ToLookup(x => x.ParentId)[null].ToList();
				cats = CreateNestedCategory(lookup, category).OrderBy(x => x.InsertedAt).ToList();
			}
			return cats;
		}

		public async Task<CategoriesFavorite> AddFavorite(int categoryId)
		{
			var entity = new CategoriesFavorite() { CategoryID = categoryId, Owner = UserId, InsertedAt = DateTime.Now, InsertedBy = UserId };
			await context.CategoriesFavorites.AddAsync(entity);
			await context.SaveChangesAsync();
			return entity;
		}

		public async Task<int> RemoveFavorite(int categoryId)
		{
			var dv = await context.CategoriesFavorites.FirstOrDefaultAsync(x => x.CategoryID == categoryId);
			context.CategoriesFavorites.Remove(dv);
			int res = await context.SaveChangesAsync();
			return res;
		}
		public async Task<CategoriesFavorite> CheckFavorite(int categoryId)
		{
			return await context.CategoriesFavorites.FirstOrDefaultAsync(x => x.CategoryID == categoryId);
		}

		public async Task<int> AddWorkFlow(Approvals approval, List<ApprovalFlows> flows)
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						Approvals dataExist = await context.Approvals.FirstOrDefaultAsync(x => x.CategoryID == approval.CategoryID && x.DocumentID == null);
						if (dataExist != null)
						{
							approval.UpdatedBy = UserId;
							approval.UpdatedAt = DateTime.Now;
							approval.MaxStep = flows.Count();
							context.Approvals.Update(approval);
							await context.SaveChangesAsync();

							foreach (var flow in flows)
							{
								flow.InsertedAt = DateTime.Now;
								flow.InsertedBy = UserId;
								flow.UpdatedAt = DateTime.Now;
								flow.UpdatedBy = UserId;
								flow.ApprovalID = approval.Id;

								//delete item from request if exsist in database
								ApprovalFlows existFlow = await context.ApprovalFlows.FirstOrDefaultAsync(d => d.ApprovalID == flow.ApprovalID && d.ApproverUserID == flow.ApproverUserID);
								if (existFlow != null)
								{
									flows.Remove(flow);
								}
							}
						}
						else
						{
							approval.InsertedAt = DateTime.Now;
							approval.InsertedBy = UserId;
							approval.UpdatedBy = UserId;
							approval.UpdatedAt = DateTime.Now;
							await context.Approvals.AddAsync(approval);
							await context.SaveChangesAsync();
							foreach (var flow in flows)
							{
								flow.ApprovalID = approval.Id;
							}
							//ApprovalActivities activity = new ApprovalActivities()
							//{
							//	ApprovalID = approvalDC.Id,
							//	CurrentStep = 0,
							//	NextStep = 0,
							//	ApprovalActivity = ApprovalActivity.Draft,
							//	Remark = "Add Workflow for this categories",
							//	InsertedAt = DateTime.Now,
							//	InsertedBy = UserId
							//};
							//await context.ApprovalActivities.AddAsync(activity);
							//await context.SaveChangesAsync();
						}

						await context.ApprovalFlows.AddRangeAsync(flows);
						await context.SaveChangesAsync();

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

		public async Task<int> DeleteWorkFlow(int categoryId)
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						Approvals approval = new Approvals();
						approval = await context.Approvals.FirstOrDefaultAsync(x => x.CategoryID == categoryId && x.DocumentID == null);
						if (approval != null)
						{
							List<ApprovalActivities> activities = context.ApprovalActivities.Where(x => x.ApprovalID == approval.Id).ToList();
							if (activities.Count() > 0)
							{
								context.ApprovalActivities.RemoveRange(activities);
								context.SaveChanges();
							}

							List<ApprovalFlows> approvalFlows = context.ApprovalFlows.Where(x => x.ApprovalID == approval.Id).ToList();
							if (approvalFlows.Count() > 0)
							{
								context.ApprovalFlows.RemoveRange(approvalFlows);
								context.SaveChanges();
							}
						}

						context.Approvals.Remove(approval);
						context.SaveChanges();

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

		private async Task<List<int>> GetAllChildCategoryIds(int parentCategoryId)
		{
			var childIds = new List<int>();
			var directChildren = await context.Categories
				.Where(c => c.ParentId == parentCategoryId && c.IsActive == true)
				.Select(c => c.Id)
				.ToListAsync();

			childIds.AddRange(directChildren);

			foreach (var childId in directChildren)
			{
				var grandChildren = await GetAllChildCategoryIds(childId);
				childIds.AddRange(grandChildren);
			}

			return childIds;
		}

		public async Task<ResponseCategoriesAndCount> GetCategories(string categoryName, bool isActive, int? parentcategory, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);

			var userGroups = context.UserGroup.Where(ug => ug.UserId == UserId).Select(ug => ug.GroupId);

			#region old query atas
			//var userGroupIds = await context.UserGroup
			//	.Where(ug => ug.UserId == UserId)
			//	.Select(ug => ug.GroupId)
			//	.ToListAsync();

			//// Get all categories shared with the user along with their privileges
			//var sharedCategoriesQuery = context.CategoriesShared
			//	.Where(cs => cs.CategoryID == parentcategory &&
			//		((cs.UserID == UserId && cs.ShareType == "user") ||
			//		 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value))))
			//	.Select(cs => new
			//	{
			//		CategoryId = cs.CategoryID,
			//		IsView = cs.IsView,
			//		IsEdit = cs.IsEdit,
			//		IsDelete = cs.IsDelete
			//	});
			#endregion old query atas

			var userGroupIds = await GetUserGroupIdsWithCaching(UserId);

			// Get the parent category's shared privileges
			var sharedParentCategory = await context.CategoriesShared
				.Where(cs => cs.CategoryID == parentcategory &&
					((cs.UserID == UserId && cs.ShareType == "user") ||
					 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value))))
				.FirstOrDefaultAsync();

			// Get all child category IDs if parent is shared
			var sharedCategoryIds = new List<int>();
			if (sharedParentCategory != null)
			{
				sharedCategoryIds = await GetAllChildCategoryIds(parentcategory.Value);
				sharedCategoryIds.Add(parentcategory.Value); // Include parent
			}

			// Modified sharedCategoriesQuery to include child categories
			var sharedCategoriesQuery = context.CategoriesShared
				.Where(cs => (sharedCategoryIds.Contains(cs.CategoryID.Value) ||
					((cs.UserID == UserId && cs.ShareType == "user") ||
					 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value)))))
				.Select(cs => new
				{
					CategoryId = cs.CategoryID,
					IsView = cs.IsView || (sharedParentCategory != null && sharedParentCategory.IsView),
					IsEdit = cs.IsEdit || (sharedParentCategory != null && sharedParentCategory.IsEdit),
					IsDelete = cs.IsDelete || (sharedParentCategory != null && sharedParentCategory.IsDelete)
				});

			// Optimize the query by pre-filtering and reducing joins
			var query = context.Categories
				.Include(c => c.OwnerInfo)
				.Where(x => x.IsActive == isActive)
				.AsSplitQuery()
				.AsNoTracking()  // Add this for better performance since we're only reading
				.Select(c => new
				{
					Category = c,
					UpdatedBy = context.User
						.Where(u => u.UserId == c.UpdatedBy)
						.Select(u => new { u.UserName, u.FullName })
						.FirstOrDefault()
				})
				.Where(x =>
					(x.Category.Owner == UserId) || // Own categories
					sharedCategoryIds.Contains(x.Category.Id) || // Categories inherited from parent
					context.CategoriesShared.Any(cs =>
						cs.CategoryID == x.Category.Id &&
						((cs.UserID == UserId && cs.ShareType == "user") ||
						 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value)))
					)
				)
				.Select(x => new ResponseCategoryListItem
				{
					Id = x.Category.Id,
					CategoryName = x.Category.Owner == UserId ? x.Category.CategoryName : x.Category.CategoryName + " (" + x.Category.OwnerInfo.UserName + ")",
					CategoryDesc = x.Category.CategoryDesc,
					Owner = x.Category.Owner,
					OwnerFullName = x.Category.OwnerInfo.FullName,
					LastUpdateDate = x.Category.UpdatedAt,
					InsertedAt = x.Category.InsertedAt,
					UpdatedByUserName = x.UpdatedBy.UserName,
					UpdatedByFullName = x.UpdatedBy.FullName,
					IsFavorite = context.CategoriesFavorites
						.Any(f => f.CategoryID == x.Category.Id && f.Owner == UserId),
					ParentId = x.Category.ParentId,
					Privillege = new RCategorySharedPrivillege
					{
						IsView = x.Category.Owner == UserId ||
								context.CategoriesShared
									.Where(cs => cs.CategoryID == x.Category.Id &&
										((cs.UserID == UserId && cs.ShareType == "user") ||
										 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value))))
									.Select(cs => cs.IsView).FirstOrDefault() ||
										(sharedParentCategory != null && sharedParentCategory.IsView),
						IsEdit = x.Category.Owner == UserId ||
								context.CategoriesShared
									.Where(cs => cs.CategoryID == x.Category.Id &&
										((cs.UserID == UserId && cs.ShareType == "user") ||
										 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value))))
									.Select(cs => cs.IsEdit).FirstOrDefault() ||
										(sharedParentCategory != null && sharedParentCategory.IsEdit),
						IsDelete = x.Category.Owner == UserId ||
								context.CategoriesShared
									.Where(cs => cs.CategoryID == x.Category.Id &&
										((cs.UserID == UserId && cs.ShareType == "user") ||
										 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value))))
									.Select(cs => cs.IsDelete).FirstOrDefault() ||
										(sharedParentCategory != null && sharedParentCategory.IsDelete)
					},
					IsNeedApproval = context.Approvals
						.Any(a => a.CategoryID == x.Category.Id && a.DocumentID == null),
					IsOwned = x.Category.Owner == UserId,
					IsShared = context.CategoriesShared
						.Any(cs => cs.CategoryID == x.Category.Id &&
							((cs.UserID == UserId && cs.ShareType == "user") ||
							 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value)))),
					SharedAt = context.CategoriesShared
						.Where(cs => cs.CategoryID == x.Category.Id &&
							((cs.UserID == UserId && cs.ShareType == "user") ||
							 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value))))
						.Select(cs => cs.InsertedAt)
						.FirstOrDefault(),
					SharedByFullName = context.CategoriesShared
						.Where(cs => cs.CategoryID == x.Category.Id &&
							((cs.UserID == UserId && cs.ShareType == "user") ||
							 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value))))
						.Select(cs => context.User
							.Where(u => u.UserId == cs.InsertedBy)
							.Select(u => u.FullName)
							.FirstOrDefault())
						.FirstOrDefault()
				})
				.WhereIf(parentcategory == null && isActive == true,
					src => (src.Owner == UserId && src.ParentId == null) || src.IsShared)
				.WhereIf(parentcategory == null && UserRoleInApp == "superadmin" && isActive == false,
					src => src.ParentId == null)
				.WhereIf(!string.IsNullOrEmpty(categoryName),
					x => x.CategoryName.Trim().ToLower().Contains(categoryName.Trim().ToLower()))
				.WhereIf(parentcategory > 0,
					x => x.ParentId == parentcategory)
				.Distinct();

			#region old query
			//var query = context.Categories
			//	.Include(c => c.OwnerInfo)//.ThenInclude(f => f.UserGroup)
			//	.Where(x => x.IsActive == isActive)
			//	//.WhereIf(UserRoleInApp == "user", x => x.Owner == UserId)
			//	.GroupJoin(
			//		context.User,
			//		c => c.UpdatedBy,
			//		u => u.UserId,
			//		(c, users) => new { c, users }
			//	)
			//	.SelectMany(
			//		x => x.users.DefaultIfEmpty(),
			//		(x, u) => new
			//		{
			//			A = x.c,
			//			UpdatedByUserName = u != null ? u.UserName : null,
			//			UpdatedByFullName = u != null ? u.FullName : null
			//		}
			//	)
			//	.GroupJoin(
			//		context.CategoriesShared,
			//		a => a.A.Id,
			//		s => s.CategoryID,
			//		(a, shareds) => new { a, shareds }
			//	)
			//	.SelectMany(
			//		x => x.shareds.Where(sh =>
			//				(sh.UserID == UserId && sh.ShareType == "user") ||
			//				(sh.GroupID != null && sh.ShareType == "group" && userGroups.Contains(sh.GroupID.Value))
			//			).DefaultIfEmpty(),
			//		(x, sh) => new
			//		{
			//			B = x.a,
			//			SH = sh
			//		}
			//	).GroupJoin(
			//		context.User,
			//		b => b.SH != null ? b.SH.InsertedBy : (int?)null,
			//		u => u.UserId,
			//		(b, sharedBy) => new { b, sharedBy }
			//	)
			//	.SelectMany(
			//		x => x.sharedBy.DefaultIfEmpty(),
			//		(x, createdUser) => new
			//		{
			//			C = x.b,
			//			SharedByFullname = createdUser != null ? createdUser.FullName : null,
			//			SharedAt = createdUser != null ? createdUser.InsertedAt : (DateTime?)null
			//		}
			//	)
			//	.GroupJoin(
			//		context.CategoriesFavorites,
			//		b => b.C.B.A.Id,
			//		f => f.CategoryID,
			//		(b, favs) => new { b, favs }
			//	)
			//	.SelectMany(
			//		x => x.favs.DefaultIfEmpty(),
			//		(x, fav) => new
			//		{
			//			D = x.b,
			//			DF = fav
			//		}
			//	)
			//	.GroupJoin(
			//		context.Approvals.Where(c => c.DocumentID == null),
			//		c => c.D.C.B.A.Id,
			//		app => app.CategoryID,
			//		(c, apps) => new { c, apps }
			//	)
			//	.SelectMany(
			//		x => x.apps.DefaultIfEmpty(),
			//		(x, app) => new
			//		{
			//			E = x.c,
			//			App = app
			//		}
			//	)
			//	.Select(x => new ResponseCategoryListItem()
			//	{
			//		Id = x.E.D.C.B.A.Id,
			//		CategoryName = x.E.D.C.B.A.CategoryName,
			//		CategoryDesc = x.E.D.C.B.A.CategoryDesc,
			//		Owner = x.E.D.C.B.A.Owner,
			//		OwnerFullName = x.E.D.C.B.A.OwnerInfo.FullName,
			//		LastUpdateDate = x.E.D.C.B.A.UpdatedAt,
			//		InsertedAt = x.E.D.C.B.A.InsertedAt,
			//		UpdatedByUserName = x.E.D.C.B.UpdatedByUserName,
			//		UpdatedByFullName = x.E.D.C.B.UpdatedByFullName,
			//		IsFavorite = x.E.DF != null && x.E.DF.CategoryID > 0,
			//		ParentId = x.E.D.C.B.A.ParentId,
			//		Privillege = new RCategorySharedPrivillege() { IsView = x.E.D.C.SH.IsView, IsEdit = x.E.D.C.SH.IsEdit, IsDelete = x.E.D.C.SH.IsDelete },
			//		IsNeedApproval = x.App != null,
			//		IsOwned = x.E.D.C.SH == null,
			//		IsShared = x.E.D.C.SH != null,
			//		SharedAt = x.E.D.SharedAt,
			//		SharedByFullName = x.E.D.SharedByFullname
			//	})
			//	//.WhereIf(parentcategory == null && UserRoleInApp == "user", src => (src.Owner == UserId && src.ParentId == null) || src.IsShared == true)
			//	.WhereIf(parentcategory == null && isActive == true, src => (src.Owner == UserId && src.ParentId == null) || src.IsShared == true)
			//	.WhereIf(parentcategory == null && UserRoleInApp == "superadmin" && isActive == false, src => src.ParentId == null)
			//	.WhereIf(!string.IsNullOrEmpty(categoryName), x => x.CategoryName.Trim().ToLower().Contains(categoryName.Trim().ToLower()))
			//	.WhereIf(parentcategory > 0, x => x.ParentId == parentcategory)
			//	.Distinct();
			#endregion old query
			////if (!string.IsNullOrWhiteSpace(orderBy))
			////	query = query.OrderBy(x => $"x.{orderBy} {orderOrientation}");

			ResponseCategoriesAndCount rdoc = new ResponseCategoriesAndCount() { TotalRecord = query.Count() };

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			rdoc.Records = await query.ToListAsync();
			return rdoc;
		}

		private RCategorySharedPrivillege SetPrivillege(CategoriesShared sh = null)
		{
			RCategorySharedPrivillege returnObj = null;
			if (sh != null)
			{
				return returnObj = new RCategorySharedPrivillege() { IsView = sh?.IsView, IsEdit = sh?.IsEdit, IsDelete = sh?.IsDelete };
			}
			return new RCategorySharedPrivillege() { IsView = true, IsEdit = true, IsDelete = true };
		}

		public async Task<bool> MarkAsDeletedAsync(int categoryId)
		{
			int result = 0;
			var entity = await context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
			if (entity != null)
			{
				entity.IsActive = false;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Categories.Update(entity);
				result += await context.SaveChangesAsync();
			}
			return result > 0;
		}

		public async Task<bool> EmptyRecyclebin(int batchSize)
		{
			int result = 0;


			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						var categoriesToDelete = await context.Categories.Where(x => x.IsActive == false && x.IsDeleted == false && x.Owner == UserId).ToListAsync();

						//.WhereIf(UserRoleInApp == "user", x => x.Owner == UserId).ToListAsync();
						if (categoriesToDelete.Count == 0)
						{
							result = 0;
						}

						for (int i = 0; i < categoriesToDelete.Count; i += batchSize)
						{
							var batch = categoriesToDelete.Skip(i).Take(batchSize).ToList();
							for (int j = 0; j < batch.Count; j += batchSize)
							{
								int categoryID = batch[j].Id;

								var approval = await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == null && x.CategoryID == categoryID && x.IsDeleted == false);
								if (approval != null)
								{
									var appActv = await context.ApprovalActivities.Where(x => x.ApprovalID == approval.Id && x.IsActive == false && x.IsDeleted == false).ToListAsync();
									foreach (var act in appActv)
									{
										act.IsDeleted = true;
										act.DeletedBy = UserId;
										act.DeletedAt = DateTime.Now;
									}
									context.ApprovalActivities.UpdateRange(appActv);
									await context.SaveChangesAsync();

									var appFlows = await context.ApprovalFlows.Where(x => x.ApprovalID == approval.Id && x.IsActive == false && x.IsDeleted == false).ToListAsync();
									foreach (var act in appFlows)
									{
										act.IsDeleted = true;
										act.DeletedBy = UserId;
										act.DeletedAt = DateTime.Now;
									}
									context.ApprovalFlows.UpdateRange(appFlows);
									await context.SaveChangesAsync();
								}

								var favs = await context.CategoriesFavorites.Where(x => x.CategoryID == categoryID && x.IsActive == false && x.IsDeleted == false).ToListAsync();
								foreach (var act in favs)
								{
									act.IsDeleted = true;
									act.DeletedBy = UserId;
									act.DeletedAt = DateTime.Now;
								}
								context.CategoriesFavorites.UpdateRange(favs);
								await context.SaveChangesAsync();

								var shared = await context.CategoriesShared.Where(x => x.CategoryID == categoryID && x.IsActive == false && x.IsDeleted == false).ToListAsync();
								if (shared.Count > 0)
								{
									foreach (var share in shared)
									{
										share.IsDeleted = true;
										share.DeletedBy = UserId;
										share.DeletedAt = DateTime.Now;
									}
									context.CategoriesShared.UpdateRange(shared);
									await context.SaveChangesAsync();
								}

								//Documets in category
								var documentsToDelete = await context.Documents.Where(x => x.CategoryID == categoryID && x.IsActive == false && x.IsDeleted == false).ToListAsync();
								for (int k = 0; k < documentsToDelete.Count; k += batchSize)
								{
									var batchdocument = documentsToDelete.Skip(i).Take(batchSize).ToList();
									int batchDocCount = batchdocument.Count();
									for (int x = 0; x < batchDocCount; x++)
									{
										int idDocument = batch[x].Id;

										var logs = await context.DocumentLog.Where(x => x.DocumentID == idDocument).ToListAsync();
										foreach (var log in logs)
										{
											log.IsDeleted = true;
											log.DeletedBy = UserId;
											log.DeletedAt = DateTime.Now;
										}
										context.DocumentLog.UpdateRange(logs);
										result += await context.SaveChangesAsync();

										var approvalDC = await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == idDocument && x.CategoryID != null);
										if (approvalDC != null)
										{
											var activities = await context.ApprovalActivities.Where(x => x.ApprovalID == approvalDC.Id).ToListAsync();
											foreach (var actAppDoc in activities)
											{
												actAppDoc.IsDeleted = true;
												actAppDoc.DeletedBy = UserId;
												actAppDoc.DeletedAt = DateTime.Now;
											}
											context.ApprovalActivities.UpdateRange(activities);
											result += await context.SaveChangesAsync();

											var flows = await context.ApprovalFlows.Where(x => x.ApprovalID == approvalDC.Id).ToListAsync();
											foreach (var flw in flows)
											{
												flw.IsDeleted = true;
												flw.DeletedBy = UserId;
												flw.DeletedAt = DateTime.Now;
											}
											context.ApprovalFlows.UpdateRange(flows);
											result += await context.SaveChangesAsync();

											approvalDC.IsDeleted = true;
											approvalDC.DeletedBy = UserId;
											approvalDC.DeletedAt = DateTime.Now;
											context.Approvals.UpdateRange(approvalDC);
											result += await context.SaveChangesAsync();
										}


										var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == idDocument).ToListAsync();
										foreach (var dcAttr in docAttr)
										{
											dcAttr.IsDeleted = true;
											dcAttr.DeletedBy = UserId;
											dcAttr.DeletedAt = DateTime.Now;
										}

										context.DocumentAttributes.UpdateRange(docAttr);
										result += context.SaveChanges();

										var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == idDocument).ToListAsync();
										foreach (var dcFavs in docFavs)
										{
											dcFavs.IsDeleted = true;
											dcFavs.DeletedBy = UserId;
											dcFavs.DeletedAt = DateTime.Now;
										}

										context.DocumentFavorites.UpdateRange(docFavs);
										result += context.SaveChanges();

										var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == idDocument).ToListAsync();

										//string baseDir = AppDomain.CurrentDomain.BaseDirectory;
										//var categoryPathFolder = Path.Combine(baseDir, "upload", $"{categoryID}-{batch[j].CategoryName.Trim()}");
										//var uploadPath = Path.Combine(baseDir, categoryPathFolder);

										var uploadPath = Extensions.DocumentFilesHelper.GetPhysicalPathForCategory(UserId, UserName, categoryID, batch[j].CategoryName.Trim());

										foreach (var fl in docFiles)
										{
											var theFile = Path.Combine(uploadPath, fl.NewDocumentFileName);
											if (File.Exists(theFile))
											{
												File.Delete(theFile);
											}
											fl.IsDeleted = true;
											fl.DeletedBy = UserId;
											fl.DeletedAt = DateTime.Now;
										}

										context.DocumentFiles.UpdateRange(docFiles);
										result += context.SaveChanges();

										var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == idDocument).ToListAsync();
										foreach (var dcItemList in docItemList)
										{
											dcItemList.IsDeleted = true;
											dcItemList.DeletedBy = UserId;
											dcItemList.DeletedAt = DateTime.Now;
										}

										context.DocumentItemList.UpdateRange(docItemList);
										result += context.SaveChanges();

										var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == idDocument).ToListAsync();
										foreach (var dcRelate in docRelated)
										{
											dcRelate.IsDeleted = true;
											dcRelate.DeletedBy = UserId;
											dcRelate.DeletedAt = DateTime.Now;
										}

										context.RelatedDocuments.UpdateRange(docRelated);
										result += context.SaveChanges();


										var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == idDocument).ToListAsync();
										foreach (var dcRmdrs in docReminders)
										{
											dcRmdrs.IsDeleted = true;
											dcRmdrs.DeletedBy = UserId;
											dcRmdrs.DeletedAt = DateTime.Now;
										}

										context.DocumentReminders.UpdateRange(docReminders);
										result += context.SaveChanges();

										var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == idDocument);
										if (docShared != null)
										{
											var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
											foreach (var dcSharePRiv in sharePriv)
											{
												dcSharePRiv.IsDeleted = true;
												dcSharePRiv.DeletedBy = UserId;
												dcSharePRiv.DeletedAt = DateTime.Now;
											}

											context.DocumentSharedPrivillege.UpdateRange(sharePriv);
											result += context.SaveChanges();

											docShared.IsDeleted = true;
											docShared.DeletedBy = UserId;
											docShared.DeletedAt = DateTime.Now;
											context.DocumentShared.Update(docShared);
											result += context.SaveChanges();
										}

										var notifs = await context.Notifications.Where(x => x.DocumentID == idDocument).ToListAsync();
										if (notifs.Count > 0)
										{
											foreach (var t in notifs)
											{
												t.IsDeleted = true;
												t.DeletedBy = UserId;
												t.DeletedAt = DateTime.Now;
											}

											context.Notifications.UpdateRange(notifs);
											result += context.SaveChanges();
										}

										Documents docx = batchdocument[x];
										docx.IsDeleted = true;
										docx.DeletedBy = UserId;
										docx.DeletedAt = DateTime.Now;
										context.Documents.Update(docx);
										result += context.SaveChanges();
									}
								}


								Categories category = batchSize > 1 ? batch[j] : new Categories() { Id = categoryID };

								category.IsActive = false;
								category.IsDeleted = true;
								category.DeletedBy = UserId;
								category.DeletedAt = DateTime.Now;

								context.Categories.Update(category);
								await context.SaveChangesAsync();
							}

							transaction.Commit();
						}
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						throw;
					}
				}
			});

			return result > 0;
		}


		public async Task<bool> EmptyByPurging(int batchSize)
		{
			int result = 0;
			var recordsToDelete = await context.Categories
				.Where(x => x.IsActive == false && x.IsDeleted == true).ToListAsync();
			if (recordsToDelete.Count == 0)
			{
				return true;
			}

			for (int i = 0; i < recordsToDelete.Count; i += batchSize)
			{
				var batch = recordsToDelete.Skip(i).Take(batchSize).ToList();
				context.Categories.RemoveRange(batch);
				result += await context.SaveChangesAsync();
			}
			return result > 0;
		}

		public async Task<bool> MarkAsUnDeletedAsync(int categoryId)
		{
			int result = 0;
			var entity = await context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
			if (entity != null)
			{
				entity.IsActive = true;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Categories.Update(entity);
				result += await context.SaveChangesAsync();
			}
			return result > 0;
		}

		public async Task<int> DeleteAsync(int categoryId)
		{
			return await DeleteCategoryAndDocumentsAsync(categoryId);
		}

		//start here
		private async Task<int> DeleteCategoryAndDocumentsAsync(int categoryId)
		{
			int result = 0;
			using var transaction = context.Database.BeginTransaction();
			{
				try
				{
					var categoryToDelete = await context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId && x.IsActive != true);
					if (categoryToDelete == null)
					{
						return 1; // Category not found or already deleted
					}

					// Get all child categories
					var childCategories = await context.Categories.Where(x => x.ParentId == categoryId && x.IsActive != true).ToListAsync();
					foreach (var childCategory in childCategories)
					{
						// Recursively delete documents and child categories
						result += await DeleteCategoryAndDocumentsAsync(childCategory.Id);
					}

					// Now delete documents associated with the current category
					var documents = await context.Documents.Where(x => x.CategoryID == categoryToDelete.Id).ToListAsync();
					foreach (var doc in documents)
					{
						int documentId = doc.Id;

						// Delete document attributes, favorites, files, item lists, related documents, and reminders
						await DeleteDocumentRelatedEntitiesAsync(documentId);

						// Finally, remove the document itself
						context.Documents.Remove(doc);
						result += await context.SaveChangesAsync();
					}

					// Remove the category itself
					context.Categories.Remove(categoryToDelete);
					result += await context.SaveChangesAsync();

					transaction.Commit();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw;
				}
			}
			return result;
		}

		private async Task DeleteDocumentRelatedEntitiesAsync(int documentId)
		{
			var dateNow = DateTime.Now;

			// Document Logs
			var logs = await context.DocumentLog.Where(x => x.DocumentID == documentId).ToListAsync();
			foreach (var log in logs)
			{
				log.IsDeleted = true;
				log.DeletedBy = UserId;
				log.DeletedAt = dateNow;
			}
			await context.SaveChangesAsync();

			// Approvals and related activities
			var approval = await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == documentId && x.CategoryID != null);
			if (approval != null)
			{
				var activities = await context.ApprovalActivities.Where(x => x.ApprovalID == approval.Id).ToListAsync();
				foreach (var activity in activities)
				{
					activity.IsDeleted = true;
					activity.DeletedBy = UserId;
					activity.DeletedAt = dateNow;
				}
				await context.SaveChangesAsync();

				var flows = await context.ApprovalFlows.Where(x => x.ApprovalID == approval.Id).ToListAsync();
				foreach (var flow in flows)
				{
					flow.IsDeleted = true;
					flow.DeletedBy = UserId;
					flow.DeletedAt = dateNow;
				}
				await context.SaveChangesAsync();

				approval.IsDeleted = true;
				approval.DeletedBy = UserId;
				approval.DeletedAt = dateNow;
				await context.SaveChangesAsync();
			}

			// Document Attributes
			var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == documentId).ToListAsync();
			foreach (var attr in docAttr)
			{
				attr.IsDeleted = true;
				attr.DeletedBy = UserId;
				attr.DeletedAt = dateNow;
			}
			await context.SaveChangesAsync();

			// Document Favorites
			var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == documentId).ToListAsync();
			foreach (var fav in docFavs)
			{
				fav.IsDeleted = true;
				fav.DeletedBy = UserId;
				fav.DeletedAt = dateNow;
			}
			await context.SaveChangesAsync();

			// Document Files
			var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == documentId).ToListAsync();
			string folderOfTheFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "upload");
			foreach (var fl in docFiles)
			{
				var theFile = Path.Combine(folderOfTheFile, fl.NewDocumentFileName);
				if (File.Exists(theFile))
				{
					File.Delete(theFile);
				}

				fl.IsDeleted = true;
				fl.DeletedBy = UserId;
				fl.DeletedAt = dateNow;
			}
			await context.SaveChangesAsync();

			// Document Item List
			var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == documentId).ToListAsync();
			foreach (var item in docItemList)
			{
				item.IsDeleted = true;
				item.DeletedBy = UserId;
				item.DeletedAt = dateNow;
			}
			await context.SaveChangesAsync();

			// Related Documents
			var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == documentId).ToListAsync();
			foreach (var related in docRelated)
			{
				related.IsDeleted = true;
				related.DeletedBy = UserId;
				related.DeletedAt = dateNow;
			}
			await context.SaveChangesAsync();

			// Document Reminders
			var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == documentId).ToListAsync();
			foreach (var reminder in docReminders)
			{
				reminder.IsDeleted = true;
				reminder.DeletedBy = UserId;
				reminder.DeletedAt = dateNow;
			}
			await context.SaveChangesAsync();

			// Document Shared and Privileges
			var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == documentId);
			if (docShared != null)
			{
				var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
				foreach (var priv in sharePriv)
				{
					priv.IsDeleted = true;
					priv.DeletedBy = UserId;
					priv.DeletedAt = dateNow;
				}
				await context.SaveChangesAsync();

				docShared.IsDeleted = true;
				docShared.DeletedBy = UserId;
				docShared.DeletedAt = dateNow;
				await context.SaveChangesAsync();
			}
		}

		private async Task DeleteDocumentRelatedEntitiesAsyncBak(int documentId)
		{
			// Delete document logs
			var logs = await context.DocumentLog.Where(x => x.DocumentID == documentId).ToListAsync();
			context.DocumentLog.RemoveRange(logs);
			await context.SaveChangesAsync();

			// Delete approvals and related activities
			var approval = await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == documentId && x.CategoryID != null);
			if (approval != null)
			{
				var activities = await context.ApprovalActivities.Where(x => x.ApprovalID == approval.Id).ToListAsync();
				context.ApprovalActivities.RemoveRange(activities);
				await context.SaveChangesAsync();

				var flows = await context.ApprovalFlows.Where(x => x.ApprovalID == approval.Id).ToListAsync();
				context.ApprovalFlows.RemoveRange(flows);
				await context.SaveChangesAsync();

				context.Approvals.Remove(approval);
				await context.SaveChangesAsync();
			}

			var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == documentId).ToListAsync();
			context.DocumentAttributes.RemoveRange(docAttr);
			await context.SaveChangesAsync();

			var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == documentId).ToListAsync();
			context.DocumentFavorites.RemoveRange(docFavs);
			await context.SaveChangesAsync();

			var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == documentId).ToListAsync();
			string folderOfTheFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "upload");
			foreach (var fl in docFiles)
			{
				var theFile = Path.Combine(folderOfTheFile, fl.NewDocumentFileName);
				if (File.Exists(theFile))
				{
					File.Delete(theFile);
				}
			}
			context.DocumentFiles.RemoveRange(docFiles);
			await context.SaveChangesAsync();

			var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == documentId).ToListAsync();
			context.DocumentItemList.RemoveRange(docItemList);
			await context.SaveChangesAsync();

			var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == documentId).ToListAsync();
			context.RelatedDocuments.RemoveRange(docRelated);
			await context.SaveChangesAsync();

			var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == documentId).ToListAsync();
			context.DocumentReminders.RemoveRange(docReminders);
			await context.SaveChangesAsync();

			var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == documentId);
			if (docShared != null)
			{
				var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
				context.DocumentSharedPrivillege.RemoveRange(sharePriv);
				await context.SaveChangesAsync();

				context.DocumentShared.Remove(docShared);
				await context.SaveChangesAsync();
			}
		}

		public async Task<List<DropdownTextValue>> GetDropdownCategories(string categoryname)
		{
			var userGroupIds = await context.UserGroup
					.Where(ug => ug.UserId == UserId)
					.Select(ug => ug.GroupId)
					.ToListAsync();

			var sharedCat = context.CategoriesShared
				.Where(cs => (cs.UserID == UserId && cs.ShareType == "user") ||
							 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value)))
				.Select(x => x.CategoryID);

			var sharedCatFromDocId = context.DocumentSharedPrivillege
				.Where(x => (x.UserID == UserId && x.ShareType == "user") ||
							(x.GroupID != null && x.ShareType == "group" && userGroupIds.Contains(x.GroupID.Value)))
				.Select(x => x.DocumentShared.Document.CategoryID).Distinct();

			var categories = await context.Categories.Where(x => x.IsActive == true && (x.Owner == UserId || sharedCat.Contains(x.Id) || sharedCatFromDocId.Contains(x.Id)))
				.WhereIf(!string.IsNullOrEmpty(categoryname), x => x.CategoryName.ToLower().Contains(categoryname.ToLower())).Take(30)
				.Select(x => new DropdownTextValue { Text = x.Owner != UserId ? $"{x.CategoryName}-{x.OwnerInfo.UserName}" : x.CategoryName, Value = x.Id }).ToListAsync();

			return categories;
		}

		public async Task<int> PurgeDeletedCategories(int batchSize, int purgingInMonth)
		{
			int deletedCount = 0;
			var cutoff = DateTime.Today.AddMonths(-purgingInMonth);
			var strategy = context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				// deferred query with role filter
				var baseQuery = context.Categories
					.Where(c => c.IsDeleted == true && c.DeletedAt.HasValue && c.DeletedAt.Value <= cutoff)
					//.WhereIf(UserRoleInApp == "user", c => c.Owner == UserId)
					.OrderBy(c => c.Id);

				var total = await baseQuery.CountAsync();
				if (total == 0)
					return;

				var totalPages = (int)Math.Ceiling((double)total / batchSize);

				for (int page = 1; page <= totalPages; page++)
				{
					int skip = Skip(page, batchSize);
					var batch = await baseQuery.Skip(skip).Take(batchSize).ToListAsync();
					if (batch.Count == 0)
						continue;

					var categoryIds = batch.Select(c => c.Id).ToList();

					using var transaction = await context.Database.BeginTransactionAsync();
					try
					{
						// prefetch categories map for file path resolution
						var categoriesMap = await context.Categories
							.Where(c => categoryIds.Contains(c.Id))
							.ToDictionaryAsync(c => c.Id);

						// remove category-level approvals (and their activities/flows)
						var catApprovals = await context.Approvals
							.Where(a => a.CategoryID != null && categoryIds.Contains(a.CategoryID.Value) && a.DocumentID == null)
							.ToListAsync();

						if (catApprovals.Count > 0)
						{
							var approvalIds = catApprovals.Select(a => a.Id).ToList();
							var appActivities = await context.ApprovalActivities.Where(aa => approvalIds.Contains(aa.ApprovalID)).ToListAsync();
							if (appActivities.Count > 0) context.ApprovalActivities.RemoveRange(appActivities);

							var appFlows = await context.ApprovalFlows.Where(af => approvalIds.Contains(af.ApprovalID)).ToListAsync();
							if (appFlows.Count > 0) context.ApprovalFlows.RemoveRange(appFlows);

							context.Approvals.RemoveRange(catApprovals);
						}

						// remove category favorites and shared entries
						var catFavs = await context.CategoriesFavorites.Where(cf => categoryIds.Contains(cf.CategoryID)).ToListAsync();
						if (catFavs.Count > 0) context.CategoriesFavorites.RemoveRange(catFavs);

						var catShared = await context.CategoriesShared.Where(cs => cs.CategoryID != null && categoryIds.Contains(cs.CategoryID.Value)).ToListAsync();
						if (catShared.Count > 0) context.CategoriesShared.RemoveRange(catShared);

						// Handle documents inside these categories (permanently delete documents + related children)
						var docs = await context.Documents
							.Where(d => d.IsDeleted == true && d.DeletedAt.HasValue && d.DeletedAt.Value <= cutoff && categoryIds.Contains(d.CategoryID))
							.ToListAsync();

						foreach (var doc in docs)
						{
							var documentId = doc.Id;

							// Document logs
							var logs = await context.DocumentLog.Where(x => x.DocumentID == documentId).ToListAsync();
							if (logs.Count > 0) context.DocumentLog.RemoveRange(logs);

							// Approvals for this document and their activities/flows
							var docApprovals = await context.Approvals.Where(a => a.DocumentID == documentId).ToListAsync();
							if (docApprovals.Count > 0)
							{
								var approvalIds = docApprovals.Select(a => a.Id).ToList();
								var activities = await context.ApprovalActivities.Where(a => approvalIds.Contains(a.ApprovalID)).ToListAsync();
								if (activities.Count > 0) context.ApprovalActivities.RemoveRange(activities);

								var flows = await context.ApprovalFlows.Where(f => approvalIds.Contains(f.ApprovalID)).ToListAsync();
								if (flows.Count > 0) context.ApprovalFlows.RemoveRange(flows);

								context.Approvals.RemoveRange(docApprovals);
							}

							// Attributes, favorites
							var attrs = await context.DocumentAttributes.Where(a => a.DocumentID == documentId).ToListAsync();
							if (attrs.Count > 0) context.DocumentAttributes.RemoveRange(attrs);

							var favs = await context.DocumentFavorites.Where(f => f.DocumentId == documentId).ToListAsync();
							if (favs.Count > 0) context.DocumentFavorites.RemoveRange(favs);

							// Document files - attempt to delete physical files then delete DB rows
							var docFiles = await context.DocumentFiles.Where(f => f.DocumentID == documentId).ToListAsync();
							if (docFiles.Count > 0)
							{
								try
								{
									var category = categoriesMap.TryGetValue(doc.CategoryID, out var cat) ? cat : null;
									//string baseDir = AppDomain.CurrentDomain.BaseDirectory;
									//var categoryFolder = category != null ? $"{category.Id}-{category.CategoryName?.Trim() ?? "unknown"}" : "0-unknown";
									//var uploadPath2 = Path.Combine(baseDir, "upload", categoryFolder);
									var uploadPath = Extensions.DocumentFilesHelper.GetPhysicalPathForCategory(UserId, UserName, category.Id, category.CategoryName?.Trim());
									foreach (var f in docFiles)
									{
										if (!string.IsNullOrEmpty(f.NewDocumentFileName))
										{
											var physical = Path.Combine(uploadPath, f.NewDocumentFileName);
											try { if (File.Exists(physical)) File.Delete(physical); } catch { /* ignore */ }
										}
									}
								}
								catch
								{
									// ignore filesystem errors
								}

								context.DocumentFiles.RemoveRange(docFiles);
							}

							// item lists, related docs (both directions), reminders
							var items = await context.DocumentItemList.Where(x => x.DocumentID == documentId).ToListAsync();
							if (items.Count > 0) context.DocumentItemList.RemoveRange(items);

							var related = await context.RelatedDocuments.Where(x => x.DocumentID == documentId || x.RelatedDocumentID == documentId).ToListAsync();
							if (related.Count > 0) context.RelatedDocuments.RemoveRange(related);

							var reminders = await context.DocumentReminders.Where(x => x.DocumentID == documentId).ToListAsync();
							if (reminders.Count > 0) context.DocumentReminders.RemoveRange(reminders);

							// Document shared & privileges
							var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == documentId);
							if (docShared != null)
							{
								var sharePrivs = await context.DocumentSharedPrivillege.Where(p => p.DocumentSharedID == docShared.Id).ToListAsync();
								if (sharePrivs.Count > 0) context.DocumentSharedPrivillege.RemoveRange(sharePrivs);
								context.DocumentShared.Remove(docShared);
							}

							// Notifications for this document
							var notifs = await context.Notifications.Where(n => n.DocumentID == documentId).ToListAsync();
							if (notifs.Count > 0) context.Notifications.RemoveRange(notifs);

							// Finally remove document row
							context.Documents.Remove(doc);
						}

						// remove notifications associated directly to categories (if any reference exists)
						var catNotifs = await context.Notifications.Where(n => n.DocumentID == null && n.TargetActor == null).ToListAsync();
						if (catNotifs.Count > 0)
							context.Notifications.RemoveRange(catNotifs);

						// Finally remove categories
						context.Categories.RemoveRange(batch);

						// persist all removals for this batch
						await context.SaveChangesAsync();
						await transaction.CommitAsync();

						deletedCount += batch.Count;
					}
					catch
					{
						await transaction.RollbackAsync();
						throw;
					}
				}
			});

			return deletedCount;
		}
	}
}