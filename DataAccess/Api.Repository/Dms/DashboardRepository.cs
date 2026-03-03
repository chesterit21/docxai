using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Enum;
using Api.Domain.Formatters;
using Api.Extensions;
using Azure;
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MimeKit.Tnef;
using Npgsql.EntityFrameworkCore.PostgreSQL.Extensions;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text.Json;
using static NPOI.HSSF.Util.HSSFColor;

namespace Api.Repository
{
	public interface IDashboardRepository
	{
		Task<ResponseDashboardCounter> GetDashboardCounterAsync(int? UserId = null);
		Task<List<ResponseDashboardRecentActivity>> GetRecentActivities(int? documentId, int? UserId, DateTime? startDate = null, DateTime? endDate = null, int page = 0, int limit = 0);
		Task<List<ResponseDashboardTop10UserMostDownloads>> GetTop10UserMostDownload(DateTime? startDate = null, DateTime? endDate = null);
		Task<List<ResponseDashboardTop10UserMostStorage>> ResponseDashboardTop10UserMostStorages(DateTime? startDate = null, DateTime? endDate = null);
		Task<List<ResponseDocumentMonthlyGrowth>> GetDocumentGrowthByMonth(int year, int userId);

		Task<List<ResponseDocumenCountByCategory>> GetDocumentCountByCategory(int? userId, DateTime? startDate = null, DateTime? endDate = null);
		Task<List<ResponseDocumenCountByApprovalStatus>> GetDocumentCountByApprovalStatus(int userId, DateTime? startDate = null, DateTime? endDate = null);
		Task<List<ResponseDocumentExpiringCount>> GetExpiringDocumentCount(int userId, ExpirationPeriod period, DateTime? startDate = null, DateTime? endDate = null);
		Task<List<ResponseMostActiveUsers>> GetMostActiveUsersAsync(DateTime startDate, DateTime endDate, int topN = 10);
		Task<List<ResponseMostDownloadDocument>> GetMostDownloadedDocumentsAsync(DateTime startDate, DateTime endDate, int topN = 10);
		Task<List<ResponseMostViewedDocument>> GetMostViewedDocumentsAsync(DateTime startDate, DateTime endDate, int topN = 10);
	}

