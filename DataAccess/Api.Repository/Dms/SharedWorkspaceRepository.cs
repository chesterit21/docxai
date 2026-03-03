namespace Api.Repository.Dms
{
	using Api.DataAccess;
	using Api.DataAccess.Models.Dms;
	using Api.Domain.EntityResponses.Dms;
	using Api.Domain.Formatters;
	using Microsoft.AspNetCore.Http;
	using Microsoft.EntityFrameworkCore;
	using System.Linq;
	using System.Linq.Dynamic.Core;
	using System.Threading.Tasks;

	/// <summary>
	/// Defines the <see cref="ISharedWorkspaceRepository" />
	/// </summary>
	public interface ISharedWorkspaceRepository : IRepository<CategoriesShared>
	{
		//Task<List<ResponseSharedWorkspace>> ListMySharedFiles(int page = 0, int limit = 0, string orderBy = null, string orderOrientation = null, string filterBy = null, string filterValue = null);

		/// <summary>
		/// The ListMySharedFiles
		/// </summary>
		/// <param name="DocumentOrCategoryTitle">The DocumentOrCategoryTitle<see cref="string"/></param>
		/// <param name="DocumentOrCategoryDesc">The DocumentOrCategoryDesc<see cref="string"/></param>
		/// <param name="page">The page<see cref="int"/></param>
		/// <param name="limit">The limit<see cref="int"/></param>
		/// <returns>The <see cref="Task{ResponseSharedWorkspaceList}"/></returns>
		Task<ResponseSharedWorkspaceList> ListMySharedFiles(string DocumentOrCategoryTitle, string DocumentOrCategoryDesc, int page = 0, int limit = 0);

		/// <summary>
		/// The ListSharedToMe
		/// </summary>
		/// <param name="DocumentOrCategoryTitle">The DocumentOrCategoryTitle<see cref="string"/></param>
		/// <param name="DocumentOrCategoryDesc">The DocumentOrCategoryDesc<see cref="string"/></param>
		/// <param name="page">The page<see cref="int"/></param>
		/// <param name="limit">The limit<see cref="int"/></param>
		/// <returns>The <see cref="Task{ResponseSharedWorkspaceList}"/></returns>
		Task<ResponseSharedWorkspaceList> ListSharedToMe(string DocumentOrCategoryTitle, string DocumentOrCategoryDesc, int page = 0, int limit = 0);

		/// <summary>
		/// The RemoveCategoryShare
		/// </summary>
		/// <param name="categoryId">The categoryId<see cref="int"/></param>
		/// <returns>The <see cref="Task{int}"/></returns>
		Task<int> RemoveCategoryShare(int categoryId);

		/// <summary>
		/// The RemoveDocumentShare
		/// </summary>
		/// <param name="documentId">The documentId<see cref="int"/></param>
		/// <returns>The <see cref="Task{int}"/></returns>
		Task<int> RemoveDocumentShare(int documentId);

		/// <summary>
		/// The CheckIfDocumentSharedToLogedUser
		/// </summary>
		/// <param name="documentId">The documentId<see cref="int"/></param>
		/// <returns>The <see cref="Task{bool}"/></returns>
		Task<bool> CheckIfDocumentSharedToLogedUser(int documentId);
	}

	/// <summary>
	/// Defines the <see cref="SharedWorkspaceRepository" />
	/// </summary>
	public class SharedWorkspaceRepository(DataContext context, IHttpContextAccessor accessor) : Repository<CategoriesShared>(context, accessor), ISharedWorkspaceRepository
	{
		//public async Task<List<ResponseSharedWorkspace>> ListMySharedFiles(int page = 0, int limit = 0, string orderBy = null, string orderOrientation = null, string filterBy = null, string filterValue = null)

		/// <summary>
		/// The GetAllSubCategoryIds
		/// </summary>
		/// <param name="parentCategoryId">The parentCategoryId<see cref="int"/></param>
		/// <returns>The <see cref="Task{List{int}}"/></returns>
		private async Task<List<int>> GetAllSubCategoryIds(int parentCategoryId)
		{
			var allIds = new List<int> { parentCategoryId };
			var childCategories = await context.Categories
				.Where(c => c.ParentId == parentCategoryId && c.IsActive)
				.Select(c => c.Id)
				.ToListAsync();

			foreach (var childId in childCategories)
			{
				var subIds = await GetAllSubCategoryIds(childId);
				allIds.AddRange(subIds);
			}

			return allIds;
		}

		/// <summary>
		/// The ListMySharedFiles
		/// </summary>
		/// <param name="DocumentOrCategoryTitle">The DocumentOrCategoryTitle<see cref="string"/></param>
		/// <param name="DocumentOrCategoryDesc">The DocumentOrCategoryDesc<see cref="string"/></param>
		/// <param name="page">The page<see cref="int"/></param>
		/// <param name="limit">The limit<see cref="int"/></param>
		/// <returns>The <see cref="Task{ResponseSharedWorkspaceList}"/></returns>
		public async Task<ResponseSharedWorkspaceList> ListMySharedFiles(string DocumentOrCategoryTitle, string DocumentOrCategoryDesc, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);

			var categoryQueryBase = await context.Categories
			.Where(c => c.Owner == UserId && c.IsActive == true)
			.Where(c =>
				(string.IsNullOrEmpty(DocumentOrCategoryTitle) || (c.CategoryName != null && c.CategoryName.ToLower().Contains(DocumentOrCategoryTitle.ToLower()))) &&
				(string.IsNullOrEmpty(DocumentOrCategoryDesc) || (c.CategoryDesc != null && c.CategoryDesc.ToLower().Contains(DocumentOrCategoryDesc.ToLower())))
			)
			.GroupJoin(
				context.User,
				c => c.UpdatedBy,
				u => u.UserId,
				(c, userGroup) => new
				{
					Category = c,
					OwnerFullName = c.OwnerInfo.FullName,
					UpdatedByUser = userGroup.FirstOrDefault() // Left Join result is reliable here
				}
			)
			.Join(
				context.CategoriesShared,
				cat => cat.Category.Id,
				cs => cs.CategoryID,
				(cat, cs) => new { CategoryShared = cs, CategoryDetails = cat }
			)
			// 3. Group and Select the simplified structure for Union
			.GroupBy(j => j.CategoryDetails.Category.Id)
			.Select(g => new // **Simplified Anonymous Type for Union**
			{
				Id = g.Key,
				Name = g.First().CategoryDetails.Category.CategoryName,
				Description = g.First().CategoryDetails.Category.CategoryDesc,
				Type = "Category",
				LastUpdateName = g.First().CategoryDetails.UpdatedByUser == null ? "" : g.First().CategoryDetails.UpdatedByUser.FullName,
				LastUpdateDate = g.Max(x => x.CategoryShared.UpdatedAt),
				OwnerFullname = g.First().CategoryDetails.OwnerFullName ?? "",
				SharedWithUsersCount = g.Count()
			}).ToListAsync();

			var documentQuery = context.Documents
				.Include(d => d.OwnerInfo)
				.Where(d => d.Owner == UserId && d.IsActive == true);

			if (!string.IsNullOrEmpty(DocumentOrCategoryTitle))
			{
				documentQuery = documentQuery
					.Where(d => d.DocumentTitle.ToLower().Contains(DocumentOrCategoryTitle.ToLower()));
			}
			if (!string.IsNullOrEmpty(DocumentOrCategoryDesc))
			{
				documentQuery = documentQuery
					.Where(d => d.DocumentDesc.ToLower().Contains(DocumentOrCategoryDesc.ToLower()));
			}

			var documentQueryBase = await context.DocumentShared
			.Join(
				documentQuery,
				ds => ds.DocumentID,
				d => d.Id,
				(ds, d) => new
				{
					SharedInfo = ds,
					Document = d,
					OwnerFullName = d.OwnerInfo.FullName,
				}
			)
			.GroupBy(j => j.Document.Id)
			.Select(g => new
			{
				Id = g.Key,
				Name = g.First().Document.DocumentTitle,
				Description = g.First().Document.DocumentDesc,
				Type = "Document",
				LastUpdateName = g.First().SharedInfo.UpdatedByFullName,
				LastUpdateDate = g.Max(x => x.SharedInfo.UpdatedAt),
				OwnerFullname = g.First().OwnerFullName ?? "",
				SharedWithUsersCount = g.Count()
			}).ToListAsync();

			//ResponseSharedWorkspaceIntial
			//if(documentQueryBase.Count > 0)
			var combinedQuery = documentQueryBase.Union(categoryQueryBase);
			//var combinedQuery = documentQueryBase.Union(categoryQueryBase);

			// Get the total count of all combined items
			int totalItemCount = combinedQuery.Count();

			var paginatedQuery = combinedQuery
			.OrderBy(item => item.Name)
			.ThenByDescending(item => item.LastUpdateDate)
			.Skip(skip)
			.Take(limit);

			var bb = paginatedQuery
				.Select(item => new // Anonymous Type for execution
				{
					item.Id,
					item.Name,
					item.Description,
					item.Type,
					item.LastUpdateName,
					item.LastUpdateDate,
					item.OwnerFullname,
					item.SharedWithUsersCount,

					// RE-INTRODUCE the complex subqueries based on the item's Type
					TotalSizeBytes = item.Type == "Document"
						? (context.DocumentFiles.Where(f => f.DocumentID == item.Id).Sum(f => (long?)f.DocumentFileSize) ?? 0)
						: (context.Documents.Where(d => d.CategoryID == item.Id).Sum(d => (long?)d.FileSize) ?? 0),

					FileType = item.Type == "Document"
						? context.DocumentFiles
							.Where(f => f.DocumentID == item.Id && f.IsMainDocumentFile)
							.Select(f => f.DocumentType)
							.FirstOrDefault()
						: (string)null
				});

			var finalResult = bb.Select(item => new ResponseSharedWorkspace
			{
				DocumentId = item.Id,
				DocumentName = item.Name,
				DocumentDescription = item.Description,
				Type = item.Type,
				LastUpdateName = item.LastUpdateName,
				LastUpdateDate = item.LastUpdateDate,
				OwnerFullname = item.OwnerFullname,
				SharedWithUsersCount = item.SharedWithUsersCount,
				FileType = item.FileType,
				Size = SizeFormatter.SizeSuffix(item.TotalSizeBytes, 1)
			}).ToList();

			//		//if (!string.IsNullOrWhiteSpace(orderBy))
			//		//	query = query.OrderBy(x => $"x.{orderBy} {orderOrientation}");

			ResponseSharedWorkspaceList returnObj = new ResponseSharedWorkspaceList { TotalRecord = totalItemCount, Records = finalResult };

			return returnObj;
		}

		/// <summary>
		/// The ListSharedToMe
		/// </summary>
		/// <param name="DocumentOrCategoryTitle">The DocumentOrCategoryTitle<see cref="string"/></param>
		/// <param name="DocumentOrCategoryDesc">The DocumentOrCategoryDesc<see cref="string"/></param>
		/// <param name="page">The page<see cref="int"/></param>
		/// <param name="limit">The limit<see cref="int"/></param>
		/// <returns>The <see cref="Task{ResponseSharedWorkspaceList}"/></returns>
		public async Task<ResponseSharedWorkspaceList> ListSharedToMe(string DocumentOrCategoryTitle, string DocumentOrCategoryDesc, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);
			var userGroupIds = await context.UserGroup
					.Where(ug => ug.UserId == UserId)
					.Select(ug => ug.GroupId)
					.ToListAsync();

			var favorites = await context.DocumentFavorites
				.Where(x => x.Owner == UserId)
				.Select(x => x.DocumentId)
				.ToListAsync();

			var categoryQueryBase = await context.CategoriesShared.Include(g => g.Category).ThenInclude(f => f.OwnerInfo)
				.Where(cs => ((cs.UserID == UserId && cs.ShareType == "user") ||
						(cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value))))
				.GroupJoin(
					context.User,
					c => c.InsertedBy,
					u => u.UserId,
					(c, userGroup) => new
					{
						CategoryShared = c,
						OwnerFullName = c.Category.OwnerInfo.FullName,
						UpdatedByUser = userGroup.FirstOrDefault(),
						UpdatedAt = c.InsertedAt
					}
				)
				.Join(context.Categories.Where(c => c.IsActive == true),
					cs => cs.CategoryShared.CategoryID,
					cat => cat.Id,
					(cs, cat) => new { CategoryShared = cs, CategoryDetails = cat }
				).GroupBy(j => j.CategoryShared.CategoryShared.CategoryID)
				.Select(g => new
				{
					Id = g.Key.Value,
					Name = g.First().CategoryDetails.CategoryName,
					Description = g.First().CategoryDetails.CategoryDesc,
					Type = "Category",
					LastUpdateName = g.First().CategoryShared.UpdatedByUser == null ? "" : g.First().CategoryShared.UpdatedByUser.FullName,
					LastUpdateDate = g.Max(x => x.CategoryShared.UpdatedAt),
					OwnerFullname = g.First().CategoryShared.OwnerFullName ?? "",
					SharedWithUsersCount = g.Count(),
					IsView = g.First().CategoryShared.CategoryShared.IsView,
					IsEdit = g.First().CategoryShared.CategoryShared.IsEdit,
					IsDelete = g.First().CategoryShared.CategoryShared.IsDelete
				}).ToListAsync();

			//var docShares = await context.DocumentSharedPrivillege
			//		.Include(x => x.DocumentShared)
			//		.Where(x => (x.UserID == UserId && x.ShareType == "user") ||
			//					(x.GroupID != null && x.ShareType == "group" && userGroupIds.Contains(x.GroupID.Value)))
			//					.GroupBy(x => x.DocumentShared.DocumentID)
			//		.Select(g => g.First())
			//		.ToDictionaryAsync(x => x.DocumentShared.DocumentID);
			//var sharedDocIds = docShares.Keys.ToList();

			var sharedDocIdQuery = context.DocumentSharedPrivillege
				.Where(x => (x.UserID == UserId && x.ShareType == "user") ||
							(x.GroupID != null && x.ShareType == "group" && userGroupIds.Contains(x.GroupID.Value)))
				.Select(x => x.DocumentShared.DocumentID)
				.Distinct();

			var documentQuery = context.Documents
				.Include(d => d.OwnerInfo)
				.Where(d => d.IsActive == true && sharedDocIdQuery.Contains(d.Id));
			if (!string.IsNullOrEmpty(DocumentOrCategoryTitle))
			{
				documentQuery = documentQuery
					.Where(d => d.DocumentTitle.ToLower().Contains(DocumentOrCategoryTitle.ToLower()));
			}
			if (!string.IsNullOrEmpty(DocumentOrCategoryDesc))
			{
				documentQuery = documentQuery
					.Where(d => d.DocumentDesc.ToLower().Contains(DocumentOrCategoryDesc.ToLower()));
			}

			var documentQuerys = await context.DocumentShared.Where(j => j.IsActive != false && sharedDocIdQuery.Contains(j.DocumentID))
			.GroupJoin(
					context.User,
					c => c.InsertedBy,
					u => u.UserId,
					(c, userGroup) => new
					{
						DocumentShared = c,
						UpdateUser = userGroup.FirstOrDefault(),
						UpdatedAt = c.UpdatedAt
					}
				)
			.Join(
				documentQuery,
				ds => ds.DocumentShared.DocumentID,
				d => d.Id,
				(ds, d) => new
				{
					SharedInfo = ds,
					Document = d,
					OwnerFullName = d.OwnerInfo.FullName,
				}
			)
			.Join
			(
				context.DocumentSharedPrivillege,
				ds => ds.SharedInfo.DocumentShared.Id,
				dsp => dsp.DocumentSharedID,
				(ds, dsp) => new
				{
					SharedInfo = ds.SharedInfo,
					Document = ds.Document,
					OwnerFullName = ds.OwnerFullName,
					DocPrivilege = dsp
				}
			)
			.GroupBy(j => j.Document.Id)
			.Select(g => new
			{
				Id = g.Key,
				Name = g.First().Document.DocumentTitle,
				Description = g.First().Document.DocumentDesc,
				Type = "Document",
				LastUpdateName = g.First().SharedInfo.UpdateUser.FullName,
				LastUpdateDate = g.Max(x => x.SharedInfo.UpdatedAt),
				OwnerFullname = g.First().OwnerFullName ?? "",
				SharedWithUsersCount = g.Count(),
				IsView = g.First(x => x.DocPrivilege.UserID == UserId || (x.DocPrivilege.ShareType == "group" && userGroupIds.Contains(x.DocPrivilege.GroupID.Value))).DocPrivilege.IsView,
				IsEdit = g.First(x => x.DocPrivilege.UserID == UserId || (x.DocPrivilege.ShareType == "group" && userGroupIds.Contains(x.DocPrivilege.GroupID.Value))).DocPrivilege.IsEdit,
				IsDelete = g.First(x => x.DocPrivilege.UserID == UserId || (x.DocPrivilege.ShareType == "group" && userGroupIds.Contains(x.DocPrivilege.GroupID.Value))).DocPrivilege.IsDelete
			}).ToListAsync();

			var combinedQuery = documentQuerys.Union(categoryQueryBase);
			//var combinedQuery = documentQueryBase.Union(categoryQueryBase);

			// Get the total count of all combined items
			int totalItemCount = combinedQuery.Count();

			var paginatedQuery = combinedQuery
			.OrderBy(item => item.Name)
			.ThenByDescending(item => item.LastUpdateDate)
			.Skip(skip)
			.Take(limit);

			var bb = paginatedQuery
				.Select(item => new // Anonymous Type for execution
				{
					item.Id,
					item.Name,
					item.Description,
					item.Type,
					item.LastUpdateName,
					item.LastUpdateDate,
					item.OwnerFullname,
					item.SharedWithUsersCount,
					item.IsView,
					item.IsEdit,
					item.IsDelete,

					// RE-INTRODUCE the complex subqueries based on the item's Type
					TotalSizeBytes = item.Type == "Document"
						? (context.DocumentFiles.Where(f => f.DocumentID == item.Id).Sum(f => (long?)f.DocumentFileSize) ?? 0)
						: (context.Documents.Where(d => d.CategoryID == item.Id).Sum(d => (long?)d.FileSize) ?? 0),

					FileType = item.Type == "Document"
						? context.DocumentFiles
							.Where(f => f.DocumentID == item.Id && f.IsMainDocumentFile)
							.Select(f => f.DocumentType)
							.FirstOrDefault()
						: (string)null
				});

			var finalResult = bb.Select(item => new ResponseSharedWorkspace
			{
				DocumentId = item.Id,
				DocumentName = item.Name,
				DocumentDescription = item.Description,
				Type = item.Type,
				LastUpdateName = item.LastUpdateName,
				LastUpdateDate = item.LastUpdateDate,
				OwnerFullname = item.OwnerFullname,
				SharedWithUsersCount = item.SharedWithUsersCount,
				FileType = item.FileType,
				Size = SizeFormatter.SizeSuffix(item.TotalSizeBytes, 1),
				IsFavorite = favorites.Contains(item.Id),
				IsView = item.IsView,
				IsEdit = item.IsEdit,
				IsDelete = item.IsDelete,
			}).ToList();

			//		//if (!string.IsNullOrWhiteSpace(orderBy))
			//		//	query = query.OrderBy(x => $"x.{orderBy} {orderOrientation}");

			ResponseSharedWorkspaceList returnObj = new ResponseSharedWorkspaceList { TotalRecord = totalItemCount, Records = finalResult };

			return returnObj;
		}

		/// <summary>
		/// The GetTotalFileSizeForCategory
		/// </summary>
		/// <param name="categoryId">The categoryId<see cref="int"/></param>
		/// <returns>The <see cref="long"/></returns>
		public long GetTotalFileSizeForCategory(int categoryId)
		{
			return context.Categories
				.Where(c => c.Id == categoryId)               // Filter cty
				.SelectMany(c => c.Documents)                 // Flatten documents
				.Sum(d => (long)d.FileSize);                        // Sum all FileSize
		}

		/// <summary>
		/// The RemoveCategoryShare
		/// </summary>
		/// <param name="categoryId">The categoryId<see cref="int"/></param>
		/// <returns>The <see cref="Task{int}"/></returns>
		public async Task<int> RemoveCategoryShare(int categoryId)
		{
			var dv = await context.CategoriesShared.FirstOrDefaultAsync(x => x.CategoryID == categoryId && x.UserID == UserId);
			context.CategoriesShared.Remove(dv);
			int res = await context.SaveChangesAsync();
			return res;
		}

		/// <summary>
		/// The RemoveDocumentShare
		/// </summary>
		/// <param name="documentId">The documentId<see cref="int"/></param>
		/// <returns>The <see cref="Task{int}"/></returns>
		public async Task<int> RemoveDocumentShare(int documentId)
		{
			var SharedDocumentsToUserLogedIn = context.DocumentSharedPrivillege.Where(x => x.UserID == UserId)
				.Join(context.DocumentShared,
					sh => sh.DocumentSharedID,
					ds => ds.Id,
					(sh, ds) => new
					{
						DocumentSharedID = ds.Id,
						DocumentSharedPrivID = sh.Id,
						ds.DocumentID,
						sh.UserID
					}
				).ToList();

			var sharedPrivIDtoDelete = SharedDocumentsToUserLogedIn.FirstOrDefault(x => x.DocumentID == documentId);
			var dpv = await context.DocumentSharedPrivillege.Where(x => x.Id == sharedPrivIDtoDelete.DocumentSharedPrivID).FirstOrDefaultAsync();
			context.DocumentSharedPrivillege.Remove(dpv);

			//context.DocumentShared.Remove(dv);
			int res = await context.SaveChangesAsync();
			return res;
		}

		/// <summary>
		/// The CheckIfDocumentSharedToLogedUser
		/// </summary>
		/// <param name="documentId">The documentId<see cref="int"/></param>
		/// <returns>The <see cref="Task{bool}"/></returns>
		public async Task<bool> CheckIfDocumentSharedToLogedUser(int documentId)
		{
			var SharedDocumentsToUserLogedIn = context.DocumentSharedPrivillege.Where(x => x.UserID == UserId)
					.Join(context.DocumentShared,
						sh => sh.DocumentSharedID,
						ds => ds.Id,
						(sh, ds) => new
						{
							DocumentSharedID = ds.Id,
							DocumentSharedPrivID = sh.Id,
							ds.DocumentID,
							sh.UserID
						}
					).ToList();

			var sharedPrivIDtoDelete = SharedDocumentsToUserLogedIn.FirstOrDefault(x => x.DocumentID == documentId);
			return sharedPrivIDtoDelete != null;
		}
	}
}
