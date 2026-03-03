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
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NPOI.HPSF;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text.Json;
using static Api.Domain.EntityResponses.Dms.ResponseDocumentItemlist;
using RDocumentSharedPrivillege = Api.Domain.EntityResponses.Dms.ResponseDocumentItemlist.RDocumentSharedPrivillege;
//using Microsoft.EntityFrameworkCore.Internal;
//using Microsoft.EntityFrameworkCore.Storage;
//using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
	public interface IDocumentsItemlistRepository : IRepository<DocumentItemList>
	{
		Task<int> AddItemList(List<int> documentIds);
		Task<int> DeleteItemList(List<int> documentIds);
		Task<ResponseDocumentItemlist> ItemList(int page = 0, int limit = 0);
		Task<int> EmptyItemList();
	}

	public class DocumentsItemlistRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentItemList>(context, accessor), IDocumentsItemlistRepository
	{
		public async Task<int> AddItemList(List<int> documentIds)
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						List<DocumentItemList> newrList = new List<DocumentItemList>();
						DocumentItemList newr = null;
						foreach (var item in documentIds)
						{
							DocumentItemList existitem = await context.DocumentItemList.FirstOrDefaultAsync(d => d.DocumentID == item && d.ItemUser == UserId);
							if (existitem == null)
							{
								newr = new DocumentItemList() { DocumentID = item, InsertedAt = DateTime.Now, InsertedBy = UserId, ItemUser = UserId };
								newrList.Add(newr);
							}
						}

						if (newrList.Count > 0)
						{
							context.DocumentItemList.AddRange(newrList);
							result += await context.SaveChangesAsync();
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

		public async Task<int> DeleteItemList(List<int> documentIds)
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						List<DocumentItemList> newrList = new List<DocumentItemList>();
						DocumentItemList newr = null;
						foreach (var item in documentIds)
						{
							DocumentItemList existitem = await context.DocumentItemList.FirstOrDefaultAsync(d => d.DocumentID == item && d.ItemUser == UserId);
							if (existitem != null)
							{
								context.DocumentItemList.Remove(existitem);
								context.SaveChanges();
							}
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

		public async Task<ResponseDocumentItemlist> ItemList(int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);

			var query = context.DocumentItemList.Include(d => d.Document).ThenInclude(du => du.OwnerInfo).Include(x => x.User).Where(x => x.ItemUser == UserId)
				.LeftJoin(context.User,
					"Document.UpdatedBy",
					"UserId",
					(left, userUpdate) => new
					{
						A = left,
						UpdatedByUserName = userUpdate.UserName,
						UpdatedByFullName = userUpdate.FullName
					})
					.LeftJoin(context.DocumentShared.Include(p => p.DocumentSharePrivs),
						document => document.A.DocumentID,
						shared => shared.DocumentID,
						(document, shared) => new
						{
							B = document,
							SH = shared,
							DCPriv = shared.DocumentSharePrivs
						})
					.LeftJoin(context.DocumentFiles,
						di => new { di.B.A.DocumentID, IsMainDocumentFile = true },
						df => new { df.DocumentID, df.IsMainDocumentFile },
						(di, df) => new
						{
							C = di,
							DF = df
						})
					.Select(x => new RDocumentItemList()
					{
						ItemListId = x.C.B.A.Id,
						DocumentId = x.C.B.A.DocumentID,
						DocumentName = x.C.B.A.Document.DocumentTitle,
						Description = x.C.B.A.Document.DocumentDesc,
						Owner = x.C.B.A.Document.OwnerInfo.UserId,
						OwnerName = x.C.B.A.Document.OwnerInfo.UserName,
						LastUpdateDate = x.C.B.A.Document.UpdatedAt,
						FilesSize = x.C.B.A.Document.FileSize != null ? SizeFormatter.SizeSuffix((long)x.C.B.A.Document.FileSize, 2) : null,
						FileType = x.DF.DocumentType,
						InsertedAt = x.C.B.A.Document.InsertedAt,
						UpdatedAt = x.C.B.A.Document.UpdatedAt,
						UpdatedBy = x.C.B.A.Document.UpdatedBy,
						UpdatedByUserName = x.C.B.UpdatedByUserName,
						UpdatedByFullName = x.C.B.UpdatedByFullName,
						IsFavorite = x.C.SH == null ? false : (x.C.SH.DocumentID > 0),
						Privillege = x.C.DCPriv == null ? null : x.C.DCPriv.Where(x => x.UserID == UserId).Select(x => new RDocumentSharedPrivillege() { IsView = x.IsView, IsEdit = x.IsEdit, IsDelete = x.IsDelete }).FirstOrDefault()
					});

			//if (!string.IsNullOrWhiteSpace(orderBy))
			//	query = query.OrderBy(x => $"x.{orderBy} {orderOrientation}");

			ResponseDocumentItemlist rdoc = new ResponseDocumentItemlist() { TotalRecord = query.Count() };

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			rdoc.Records = await query.ToListAsync();
			return rdoc;
		}

		public async Task<int> EmptyItemList()
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						List<DocumentItemList> newrList = new List<DocumentItemList>();
						DocumentItemList existitem = await context.DocumentItemList.FirstOrDefaultAsync(d => d.ItemUser == UserId);

						context.DocumentItemList.Remove(existitem);
						context.SaveChanges();

						await transaction.CommitAsync();
						result++;
					}
					catch
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