	public class DashboardRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentFiles>(context, accessor), IDashboardRepository
	{
		public async Task<ResponseDashboardCounter> GetDashboardCounterAsync(int? pUserId = null)
		{
			DateTime today = DateTime.Today.Date;

			ResponseDashboardCounter ct = new ResponseDashboardCounter();


			if (UserRoleInApp == "superadmin" && pUserId == null)
			{
				ct.TotalDocuments = await context.Documents.Where(d => d.IsActive == true).CountAsync();
				ct.TotalSizeDocuments = SizeFormatter.SizeSuffix(await context.Documents.Where(d => d.IsActive == true).SumAsync(x => x.FileSize.Value), 2);
				ct.ExpiringDocuments = await context.Documents.Where(x => x.IsActive == true && x.ExpiryDate != null && x.ReminderDays != null
					&& (x.ExpiryDate.Value.Date - today).Days <= x.ReminderDays.Value && x.Owner == UserId).CountAsync();
				ct.ExpiredDocuments = await context.Documents.Where(x => x.IsActive == true && x.ExpiryDate.Value.Date < today).CountAsync();
				ct.PendingApprovalDocuments = await context.Approvals.Where(d => d.IsActive == true && d.Status == ApprovalStatusEnum.Pending).Include(e => e.Documents).CountAsync();
				ct.RejectedDocuments = await context.Approvals.Where(d => d.IsActive == true && d.Status == ApprovalStatusEnum.Rejected).Include(e => e.Documents).CountAsync();
				ct.ApprovedDocuments = await context.Approvals.Where(d => d.IsActive == true && d.Status == ApprovalStatusEnum.Approved).Include(e => e.Documents).CountAsync();
			}
			else
			{
				if (pUserId == null)
					pUserId = UserId;

				if (pUserId.HasValue)
				{
					ct.TotalDocuments = await context.Documents.Where(d => d.IsActive == true && d.Owner == pUserId).CountAsync();
					//ct.TotalSizeDocuments = await context.Documents.Where(d => d.Owner == pUserId).SumAsync(x => x.FileSize.Value);
					ct.TotalSizeDocuments = SizeFormatter.SizeSuffix(await context.Documents.Where(d => d.Owner == pUserId).SumAsync(x => x.FileSize.Value), 2);
					ct.ExpiringDocuments = await context.Documents.Where(x => x.ExpiryDate != null && x.ReminderDays != null
						&& (x.ExpiryDate.Value.Date - today).Days <= x.ReminderDays.Value && x.Owner == pUserId).CountAsync();
					ct.ExpiredDocuments = await context.Documents.Where(x => x.ExpiryDate.Value.Date < today && x.Owner == pUserId).CountAsync();
					ct.PendingApprovalDocuments = await context.Approvals.Where(d => d.Status == ApprovalStatusEnum.Pending).Include(e => e.Documents).Where(e => e.Documents.Owner == pUserId).CountAsync();
					ct.RejectedDocuments = await context.Approvals.Where(d => d.Status == ApprovalStatusEnum.Rejected).Include(e => e.Documents).Where(e => e.Documents.Owner == pUserId).CountAsync();
					ct.ApprovedDocuments = await context.Approvals.Where(d => d.Status == ApprovalStatusEnum.Approved).Include(e => e.Documents).Where(e => e.Documents.Owner == pUserId).CountAsync();
				}
			}

				ct.PendingMyApproval = await context.Approvals.Where(d => d.IsActive == true && d.Status == ApprovalStatusEnum.Pending && d.NextApproverUserID == UserId)
					.Join(context.ApprovalFlows,
						a => a.Id,
						af => af.ApprovalID,
						(a, af) => new { a, af })
					//.Where(x => x.af.ApproverUserID == UserId)
					.CountAsync();

			return ct;
		}
		public async Task<List<ResponseDashboardRecentActivity>> GetRecentActivities(int? documentId, int? pUserId, DateTime? startDate = null, DateTime? endDate = null, int page = 0, int limit = 0)
		{
			int _UserId = pUserId ?? UserId;
			var query = context.DocumentLog.Include(d => d.Documents).Where(x =>
					x.Documents.Owner == _UserId && x.IsActive == true)
				.Join(context.User,
					dl => dl.InsertedBy,
					u => u.UserId,
					(dl, u) => new { DocumentLog = dl, User = u });

			// Apply filters based on user role
			if (UserRoleInApp == "superadmin" && pUserId == null)
			{
				// Superadmin can see all activities with optional filters
				query = query.WhereIf(documentId.HasValue, x => x.DocumentLog.DocumentID == documentId).WhereIf(pUserId.HasValue, x => x.DocumentLog.Documents.Owner == pUserId);

				if (startDate.HasValue && endDate.HasValue)
				{
					query = query.Where(x => x.DocumentLog.InsertedAt >= startDate && x.DocumentLog.InsertedAt <= endDate);
				}
			}
			else
			{
				// Regular users can only see their own documents' activities for the last 10 days
				var tenDaysAgo = DateTime.Today.AddDays(-10);

				query = query.Where(x => x.DocumentLog.InsertedAt >= tenDaysAgo);

				if (documentId.HasValue)
				{
					query = query.Where(x => x.DocumentLog.DocumentID == documentId);
				}
			}

			// Final selection and ordering
			var result = query
				.OrderByDescending(x => x.DocumentLog.InsertedAt)
				.Select(x => new ResponseDashboardRecentActivity
				{
					Id = x.DocumentLog.Id,
					DocumentID = x.DocumentLog.DocumentID,
					DocumentTitle = x.DocumentLog.Documents.DocumentTitle,
					Activity = x.DocumentLog.ActionLogDocument,
					ActivityBy = x.User.FullName,
					ActivityDate = x.DocumentLog.InsertedAt
				})
				.Take(100); // Limit the maximum number of records
							//.ToListAsync();

			int skip = Skip(page, limit);
			if (page > 0 && limit > 0)
			{
				return await result.Skip(skip).Take(limit).ToListAsync();
			}
			else
			{
				return await result.ToListAsync();
			}
		}
		public async Task<List<ResponseDashboardTop10UserMostDownloads>> GetTop10UserMostDownload(DateTime? startDate = null, DateTime? endDate = null)
		{
			var query = context.DocumentLog
				.Join(context.User,
					dl => dl.InsertedBy,
					u => u.UserId,
					(dl, u) => new { DocumentLog = dl, User = u })
				.Where(x => x.DocumentLog.ActionLogDocument == "Download");
			if (startDate.HasValue && endDate.HasValue)
			{
				query = query.Where(x => x.DocumentLog.InsertedAt >= startDate && x.DocumentLog.InsertedAt <= endDate);
			}
			else
			{
				var ThirteenDaysAgo = DateTime.Today.AddDays(-30);
				query = query.Where(x => x.DocumentLog.InsertedAt >= ThirteenDaysAgo);
			}
			var result = await query
				.GroupBy(x => new { x.User.UserId, x.User.FullName })
				.Select(g => new ResponseDashboardTop10UserMostDownloads
				{
					UserID = g.Key.UserId,
					UserFullName = g.Key.FullName,
					DownloadCount = g.Count()
				})
				.OrderByDescending(x => x.DownloadCount)
				.Take(10)
				.ToListAsync();
			return result;
		}
		public async Task<List<ResponseDashboardTop10UserMostStorage>> ResponseDashboardTop10UserMostStorages(DateTime? startDate = null, DateTime? endDate = null)
		{

			var query = context.Documents
		.Join(context.User,
			d => d.Owner,
			u => u.UserId,
			(d, u) => new { Document = d, User = u });

			if (startDate.HasValue && endDate.HasValue)
			{
				query = query.Where(x => x.Document.InsertedAt >= startDate && x.Document.InsertedAt <= endDate);
			}

			var intermediateResult = await query
				.GroupBy(x => new { x.User.UserId, x.User.FullName })
				.Select(g => new
				{
					UserID = g.Key.UserId,
					UserName = g.Key.FullName,
					TotalSize = g.Sum(x => x.Document.FileSize ?? 0)
				})
				.OrderByDescending(x => x.TotalSize)
				.Take(10)
				.ToListAsync();

			// Now format the size after getting data from database
			return intermediateResult.Select(x => new ResponseDashboardTop10UserMostStorage
			{
				UserID = x.UserID,
				UserName = x.UserName,
				TotalSizeNum = x.TotalSize,
				TotalSize = SizeFormatter.SizeSuffix(x.TotalSize, 2)
			})
			.ToList();

			//var query = context.Documents
			//	.Join(context.User,
			//		d => d.Owner,
			//		u => u.UserId,
			//		(d, u) => new { Document = d, User = u });
			//if (startDate.HasValue && endDate.HasValue)
			//{
			//	query = query.Where(x => x.Document.InsertedAt >= startDate && x.Document.InsertedAt <= endDate);
			//}
			//var result = await query
			//	.GroupBy(x => new { x.User.UserId, x.User.FullName })
			//	.Select(g => new ResponseDashboardTop10UserMostStorage
			//	{
			//		UserID = g.Key.UserId,
			//		UserName = g.Key.FullName,
			//		TotalSizeNum = g.Sum(x => x.Document.FileSize ?? 0)
			//		TotalSize = SizeFormatter.SizeSuffix(g.Sum(x => x.Document.FileSize ?? 0), 2)//SizeFormatter.SizeSuffix(TotalSizeNum, 2)
			//	})
			//	.OrderByDescending(x => x.TotalSize)
			//	.Take(10).ToListAsync();

			//return result;
		}

		public async Task<List<ResponseDocumentMonthlyGrowth>> GetDocumentGrowthByMonth(int year, int userId)
		{
			var dbData = context.Documents
			.Where(d => d.InsertedAt.Year == year && !d.IsDeleted)
			.GroupBy(d => d.InsertedAt.Month)
			.Select(g => new
			{
				Month = g.Key,
				Count = g.Count()
			})
			.ToList();

			var allMonths = Enumerable.Range(1, 12);
			var result = allMonths.GroupJoin(
				dbData,
				month => month,       // Outer Key (1-12)
				data => data.Month,   // Inner Key (DB Month)
				(month, dataCollection) => new ResponseDocumentMonthlyGrowth
				{
					MonthNumber = month,
					// Get Culture-specific month name (e.g., "January", "Januari")
					MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
					TotalDocuments = dataCollection.FirstOrDefault()?.Count ?? 0
				}
			).ToList();

			return result;
		}

		public async Task<List<ResponseDocumenCountByCategory>> GetDocumentCountByCategory(int? userId, DateTime? startDate = null, DateTime? endDate = null)
		{
				var query = context.Documents
			.Where(d => (UserRoleInApp == "superadmin" && userId == null) ? true : d.Owner == userId && !d.IsDeleted);


			if (startDate.HasValue && endDate.HasValue)
			{
				query = query.Where(x => x.InsertedAt >= startDate && x.InsertedAt <= endDate);
			}

			//Group By: CategoryID and CategoryName (to get the name in the output)
			var result = query.GroupBy(d => new { d.CategoryID, d.Categories.CategoryName })
			.Select(g => new ResponseDocumenCountByCategory
			{
				CategoryId = g.Key.CategoryID,
				CategoryName = g.Key.CategoryName,
				DocumentCount = g.Count()
			})
			// 4. Order: Highest count first
			.OrderByDescending(x => x.DocumentCount)
			.ToList();

			return result;
		}

		public async Task<List<ResponseDocumenCountByApprovalStatus>> GetDocumentCountByApprovalStatus(int userId, DateTime? startDate = null, DateTime? endDate = null)
		{
			var query = context.Approvals
			.Where(a => a.Documents.InsertedBy == userId // Filter by Document Creator
						&& !a.IsDeleted                  // Approval not deleted
						&& !a.Documents.IsDeleted);       // Document not deleted
			if (startDate.HasValue && endDate.HasValue)
			{
				query = query.Where(x => x.InsertedAt >= startDate && x.InsertedAt <= endDate);
			}
			
			var dbCounts = query.GroupBy(a => a.Status)
			.Select(g => new
			{
				Status = g.Key,
				Count = g.Count()
			})
			.ToList();

			// This ensures your UI gets a complete list (Draft, Pending, Approved, etc.)
			var allStatuses = Enum.GetValues(typeof(Domain.Enum.ApprovalStatusEnum))
								  .Cast<Domain.Enum.ApprovalStatusEnum>();

			// Step 3: Join the full Enum list with the DB results
			var result = allStatuses.GroupJoin(
				dbCounts,
				status => status,      // Outer Key (Enum)
				db => db.Status,       // Inner Key (DB Result)
				(status, matches) => new ResponseDocumenCountByApprovalStatus
				{
					Status = status,
					StatusName = status.ToString(), // e.g., "Approved"
					DocumentCount = matches.FirstOrDefault()?.Count ?? 0
				}
			).ToList();

			return result;
		}


		public async Task<List<ResponseDocumentExpiringCount>> GetExpiringDocumentCount(int userId, ExpirationPeriod period, DateTime? startDate = null, DateTime? endDate = null)
		{
			// only care about documents that are not deleted and have an ExpiryDate set
			var baseQuery = context.Documents
				.Where(d => (UserRoleInApp == "superadmin") ? true : d.Owner == userId && d.ExpiryDate.HasValue 
							&& !d.IsDeleted);
			if (startDate.HasValue && endDate.HasValue)
			{
				baseQuery = baseQuery.Where(x => x.ExpiryDate >= startDate && x.ExpiryDate <= endDate);
			}
			
			IQueryable<ResponseDocumentExpiringCount> resultQuery;
			#region oldprediod
			//switch (period)
			//{
			//	case ExpirationPeriod.Year:
			//		resultQuery = baseQuery
			//			.GroupBy(d => d.ExpiryDate.Value.Year)
			//			.Select(g => new ResponseDocumentExpiringCount
			//			{
			//				GroupKey = g.Key,
			//				GroupLabel = g.Key.ToString(),
			//				ExpiringDocumentCount = g.Count()
			//			});
			//			//.OrderBy(x => x.GroupKey);
			//		break;

			//	case ExpirationPeriod.Month:
			//		resultQuery = baseQuery
			//			// Group by both Year and Month to prevent mixing months from different years
			//			.GroupBy(d => new { d.ExpiryDate.Value.Year, Month = d.ExpiryDate.Value.Month })
			//			.Select(g => new ResponseDocumentExpiringCount
			//			{
			//				GroupKey = g.Key.Month,
			//				GroupLabel = $"{g.Key.Year}-{g.Key.Month:D2}", // e.g., "2024-11"
			//				ExpiringDocumentCount = g.Count()
			//			});
			//			//.OrderBy(x => x.GroupLabel);
			//		break;

			//	case ExpirationPeriod.Week:

			//		var serverAggregatedData = baseQuery
			//			.GroupBy(d => new {
			//				d.ExpiryDate.Value.Year,
			//				Week = PgSqlDbFunctions.DatePart("week", d.InsertedAt)
			//			})
			//			.Select(g => new
			//			{
			//				WeekNumber = (int)g.Key.Week,
			//				Year = g.Key.Year,
			//				Count = g.Count()
			//			});

			//		//GroupKey = (int)g.Key.Week,
			//		//GroupLabel = $"W{g.Key.Week:D2}-{g.Key.Year}", // e.g., "W45-2024"
			//		//ExpiringDocumentCount = g.Count()

			//		resultQuery = serverAggregatedData
			//		.OrderBy(x => x.Year) // Order by year first
			//		.ThenBy(x => x.WeekNumber) // Then by week number
			//		.Select(x => new ResponseDocumentExpiringCount
			//		{
			//			GroupKey = x.WeekNumber,
			//			//GroupLabel = $"W{x.WeekNumber:D2}-{x.Year}",
			//			GroupLabel = $"Week {x.WeekNumber}-{x.Year}", // e.g., "W45-2024"
			//			ExpiringDocumentCount = x.Count
			//		});
			//		break;

			//	default:
			//		throw new ArgumentException("Invalid expiration period specified.");
			//}
			#endregion

			int requestyear = 2020;
			switch (period)
			{
				case ExpirationPeriod.ThisYear:
					requestyear = DateTime.Today.Year;
					break;

				case ExpirationPeriod.Lastear:
					requestyear = DateTime.Today.Year - 1;
					break;

				case ExpirationPeriod.NextYear:
					requestyear = DateTime.Today.Year + 1;					
					break;

				default:
					throw new ArgumentException("Invalid expiration period specified.");
			}

			resultQuery = baseQuery.Where(x => x.ExpiryDate.Value.Year == requestyear)
				.GroupBy(d => new { d.ExpiryDate.Value.Year, Month = d.ExpiryDate.Value.Month })
						.Select(g => new ResponseDocumentExpiringCount
						{
							GroupKey = g.Key.Month,
							GroupLabel = $"{g.Key.Year}-{g.Key.Month:D2}", // e.g., "2024-11"
							ExpiringDocumentCount = g.Count()
						});

			var allMonths = Enumerable.Range(1, 12);
			var result = allMonths.GroupJoin(
				resultQuery,
				month => month,       // Outer Key (1-12)
				data => data.GroupKey,   // Inner Key (DB Month)
				(month, dataCollection) => new ResponseDocumentExpiringCount
				{
					GroupKey = month,
					GroupLabel = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
					ExpiringDocumentCount = dataCollection.FirstOrDefault()?.ExpiringDocumentCount ?? 0
				}
			).ToList();

			
			//.OrderBy(x => x.GroupKey);
			return result;
		}

		public async Task<List<ResponseMostActiveUsers>> GetMostActiveUsersAsync(DateTime startDate, DateTime endDate, int topN = 10)
		{
			// --- Step 1: Query Login Activity Counts ---
			// We group by ActorUserId to see how many times they accessed the system
			var loginStats = await context.LoginActivityLogs
				.Where(x => x.LoginTime >= startDate && x.LoginTime <= endDate)
				.GroupBy(x => x.ActorUserId)
				.Select(g => new { UserId = g.Key, Count = g.Count() })
				.ToListAsync();

			//// --- Step 2: Query Document Creation Counts ---
			//var docStats = await context.Documents
			//	.Where(d => d.InsertedAt >= startDate && d.InsertedAt <= endDate && !d.IsDeleted)
			//	.GroupBy(d => d.InsertedBy) // Or d.Owner
			//	.Select(g => new { UserId = g.Key, Count = g.Count() })
			//	.ToListAsync();

			//// --- Step 3: Query Approval/Workflow Action Counts ---
			//// Assuming you use 'ApprovalActivities' to track history. 
			//// If you only have 'Approvals', use that table instead.
			//var approvalStats = await context.ApprovalActivities
			//	.Where(a => a.InsertedAt >= startDate && a.InsertedAt <= endDate)
			//	.GroupBy(a => a.InsertedBy)
			//	.Select(g => new { UserId = g.Key, Count = g.Count() })
			//	.ToListAsync();

			// --- Step 4: Combine Data in Memory ---

			// A. Get all unique User IDs appearing in ANY of the three lists
			var allUserIds = loginStats.Select(x => x.UserId)
				//.Union(docStats.Select(x => x.UserId))
				//.Union(approvalStats.Select(x => x.UserId))
				.Distinct()
				.ToList();

			// B. Fetch User Names (Optional: optimize by fetching only these IDs)
			var users = await context.User
				.Where(u => allUserIds.Contains(u.UserId))
				.ToDictionaryAsync(u => u.UserId, u => u.UserName); // Create a Lookup Dictionary

			// C. Calculate Scores
			var leaderboard = allUserIds.Select(userId =>
			{
				// Lookup counts (default to 0 if not found)
				var logins = loginStats.FirstOrDefault(x => x.UserId == userId)?.Count ?? 0;
				//var docs = docStats.FirstOrDefault(x => x.UserId == userId)?.Count ?? 0;
				//var approvals = approvalStats.FirstOrDefault(x => x.UserId == userId)?.Count ?? 0;

				return new ResponseMostActiveUsers
				{
					UserId = userId,
					// Safe name lookup
					UserName = users.ContainsKey(userId) ? users[userId] : $"User #{userId}",

					LoginCount = logins,
					//DocumentsCreated = docs,
					//ApprovalsActioned = approvals,

					// CALCULATE THE WEIGHTED SCORE
					//TotalActivityScore =
					//	(logins * ActivityWeights.Login) +
					//	(approvals * ActivityWeights.ApprovalAction) +
					//	(docs * ActivityWeights.DocumentCreate)
				};
			})
			.OrderByDescending(x => x.LoginCount) // Highest score first
			.Take(topN)
			.ToList();

			return leaderboard;
		}

		public async Task<List<ResponseMostDownloadDocument>> GetMostDownloadedDocumentsAsync(DateTime startDate, DateTime endDate, int topN = 10)
		{
			const string DownloadAction = "Download File";

			var result = await context.DocumentLog
				.Where(log => log.InsertedAt >= startDate
							  && log.InsertedAt <= endDate
							  && log.ActionLogDocument == DownloadAction
							  && !log.IsDeleted)

				// 2. Group By: Document ID and Title (to project them in the result)
				// Accessing log.Documents (Navigation Property) allows us to grab the Title/Category
				.GroupBy(log => new
				{
					log.DocumentID,
					log.Documents.DocumentTitle,
					log.Documents.Categories.CategoryName
				})

				// 3. Project: Create the DTO with the Count
				.Select(g => new ResponseMostDownloadDocument
				{
					DocumentId = g.Key.DocumentID,
					DocumentTitle = g.Key.DocumentTitle,
					CategoryName = g.Key.CategoryName, // Optional: helpful context
					DownloadCount = g.Count()
				})

				// 4. Order & Limit
				.OrderByDescending(x => x.DownloadCount)
				.Take(topN)
				.ToListAsync();

			return result;
		}

		public async Task<List<ResponseMostViewedDocument>> GetMostViewedDocumentsAsync(DateTime startDate, DateTime endDate, int topN = 10)
		{
			const string ViewDetailDoc = "View Detail Document"; // LogDocumentAction.ViewDocument;

			var result = await context.DocumentLog
				.Where(log => log.InsertedAt >= startDate
							  && log.InsertedAt <= endDate
							  && log.ActionLogDocument == ViewDetailDoc
							  && !log.IsDeleted)

				// 2. Group By: Document ID and Title (to project them in the result)
				// Accessing log.Documents (Navigation Property) allows us to grab the Title/Category
				.GroupBy(log => new
				{
					log.DocumentID,
					log.Documents.DocumentTitle,
					log.Documents.Categories.CategoryName
				})

				// 3. Project: Create the DTO with the Count
				.Select(g => new ResponseMostViewedDocument
				{
					DocumentId = g.Key.DocumentID,
					DocumentTitle = g.Key.DocumentTitle,
					CategoryName = g.Key.CategoryName, // Optional: helpful context
					ViewCount = g.Count()
				})

				// 4. Order & Limit
				.OrderByDescending(x => x.ViewCount)
				.Take(topN)
				.ToListAsync();

			return result;
		}
	}
}
