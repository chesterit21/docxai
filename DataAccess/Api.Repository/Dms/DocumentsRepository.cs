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
using MailKit.Search;
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MimeKit.Tnef;
//using Microsoft.EntityFrameworkCore.Internal;
//using Microsoft.EntityFrameworkCore.Storage;
//using System.Linq.Dynamic.Core;
using NpgsqlTypes;
using NPOI.HPSF;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using Pipelines.Sockets.Unofficial.Buffers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text.Json;
using static Api.Domain.EntityResponses.Dms.ResponseSearchDocument;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace Api.Repository.Masters
{
	public interface IDocumentsRepository : IOwnerPrivilegesRepository<Documents>
	{
		Task<ResponseDocument> GetDetailDocumentAsync(int DocumentId);
		Task<int> SetReminderDocument(int DocumentId, Int16? days, DateTime? datetimeReminder);
		Task<ResponseInitialDocument> InsertInitAddUploaddocument(RequestDocumentAddUpload doc);
		Task<ResponseInitialDocument> UploadInitDocument(int categoryId, List<IFormFile> docs);
		Task<ResponseDocumentPagination> ListCategoryDocument(int categoryId, string docTitle, int page = 0, int limit = 0);
		Task<ResponseDocument> SaveWithChild(RequestDocuments request);
		Task<DocumentFavorite> AddFavorite(int documentId);
		Task<int> RemoveFavorite(int documentId);
		Task<DocumentFavorite> CheckFavorite(int documentId);
		Task<List<ResponseDocumentLog>> GetListDocumentLog(int documentId, int page = 0, int limit = 0, string orderBy = null, string orderOrientation = null, string filterBy = null, string filterValue = null);
		Task<int> GetListDocumentLogCount(int documentId, string filterBy = null, string filterValue = null);
		Task<int> LogDocumentInsert(Guid? transLogId, int documentId, string ActionName);
		Task<List<ResponseDocumentList>> GetHighlightDocument(string documentTitle, int categoryId, int page = 0, int limit = 0);
		Task<int> GetHighlightDocumentCount(string documentTitle, int categoryId);
		Task<List<ResponseDocumentList>> GetRecentDocument(string documentTitle, int categoryId = 0, int page = 0, int limit = 0);
		Task<int> GetRecentDocumentCount(string documentTitle, int categoryId = 0);
		Task<int> DeleteWorkFlow(int documentId);
		Task<int> AddWorkFlow(Approvals approval, List<ApprovalFlows> flows);
		Task<int> DeleteWorkFlowUser(int approvalId, int userId);
		Task<Approvals> CheckApprovalStatusDocument(int documentId);
		Task<ResponseSearchDocument> SearchListDocument(string SearchText, int page = 0, int limit = 0);
		Task<ResponseSearchDocument> AdvSearchDocument(string DocumentTitle, DateTime? DtFrom, DateTime? DtTo, int? CategoryId, int page = 0, int limit = 0);
		Task<List<DropdownTextValue>> GetDropdownDocument(string search, int[] excludedIds);
		Task<int> SaveInfoRelatedDocument(List<DocumentRelated> relateddoc, int DocumentId);
		Task<List<ResponseDocumentRelated>> GetRelatedDocumentsAsync(int DocumentId);
		Task<ResponseDocumentPagination> ListDocumentDeleted(string textSearch, int page = 0, int limit = 0);
		Task<bool> EmptyRecyclebin(int batchSize);
		Task<bool> MarkAsDeletedAsync(int documentId);
		Task<bool> MarkAsUnDeletedAsync(int documentId);
		Task<int> DeleteAsync(int documentId);
		Task<ResponseCategoryListItem> GetInfoCategoryDocumentAsync(int DocumentId);
		Task<bool> CheckOwnershipAndPrivilegesAsync(int documentId);
		Task<int> PurgeDeletedDocuments(int batchSize, int purgingInMonth);
	}

	public class DocumentsRepository(DataContext context, IHttpContextAccessor accessor) : OwnerPrivilegesRepository<Documents>(context, accessor), IDocumentsRepository
	{
		public async Task<List<ResponseDocumentRelated>> GetRelatedDocumentsAsync(int DocumentId)
		{
			return await context.RelatedDocuments.Include(d => d.Documents).Where(d => d.DocumentID == DocumentId)
			.Select(f =>
				new ResponseDocumentRelated
				{
					Id = f.Id,
					RelatedDocumentID = f.RelatedDocumentID,
					Text = f.Documents.DocumentTitle,
					Value = f.RelatedDocumentID
				}
			).ToListAsync();
		}

		public async Task<ResponseDocument> GetDetailDocumentAsync(int DocumentId)
		{
			//var result = context.Documents.Where(d => d.Id == DocumentId).Include(c => c.Categories).LeftJoin(
			//context.Approvals,
			//new[] { "DocumentIDs" }, // Outer key selector
			//new[] { "Id" }, // Inner key selector
			//(document, approval) => new
			//{
			//	Document = document,
			//	ApprovalStatusEnum = approval.Status,
			//	ApprovalStatusDesc = Enum.GetName(typeof(Domain.Enum.ApprovalStatusEnum), approval.Status)
			//});

			return await context.Documents.AsNoTracking().Include(d => d.OwnerInfo).Where(d => d.Id == DocumentId && d.IsActive == true).Include(c => c.Categories)
				.LeftJoin(context.Approvals,
					d => d.Id,
					ap => ap.DocumentID,
					(d, ap) => new
					{
						Document = d,
						Approval = ap
					})
				.Select(rd => new ResponseDocument
				{
					Id = rd.Document.Id,
					CategoryID = rd.Document.CategoryID,
					DocumentTitle = rd.Document.DocumentTitle,
					DocumentDesc = rd.Document.DocumentDesc,
					Owner = rd.Document.Owner,
					FileSize = rd.Document.FileSize == null ? SizeFormatter.SizeSuffix((long)0, 2) : SizeFormatter.SizeSuffix((long)rd.Document.FileSize, 2),
					ExpiryDate = rd.Document.ExpiryDate,
					RemindderDays = rd.Document.ReminderDays,
					RemandireDateTime = rd.Document.ReminderDateTime,
					//RelatedDocumentID = rd.Document.RelatedDocumentID,
					UpdatedAt = rd.Document.UpdatedAt,
					//Workflow = rd.Approval,
					ApprovalStatus = rd.Approval == null ? null : (int)rd.Approval.Status,
					ApprovalStatusDesc = rd.Approval == null ? null : ((Domain.Enum.ApprovalStatusEnum)rd.Approval.Status).ToString(),
					WatermarkID = rd.Document.WatermarkID,
					UserofOwner = new ResponseUser
					{
						UserID = rd.Document.Owner,
						FullName = rd.Document.OwnerInfo.FullName,
						UserName = rd.Document.OwnerInfo.UserName
					},
					Category = rd.Document.Categories != null ? new ResponseCategoryListItem
					{
						Id = rd.Document.Categories.Id,
						CategoryName = rd.Document.Categories.CategoryName,
						ParentId = rd.Document.Categories.ParentId,
						CategoryDesc = rd.Document.Categories.CategoryDesc
					} : null

				}).FirstOrDefaultAsync();
		}

		public async Task<int> SetReminderDocument(int DocumentId, Int16? days, DateTime? datetimeReminder)
		{
			var document = await context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == DocumentId);
			document.ReminderDays = days.Value;
			document.ReminderDateTime = datetimeReminder.Value;
			context.Update(document);
			return context.SaveChanges();
		}

		public async Task<ResponseInitialDocument> InsertInitAddUploaddocument(RequestDocumentAddUpload request)
		{
			int result = 0;
			Documents entity = null;
			//ResponseDocument response = null;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						/* 
						 * 1 KB = 1,024 bytes
						 * 1 MB = 1,024 KB = 1,048,576 bytes.
						 */

						DateTime DateNow = DateTime.Now;
						entity = request.CopyProperties<Documents>();

						//var fileName = Path.GetFileName(request.DocFile.FileName);
						var fileName = Path.GetFileNameWithoutExtension(request.DocFile.FileName);
						var fileExtension = Path.GetExtension(request.DocFile.FileName);
						//var filePath = Path.Combine("FilesCabinet", fileName);

						var model = await context.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryID);
						
						string newFileName = Guid.NewGuid().ToString() + fileExtension;
						var uploadPath = Extensions.DocumentFilesHelper.GetPhysicalPathForDocumentFile(UserId, UserName, model.Id, model.CategoryName.Trim(), newFileName, true);
						var resourcePath = Extensions.DocumentFilesHelper.GetResourcePathForDocumentFile(UserId, UserName, model.Id, model.CategoryName.Trim(), newFileName);
						
						using (var stream = new FileStream(uploadPath, FileMode.Create))
						{
							await request.DocFile.CopyToAsync(stream);
						}

						FileInfo fin = new FileInfo(uploadPath);

						entity.Owner = UserId;
						entity.FileSize = (int)fin.Length;
						entity.InsertedBy = UserId;
						entity.InsertedAt = DateNow;
						entity.UpdatedBy = UserId;
						entity.UpdatedAt = DateNow;
						entity = await InsertAsync(entity);
						if (entity != null)
							result++;
						//await context.AddAsync(entity);
						//context.SaveChanges();

						DocumentFiles fl = new DocumentFiles()
						{
							DocumentID = entity.Id,
							DocumentType = fin.Extension,
							DocumentFileName = fileName,
							NewDocumentFileName = fin.Name,
							//DocumentFilePath = fin.FullName,
							DocumentFilePath = resourcePath,
							DocumentFileSize = (int)fin.Length,
							IsMainDocumentFile = true,
							InsertedBy = UserId,
							InsertedAt = DateNow,
							UpdatedBy = UserId,
							UpdatedAt = DateNow
						};
						await context.AddAsync(fl);
						result += context.SaveChanges();

						//transaction here
						await transaction.CommitAsync();

						//response = new ResponseDocument();
						//response = entity.CopyProperties<ResponseDocument>();
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						throw;
					}
				}
			});
			var abc = entity.CopyProperties<ResponseInitialDocument>();
			return abc;
		}

		public async Task<ResponseInitialDocument> UploadInitDocument(int categoryId, List<IFormFile> docs)
		{
			int result = 0;
			ResponseInitialDocument entity = null;
			var strategy = context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						DateTime DateNow = DateTime.Now;
						foreach (IFormFile file in docs)
						{
							//var fileName = Path.GetFileName(file.FileName);
							var fileName = Path.GetFileNameWithoutExtension(file.FileName);
							var fileExtension = Path.GetExtension(file.FileName);

							Documents dc = new Documents() { CategoryID = categoryId };
							dc.DocumentTitle = fileName;
							dc.DocumentDesc = "";
							dc.Owner = UserId;
							dc.FileSize = (int)file.Length;
							dc.InsertedBy = UserId;
							dc.InsertedAt = DateNow;
							dc.UpdatedBy = UserId;
							dc.UpdatedAt = DateNow;
							dc = await InsertAsync(dc);

							context.SaveChanges();
							entity = dc.CopyProperties<ResponseInitialDocument>();

							var model = await context.Categories.FirstOrDefaultAsync(x => x.Id == categoryId);
							string newFileName = Guid.NewGuid().ToString() + fileExtension;

							var filePath = DocumentFilesHelper.GetPhysicalPathForDocumentFile(UserId, UserName, model.Id, model.CategoryName.Trim(), newFileName, true);
							var resourcePath = Extensions.DocumentFilesHelper.GetResourcePathForDocumentFile(UserId, UserName, model.Id, model.CategoryName.Trim(), newFileName);

							using (var stream = new FileStream(filePath, FileMode.Create))
							{
								await file.CopyToAsync(stream);
							}

							FileInfo fin = new FileInfo(filePath);
							DocumentFiles fl = new DocumentFiles()
							{
								DocumentID = dc.Id,
								DocumentType = fin.Extension,
								DocumentFileName = fileName,
								NewDocumentFileName = fin.Name,
								//DocumentFilePath = fin.FullName,
								DocumentFilePath = resourcePath,
								DocumentFileSize = (int)fin.Length,
								IsMainDocumentFile = true,
								InsertedBy = UserId,
								InsertedAt = DateNow,
								UpdatedBy = UserId,
								UpdatedAt = DateNow
							};
							await context.DocumentFiles.AddAsync(fl);
							result += context.SaveChanges();
						}

						await transaction.CommitAsync();

					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						throw;
					}
				}
			});
			return entity;
		}

		public async Task<ResponseDocumentPagination> ListCategoryDocument(int categoryId, string docTitle, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);
			var userGroupIds = await context.UserGroup
				.Where(ug => ug.UserId == UserId)
				.Select(ug => ug.GroupId)
				.ToListAsync();

			// Get all categories shared with the user along with their privileges
			var sharedCategoriesQuery = context.CategoriesShared
				.Where(cs => cs.CategoryID == categoryId &&
					((cs.UserID == UserId && cs.ShareType == "user") ||
					 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value))))
				.Select(cs => new
				{
					CategoryId = cs.CategoryID,
					IsView = cs.IsView,
					IsEdit = cs.IsEdit,
					IsDelete = cs.IsDelete
				});

			// Base query combining owned and shared documents
			var query = context.Documents
				.Include(x => x.Categories)
				.Include(x => x.DocumentFiles)
				.Include(x => x.OwnerInfo)
				.Where(x => x.IsActive == true && x.CategoryID == categoryId)
				.WhereIf(docTitle != null, x => x.DocumentTitle.ToLower().Contains(docTitle.ToLower()))
				.AsSplitQuery();

			// Get document sharing info
			var docShares = await context.DocumentSharedPrivillege
				.Include(x => x.DocumentShared)
				.Where(x => (x.UserID == UserId && x.ShareType == "user") ||
						   (x.GroupID != null && x.ShareType == "group" && userGroupIds.Contains(x.GroupID.Value)))
						   .GroupBy(x => x.DocumentShared.DocumentID)
				.Select(g => g.First())
				.ToDictionaryAsync(x => x.DocumentShared.DocumentID);

			// Get favorites
			var favorites = await context.DocumentFavorites
				.Where(x => x.Owner == UserId)
				.Select(x => x.DocumentId)
				.ToListAsync();

			// Get the data in separate queries for better translation
			var documents = await query
				.Where(x => x.Owner == UserId ||
					   context.CategoriesShared.Any(cs => cs.CategoryID == x.CategoryID &&
						   ((cs.UserID == UserId && cs.ShareType == "user") ||
							(cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value)))))
				.AsNoTracking()
				.ToListAsync();

			var categoryShares = await sharedCategoriesQuery.ToDictionaryAsync(
				cs => cs.CategoryId,
				cs => cs);

			var today = DateTime.Now.Date;
			// Process permissions in memory
			var result = documents.Select(doc =>
			{
				int? daysRemaining = doc.ExpiryDate.HasValue
				? (doc.ExpiryDate.Value.Date - today).Days
				: null;

				// 2. Build the text logic clearly
				string highlightText = "";
				if (daysRemaining.HasValue)
				{
					if (daysRemaining <= 0)
						highlightText = daysRemaining == 0 ? "Expired Today" : "Expired";
					else if (daysRemaining <= doc.ReminderDays)
						highlightText = $"Expire in {daysRemaining} Day(s)";
				}

				return new ResponseDocumentList
				{
					CategoryID = doc.CategoryID,
					DocumentId = doc.Id,
					DocumentName = doc.DocumentTitle,
					Description = doc.DocumentDesc,
					Owner = doc.Owner,
					OwnerName = doc.OwnerInfo.UserName,
					LastUpdateDate = doc.UpdatedAt,
					FilesSize = SizeFormatter.SizeSuffix(doc.DocumentFiles.Where(r => r.IsActive == true).Sum(x => x.DocumentFileSize), 2),
					InsertedAt = doc.InsertedAt,
					InsertedBy = doc.InsertedBy,
					UpdatedAt = doc.UpdatedAt,
					UpdatedBy = doc.UpdatedBy,
					FileType = doc.DocumentFiles.FirstOrDefault(me => me.IsMainDocumentFile)?.DocumentType,
					DateDiff = daysRemaining,
					HighlightText = highlightText,
					IsFavorite = favorites.Contains(doc.Id),
					IsView = doc.Owner == UserId ||
						(docShares.ContainsKey(doc.Id) && docShares[doc.Id].IsView) ||
						(categoryShares.ContainsKey(doc.CategoryID) && categoryShares[doc.CategoryID].IsView),
					IsEdit = doc.Owner == UserId ||
						(docShares.ContainsKey(doc.Id) && docShares[doc.Id].IsEdit) ||
						(categoryShares.ContainsKey(doc.CategoryID) && categoryShares[doc.CategoryID].IsEdit),
					IsDelete = doc.Owner == UserId ||
						(docShares.ContainsKey(doc.Id) && docShares[doc.Id].IsDelete) ||
						(categoryShares.ContainsKey(doc.CategoryID) && categoryShares[doc.CategoryID].IsDelete)
				};
			});

			// Join with category privileges and create final result
			//var result = await (
			//	from doc in query
			//	join catShare in sharedCategoriesQuery on doc.CategoryID equals catShare.CategoryId into catShareGroup
			//	from catShare in catShareGroup.DefaultIfEmpty()
			//	where doc.Owner == UserId || catShare != null // User's own documents or shared categories
			//	select new ResponseDocumentList
			//	{
			//		CategoryID = doc.CategoryID,
			//		DocumentId = doc.Id,
			//		DocumentName = doc.DocumentTitle,
			//		Description = doc.DocumentDesc,
			//		Owner = doc.Owner,
			//		OwnerName = doc.OwnerInfo.UserName,
			//		LastUpdateDate = doc.UpdatedAt,
			//		FilesSize = SizeFormatter.SizeSuffix(doc.DocumentFiles.Sum(x => x.DocumentFileSize), 2),
			//		InsertedAt = doc.InsertedAt,
			//		InsertedBy = doc.InsertedBy,
			//		UpdatedAt = doc.UpdatedAt,
			//		UpdatedBy = doc.UpdatedBy,
			//		FileType = doc.DocumentFiles.FirstOrDefault(me => me.IsMainDocumentFile).DocumentType,
			//		IsFavorite = favorites.Contains(doc.Id),
			//		IsView = doc.Owner == UserId ||
			//	(docShares != null && docShares.GetValueOrDefault(doc.Id).IsView) ||
			//	(catShare != null && catShare.IsView),
			//		IsEdit = doc.Owner == UserId ||
			//	(docShares != null && docShares.GetValueOrDefault(doc.Id).IsEdit) ||
			//	(catShare != null && catShare.IsEdit),
			//		IsDelete = doc.Owner == UserId ||
			//	(docShares != null && docShares.GetValueOrDefault(doc.Id).IsDelete) ||
			//	(catShare != null && catShare.IsDelete)
			//	})
			//	.ToListAsync();

			var totalRecords = result.Count();

			// Apply pagination
			if (page > 0 && limit > 0)
			{
				result = result.Skip(skip).Take(limit).ToList();
			}

			return new ResponseDocumentPagination
			{
				TotalRecord = totalRecords,
				Record = result.ToList()
			};
		}

		private bool GetViewPermission(int documentOwner, DocumentSharedPrivillege docShare, CategoriesShared catShare, string flagPriv)
		{
			// If user is the owner, they always have view permission
			if (documentOwner == UserId)
				return true;

			// Check document-level sharing first
			if (docShare != null)
			{
				if (flagPriv == "IsView")
				{
					return docShare.IsView;
				}
				else if (flagPriv == "IsEdit")
				{
					return docShare.IsEdit;
				}
				else if (flagPriv == "IsDelete")
				{
					return docShare.IsDelete;
				}
			}


			// Fall back to category-level sharing
			if (catShare != null)
			{
				if (flagPriv == "IsView")
				{
					return catShare.IsView;
				}
				else if (flagPriv == "IsEdit")
				{
					return catShare.IsEdit;
				}
				else if (flagPriv == "IsDelete")
				{
					return catShare.IsDelete;
				}
			}

			// No permissions found
			return false;
		}

		public async Task<ResponseDocumentPagination> ListDocumentDeleted(string textSearch, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);

			var query = context.Documents.Include(x => x.Categories).Include(x => x.DocumentFiles).Include(x => x.OwnerInfo)
				.Where(x => x.IsActive == false && x.IsDeleted == false && x.DocumentTitle.ToLower().Contains(textSearch == null ? "" : textSearch.ToLower()))
				.WhereIf(UserRoleInApp == "user", x => x.Owner == UserId)
				.LeftJoin(context.User,
					left => left.InsertedBy,
					user => user.UserId,
				(document, userInsert) => new
				{
					Entity = document,
					InsertedByUserName = userInsert.UserName,
					InsertedByFullName = userInsert.FullName,
					UpdatedByUserName = "",
					UpdatedByFullName = ""
				})
				.LeftJoin(context.User,
					"Entity.UpdatedBy",
					"UserId",
				(left, userUpdate) => new
				{
					left.Entity,
					left.InsertedByUserName,
					left.InsertedByFullName,
					UpdatedByUserName = userUpdate.UserName,
					UpdatedByFullName = userUpdate.FullName
				})
				.Select(x => new ResponseDocumentList()
				{
					CategoryID = x.Entity.CategoryID,
					DocumentId = x.Entity.Id,
					DocumentName = x.Entity.DocumentTitle,
					Description = x.Entity.DocumentDesc,
					Owner = x.Entity.Owner,
					OwnerName = x.Entity.OwnerInfo.UserName,
					LastUpdateDate = x.Entity.UpdatedAt,
					FilesSize = SizeFormatter.SizeSuffix(x.Entity.DocumentFiles.Sum(x => x.DocumentFileSize), 2),
					InsertedAt = x.Entity.InsertedAt,
					InsertedBy = x.Entity.InsertedBy,
					UpdatedAt = x.Entity.UpdatedAt,
					UpdatedBy = x.Entity.UpdatedBy,
					FileType = x.Entity.DocumentFiles.FirstOrDefault(me => me.IsMainDocumentFile).DocumentType,
					InsertedByUserName = x.InsertedByUserName,
					InsertedByFullName = x.InsertedByFullName,
					UpdatedByUserName = x.UpdatedByUserName,
					UpdatedByFullName = x.UpdatedByFullName
				});

			ResponseDocumentPagination responseDocumentPagination = new ResponseDocumentPagination() { TotalRecord = query.Count() };

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			responseDocumentPagination.Record = await query.ToListAsync();
			return responseDocumentPagination;
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
						var recordsToDelete = await context.Documents.Where(x => x.IsActive == false)
						.WhereIf(UserRoleInApp == "user", x => x.Owner == UserId).ToListAsync();
						if (recordsToDelete.Count == 0)
						{
							result = 0;
						}

						for (int i = 0; i < recordsToDelete.Count; i += batchSize)
						{
							var batch = recordsToDelete.Skip(i).Take(batchSize).ToList();
							int batchCount = batch.Count();
							for (int x = 0; x < batchCount; x++)
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
								result += context.SaveChanges();
								//context.DocumentLog.RemoveRange(logs);
								//result += context.SaveChanges();

								var approval = await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == idDocument && x.CategoryID != null);
								if (approval != null)
								{
									var activities = await context.ApprovalActivities.Where(x => x.ApprovalID == approval.Id).ToListAsync();
									foreach (var act in activities)
									{
										act.IsDeleted = true;
										act.DeletedBy = UserId;
										act.DeletedAt = DateTime.Now;
									}

									context.ApprovalActivities.UpdateRange(activities);
									result += context.SaveChanges();
									//context.ApprovalActivities.RemoveRange(activities);
									//result += context.SaveChanges();

									var flows = await context.ApprovalFlows.Where(x => x.ApprovalID == approval.Id).ToListAsync();
									foreach (var flow in flows)
									{
										flow.IsDeleted = true;
										flow.DeletedBy = UserId;
										flow.DeletedAt = DateTime.Now;
									}

									context.ApprovalFlows.UpdateRange(flows);
									result += context.SaveChanges();
									//context.ApprovalFlows.RemoveRange(flows);
									//result += context.SaveChanges();

									approval.IsDeleted = true;
									approval.DeletedBy = UserId;
									approval.DeletedAt = DateTime.Now;

									context.Approvals.Update(approval);
									result += context.SaveChanges();
									//context.Approvals.Remove(approval);
									//result += context.SaveChanges();
								}

								var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == idDocument).ToListAsync();
								foreach (var da in docAttr)
								{
									da.IsDeleted = true;
									da.DeletedBy = UserId;
									da.DeletedAt = DateTime.Now;
								}

								context.DocumentAttributes.UpdateRange(docAttr);
								result += context.SaveChanges();
								//context.DocumentAttributes.RemoveRange(docAttr);
								//result += context.SaveChanges();

								var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == idDocument).ToListAsync();
								if (docFavs.Count > 0)
								{
									foreach (var df in docFavs)
									{
										df.IsDeleted = true;
										df.DeletedBy = UserId;
										df.DeletedAt = DateTime.Now;
									}

									context.DocumentFavorites.UpdateRange(docFavs);
									result += context.SaveChanges();
								}
								//if (docFavs.Count > 0)
								//{
								//	context.DocumentFavorites.RemoveRange(docFavs);
								//	result += context.SaveChanges();
								//}	

								var model = await context.Categories.FirstOrDefaultAsync(y => y.Id == batch[x].CategoryID);								
								var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == idDocument).ToListAsync();

								if (docFiles.Count > 0)
								{
									foreach (var df in docFiles)
									{
										df.IsDeleted = true;
										df.DeletedBy = UserId;
										df.DeletedAt = DateTime.Now;
									}

									context.DocumentFiles.UpdateRange(docFiles);
									result += context.SaveChanges();
								}

								var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docItemList.Count > 0)
								{
									foreach (var di in docItemList)
									{
										di.IsDeleted = true;
										di.DeletedBy = UserId;
										di.DeletedAt = DateTime.Now;
									}

									context.DocumentItemList.UpdateRange(docItemList);
									result += context.SaveChanges();
									//context.DocumentItemList.RemoveRange(docItemList);
									//result += context.SaveChanges();
								}

								var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docRelated.Count > 0)
								{
									foreach (var dr in docRelated)
									{
										dr.IsDeleted = true;
										dr.DeletedBy = UserId;
										dr.DeletedAt = DateTime.Now;
									}

									context.RelatedDocuments.UpdateRange(docRelated);
									result += context.SaveChanges();
									//context.RelatedDocuments.RemoveRange(docRelated);
									//result += context.SaveChanges();
								}

								var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == idDocument).ToListAsync();
								if (docReminders.Count > 0)
								{
									foreach (var t in docReminders)
									{
										t.IsDeleted = true;
										t.DeletedBy = UserId;
										t.DeletedAt = DateTime.Now;
									}

									context.DocumentReminders.UpdateRange(docReminders);
									result += context.SaveChanges();
									//context.DocumentReminders.RemoveRange(docReminders);
									//result += context.SaveChanges();
								}

								var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == idDocument);
								if (docShared != null)
								{
									var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
									foreach (var t in sharePriv)
									{
										t.IsDeleted = true;
										t.DeletedBy = UserId;
										t.DeletedAt = DateTime.Now;
									}

									context.DocumentSharedPrivillege.UpdateRange(sharePriv);
									result += context.SaveChanges();
									//context.DocumentSharedPrivillege.RemoveRange(sharePriv);
									//result += context.SaveChanges();

									docShared.IsDeleted = true;
									docShared.DeletedBy = UserId;
									docShared.DeletedAt = DateTime.Now;

									context.DocumentShared.Update(docShared);
									result += context.SaveChanges();
									//context.DocumentShared.Remove(docShared);
									//result += context.SaveChanges();
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

									//context.Notifications.RemoveRange(notifs);
									//result += context.SaveChanges();
								}

								context.Documents.Remove(batch[x]);
								result += context.SaveChanges();
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

		public async Task<ResponseDocument> SaveWithChild(RequestDocuments request)
		{
			ResponseDocument response = null;
			DocumentFiles fl = null;


			using var transaction = context.Database.BeginTransaction();
			{
				try
				{
					var entity = request.CopyProperties<Documents>();

					if (request.DocumentFiles != null)
					{
						var model = await context.Categories.FirstOrDefaultAsync(x => x.Id == entity.CategoryID);

						//var fileName = Path.GetFileName(request.DocumentFiles.file.FileName);
						var fileName = Path.GetFileNameWithoutExtension(request.DocumentFiles.file.FileName);
						var fileExtension = Path.GetExtension(request.DocumentFiles.file.FileName);

						//var filePath = Path.Combine("FilesCabinet", fileName);
						string newFileName = Guid.NewGuid().ToString() + fileExtension;

						var filePath = DocumentFilesHelper.GetPhysicalPathForDocumentFile(UserId, UserName, model.Id, model.CategoryName.Trim(), newFileName, true);
						var resourcePath = Extensions.DocumentFilesHelper.GetResourcePathForDocumentFile(UserId, UserName, model.Id, model.CategoryName.Trim(), newFileName);

						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await request.DocumentFiles.file.CopyToAsync(stream);
						}

						FileInfo fin = new FileInfo(filePath);

						entity.FileSize = (int)fin.Length;
						entity = await InsertAsync(entity);
						//await context.AddAsync(entity);
						//context.SaveChanges();
						fl = new DocumentFiles()
						{
							DocumentID = entity.Id,
							DocumentType = fin.Extension,
							DocumentFileName = fileName,
							NewDocumentFileName = fin.Name,
							//DocumentFilePath = fin.FullName,
							DocumentFilePath = resourcePath,
							DocumentFileSize = (int)fin.Length,
							IsMainDocumentFile = true
						};
						await context.AddAsync(fl);
						context.SaveChanges();
					}
					else
					{
						entity = await InsertAsync(entity);
					}

					RequestDocumentShared ReqDS = request.RequestDocumentShared;
					if (ReqDS != null)
					{
						DocumentShared docShared = new DocumentShared()
						{
							DocumentID = entity.Id,
							//DocumentFileID = ReqDS.DocumentFileID.Value
						};

						//if (fl != null)
						//	docShared.DocumentFileID = fl.Id;

						await context.AddAsync(docShared);

						if (request.RequestDocumentShared.requestUserDocPrivillege.Count > 0)
						{
							List<DocumentSharedPrivillege> docSharePrivs = new List<DocumentSharedPrivillege>();

							foreach (RequestUserDocumentPrivillege item in request.RequestDocumentShared.requestUserDocPrivillege)
							{
								DocumentSharedPrivillege docPriv = new DocumentSharedPrivillege()
								{
									DocumentSharedID = docShared.Id,
									UserID = item.UserID,
									IsView = item.IsView,
									IsEdit = item.IsEdit,
									IsDelete = item.IsDelete
								};
								docSharePrivs.Add(docPriv);
							}

							if (docSharePrivs.Count > 0)
							{
								await context.DocumentSharedPrivillege.AddRangeAsync(docSharePrivs);
								//await InsertManyAsync(docSharePrivs);
							}
						}
					}

					if (request.ReqApprovalFlows.Count > 0)
					{
						Approvals approvals = new Approvals()
						{
							CategoryID = request.CategoryID,
							DocumentID = request.Id,
							Status = Domain.Enum.ApprovalStatusEnum.None
						};

						context.Approvals.Add(approvals);
						await context.SaveChangesAsync();

						List<ApprovalFlows> rafs = new List<ApprovalFlows>();
						foreach (var raf in request.ReqApprovalFlows)
						{
							ApprovalFlows flow = new ApprovalFlows()
							{
								ApprovalID = approvals.Id,
								Step = raf.Step,
								ApproverUserID = raf.UserID,
								//RoleID = raf.RoleID
							};
							rafs.Add(flow);
						}

						await context.ApprovalFlows.AddRangeAsync(rafs);
						context.SaveChanges();

						Notifications notif = new Notifications()
						{
							DocumentID = entity.Id,
							NotificationType = (short)NotificationType.Approval,
							NotifDescription = "Document Submition, Need Your Approval",
							TargetActor = rafs.FirstOrDefault(x => x.Step == rafs.Min(d => d.Step)).ApproverUserID,
							InsertedBy = UserId,
							InsertedAt = DateTime.Now
						};
						await context.Notifications.AddAsync(notif);
						context.SaveChanges();
					}
					transaction.Commit();

					response = new ResponseDocument();
					response = entity.CopyProperties<ResponseDocument>();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw;
				}
			}
			return response;
		}

		public async Task<DocumentFavorite> AddFavorite(int documentId)
		{
			var entity = new DocumentFavorite() { DocumentId = documentId, Owner = UserId, InsertedAt = DateTime.Now, InsertedBy = UserId };
			await context.DocumentFavorites.AddAsync(entity);
			await context.SaveChangesAsync();
			return entity;
		}

		public async Task<int> RemoveFavorite(int documentId)
		{
			var dv = await context.DocumentFavorites.FirstOrDefaultAsync(x => x.DocumentId == documentId && x.Owner == UserId);
			context.DocumentFavorites.Remove(dv);
			int res = await context.SaveChangesAsync();
			return res;
		}

		public async Task<DocumentFavorite> CheckFavorite(int documentId)
		{
			return await context.DocumentFavorites.FirstOrDefaultAsync(x => x.DocumentId == documentId && x.Owner == UserId);
		}

		public async Task<List<ResponseDocumentLog>> GetListDocumentLog(int documentId, int page = 0, int limit = 0, string orderBy = null, string orderOrientation = null,
			string filterBy = null, string filterValue = null)
		{
			var skip = Skip(page, limit);

			var query = context.DocumentLog.Include(x => x.Documents)
				.Where(x => x.Documents.Owner == UserId && x.DocumentID == documentId)
				.LeftJoin(context.User,
					left => left.InsertedBy,
					user => user.UserId,
					(documentlog, userInsert) => new
					{
						Entity = documentlog,
						InsertedByUserName = userInsert.UserName,
						InsertedByFullName = userInsert.FullName,
						UpdatedByUserName = "",
						UpdatedByFullName = ""
					})
				.LeftJoin(context.User,
					"Entity.UpdatedBy",
					"UserId",
					(left, userUpdate) => new
					{
						left.Entity,
						left.InsertedByUserName,
						left.InsertedByFullName,
						UpdatedByUserName = userUpdate.UserName,
						UpdatedByFullName = userUpdate.FullName
					})
				.LeftJoin(context.AuditTrail,
					left => left.Entity.LogAuditTrailID,
					audit => audit.Id,
					(left, audit) => new
					{
						left.Entity,
						left.InsertedByUserName,
						left.InsertedByFullName,
						left.UpdatedByUserName,
						left.UpdatedByFullName,
						AuditTrls = audit
					})
					.WhereIf(!string.IsNullOrWhiteSpace(filterBy) && !string.IsNullOrWhiteSpace(filterValue), $"{filterBy}.Contains(@0)", filterValue)
					.Select(x => new ResponseDocumentLog()
					{
						Id = x.Entity.Id,
						DocumentID = x.Entity.DocumentID,
						DocumentTitle = x.Entity.Documents.DocumentTitle,
						DocumentDesc = x.Entity.Documents.DocumentDesc,
						ActionLogDocument = x.Entity.ActionLogDocument,
						Before = x.AuditTrls == null ? null : x.AuditTrls.Before,
						After = x.AuditTrls == null ? null : x.AuditTrls.After,
						InsertedAt = x.Entity.InsertedAt,
						InsertedByUserName = x.InsertedByUserName,
						InsertedByFullName = x.InsertedByFullName,
						UpdatedByUserName = x.UpdatedByUserName,
						UpdatedByFullName = x.UpdatedByFullName,
					});



			if (!string.IsNullOrWhiteSpace(orderBy))
				query = query.OrderBy(x => $"x.{orderBy} {orderOrientation}");

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			return await query.ToListAsync();
		}
		public async Task<int> GetListDocumentLogCount(int documentId, string filterBy = null, string filterValue = null)
		{
			var query = context.DocumentLog.Include(x => x.Documents).Include(x => x.AuditTrls)
				.Where(x => x.Documents.Owner == UserId && x.DocumentID == documentId)
				.LeftJoin(context.User,
					left => left.InsertedBy,
					user => user.UserId,
					(documentlog, userInsert) => new
					{
						Entity = documentlog,
						InsertedByUserName = userInsert.UserName,
						InsertedByFullName = userInsert.FullName,
						UpdatedByUserName = "",
						UpdatedByFullName = ""
					})
				.LeftJoin(context.User,
					"Entity.UpdatedBy",
					"UserId",
					(left, userUpdate) => new
					{
						left.Entity,
						left.InsertedByUserName,
						left.InsertedByFullName,
						UpdatedByUserName = userUpdate.UserName,
						UpdatedByFullName = userUpdate.FullName
					})
					.WhereIf(!string.IsNullOrWhiteSpace(filterBy) && !string.IsNullOrWhiteSpace(filterValue), $"{filterBy}.Contains(@0)", filterValue)
					.Select(x => new ResponseDocumentLog()
					{
						Id = x.Entity.Id,
						DocumentTitle = x.Entity.Documents.DocumentTitle,
						DocumentDesc = x.Entity.Documents.DocumentDesc,
						ActionLogDocument = x.Entity.ActionLogDocument,
						Before = x.Entity.AuditTrls.Before,
						After = x.Entity.AuditTrls.After,
						InsertedAt = x.Entity.InsertedAt,
						InsertedByUserName = x.InsertedByUserName,
						InsertedByFullName = x.InsertedByFullName,
						UpdatedByUserName = x.UpdatedByUserName,
						UpdatedByFullName = x.UpdatedByFullName,
					});


			return await query.CountAsync();
		}

		public async Task<int> LogDocumentInsert(Guid? trnsLogId, int documentId, string ActionName)
		{
			AuditTrail audt = null;
			DocumentLog dcLog = null;

			audt = context.AuditTrail.FirstOrDefault(x => x.TransactionLogId == trnsLogId);
			if (audt != null)
			{
				dcLog = new DocumentLog
				{
					DocumentID = documentId,
					LogAuditTrailID = audt.Id,
					ActionLogDocument = ActionName,
					InsertedBy = UserId,
					InsertedAt = DateTime.Now
				};
			}
			else
			{
				dcLog = new DocumentLog
				{
					DocumentID = documentId,
					ActionLogDocument = ActionName,
					InsertedBy = UserId,
					InsertedAt = DateTime.Now
				};
			}

			context.DocumentLog.Add(dcLog);
			return await context.SaveChangesAsync();
		}

		public async Task<List<ResponseDocumentList>> GetHighlightDocument(string documentTitle, int categoryId = 0, int page = 0, int limit = 0)
		{
			// 1. Get the local "Today" and force it to be UTC so Postgres accepts it
			var localToday = DateTime.Now.Date; // e.g., 2026-01-06 00:00:00 (Local)
			//var today = DateTime.SpecifyKind(localToday, DateTimeKind.Utc); // Same time, but marked as UTC
			var today = DateTime.Today;

			// 1. Build the Base Query (Filtering)
			var docQuery = context.Documents.AsNoTracking()
				.Where(x => x.Owner == UserId && x.IsActive);

			//docQuery = docQuery.Where(x => context.DocumentReminders.Any(r => r.DocumentID == x.Id && r.ReminderDateTime.Date == today)
			//);
			//Apply Complex Filter: (Expired OR Expiring Soon) OR(Has Reminder Today)
			docQuery = docQuery.Where(x =>
				(
					x.ExpiryDate != null &&
					x.ReminderDays != null &&
					(x.ExpiryDate.Value.Date <= today || (x.ExpiryDate.Value.Date - today).Days <= x.ReminderDays.Value)
				)
				||
				context.DocumentReminders.Any(r => r.DocumentID == x.Id && r.ReminderDateTime.Date == today)
			);

			// Apply Dynamic Filters
			if (categoryId > 0)
				docQuery = docQuery.Where(x => x.CategoryID == categoryId); // Assuming CategoryID is on Document, otherwise adjust

			if (!string.IsNullOrEmpty(documentTitle))
				docQuery = docQuery.Where(x => x.DocumentTitle.Contains(documentTitle));

			var projectionQuery =
				from d in docQuery
				join uIns in context.User.AsNoTracking() on d.InsertedBy equals uIns.UserId into uInsGroup
				from uIns in uInsGroup.DefaultIfEmpty()
				join uUpd in context.User.AsNoTracking() on d.UpdatedBy equals uUpd.UserId into uUpdGroup
				from uUpd in uUpdGroup.DefaultIfEmpty()
				join rem in context.DocumentReminders.AsNoTracking().Where(r => r.ReminderDateTime.Date == today)
					on d.Id equals rem.DocumentID into remGroup
				from rem in remGroup.DefaultIfEmpty()
				join docFav in context.DocumentFavorites on new { d.Id, Owner = UserId } equals new { Id = docFav.DocumentId, docFav.Owner } into docFavGroup
				from docFav in docFavGroup.DefaultIfEmpty()
				orderby d.InsertedAt descending
				select new
				{
					Entity = d,
					InsertedByName = uIns != null ? uIns.UserName : null,
					InsertedByFull = uIns != null ? uIns.FullName : null,
					UpdatedByName = uUpd != null ? uUpd.UserName : "",
					UpdatedByFull = uUpd != null ? uUpd.FullName : "",
					ReminderTime = rem != null ? rem.ReminderDateTime : (DateTime?)null,
					ReminderDesc = rem != null ? rem.ReminderDesc : null,
					TotalFileSize = d.DocumentFiles.Sum(f => (long)f.DocumentFileSize),
					MainFileType = d.DocumentFiles.Where(f => f.IsMainDocumentFile).Select(f => f.DocumentType).FirstOrDefault(),
					OwnerFullName = d.OwnerInfo.FullName, // EF Core will auto-join OwnerInfo if it is a navigation property
					IsFavorite = (docFav == null ? 0 : (docFav.Owner == UserId ? 1 : 0))
				};

			// 3. Pagination (in SQL)
			if (page > 0 && limit > 0)
			{
				var skip = Skip(page, limit);
				projectionQuery = projectionQuery.Skip(skip).Take(limit);
			}

			// 4. Execute Query (Fetch Data)
			var rawData = await projectionQuery.Distinct().ToListAsync();

			// 5. In-Memory Formatting 
			// map to the final class and run C# logic (SizeFormatter, Strings, etc.)
			var resultList = rawData.Select(x =>
			{
				var dateDiff = x.Entity.ExpiryDate.HasValue
					? (x.Entity.ExpiryDate.Value.Date - today).Days
					: 0;

				return new ResponseDocumentList
				{
					DocumentId = x.Entity.Id,
					DocumentName = x.Entity.DocumentTitle,
					Description = x.Entity.DocumentDesc,
					Owner = x.Entity.Owner,
					OwnerName = x.OwnerFullName,
					LastUpdateDate = x.Entity.UpdatedAt,
					FilesSize = SizeFormatter.SizeSuffix(x.TotalFileSize, 2),
					InsertedAt = x.Entity.InsertedAt,
					InsertedBy = x.Entity.InsertedBy,
					UpdatedAt = x.Entity.UpdatedAt,
					UpdatedBy = x.Entity.UpdatedBy,
					FileType = x.MainFileType,
					DateDiff = dateDiff,
					InsertedByUserName = x.InsertedByName,
					InsertedByFullName = x.InsertedByFull,
					UpdatedByUserName = x.UpdatedByName,
					UpdatedByFullName = x.UpdatedByFull,
					ReminderDateTime = x.ReminderTime,
					ReminderDesc = x.ReminderDesc,
					IsExpired = x.Entity.ExpiryDate != null && x.Entity.ExpiryDate.Value.Date <= today,
					DaysUntilExpiry = dateDiff,
					ExpiryDate = x.Entity.ExpiryDate,
					ReminderDays = x.Entity.ReminderDays,
					HighlightText =
						 (x.Entity.ExpiryDate != null && dateDiff <= 0) ? "Expired" :
						 (x.Entity.ExpiryDate != null && dateDiff <= x.Entity.ReminderDays) ?
							 (dateDiff == 0 ? "Expired Today" : $"Expire in {dateDiff} Day(s)") :
						 (x.ReminderTime != null ? "Reminder for document" : ""),
					IsFavorite = x.IsFavorite > 0,
					IsView = true,
					IsEdit = true,
					IsDelete = true
				};
			}).ToList();

			// 6. Post-Process Approval Info
			resultList = await SetInfoApproval(resultList);

			return resultList;
		}

		public async Task<int> GetHighlightDocumentCount(string documentTitle, int categoryId = 0)
		{
			// 1. Get the local "Today" and force it to be UTC so Postgres accepts it
			var localToday = DateTime.Now.Date; // e.g., 2026-01-06 00:00:00 (Local)
			//var today = DateTime.SpecifyKind(localToday, DateTimeKind.Utc); // Same time, but marked as UTC
			var today = DateTime.Today;

			// 1. Build the Base Query (Filtering)
			var docQuery = context.Documents.AsNoTracking()
				.Where(x => x.Owner == UserId && x.IsActive);

			//docQuery = docQuery.Where(x => context.DocumentReminders.Any(r => r.DocumentID == x.Id && r.ReminderDateTime.Date == today) 
			//);
			//Apply Complex Filter: (Expired OR Expiring Soon) OR(Has Reminder Today)
			docQuery = docQuery.Where(x =>
				(
					x.ExpiryDate != null &&
					x.ReminderDays != null &&
					(x.ExpiryDate.Value.Date <= today || (x.ExpiryDate.Value.Date - today).Days <= x.ReminderDays.Value)
				)
				||
				context.DocumentReminders.Any(r => r.DocumentID == x.Id && r.ReminderDateTime.Date == today)
			);

			// Apply Dynamic Filters
			if (categoryId > 0)
				docQuery = docQuery.Where(x => x.CategoryID == categoryId); // Assuming CategoryID is on Document, otherwise adjust

			if (!string.IsNullOrEmpty(documentTitle))
				docQuery = docQuery.Where(x => x.DocumentTitle.Contains(documentTitle));

			var projectionQuery =
				from d in docQuery
				join uIns in context.User.AsNoTracking() on d.InsertedBy equals uIns.UserId into uInsGroup
				from uIns in uInsGroup.DefaultIfEmpty()
				join uUpd in context.User.AsNoTracking() on d.UpdatedBy equals uUpd.UserId into uUpdGroup
				from uUpd in uUpdGroup.DefaultIfEmpty()
				join rem in context.DocumentReminders.AsNoTracking().Where(r => r.ReminderDateTime.Date == today)
					on d.Id equals rem.DocumentID into remGroup
				from rem in remGroup.DefaultIfEmpty()
				join docFav in context.DocumentFavorites on new { d.Id, Owner = UserId } equals new { Id = docFav.DocumentId, docFav.Owner } into docFavGroup
				from docFav in docFavGroup.DefaultIfEmpty()
				orderby d.InsertedAt descending
				select new
				{
					Entity = d,
					InsertedByName = uIns != null ? uIns.UserName : null,
					InsertedByFull = uIns != null ? uIns.FullName : null,
					UpdatedByName = uUpd != null ? uUpd.UserName : "",
					UpdatedByFull = uUpd != null ? uUpd.FullName : "",
					ReminderTime = rem != null ? rem.ReminderDateTime : (DateTime?)null,
					ReminderDesc = rem != null ? rem.ReminderDesc : null,
					TotalFileSize = d.DocumentFiles.Sum(f => (long)f.DocumentFileSize),
					MainFileType = d.DocumentFiles.Where(f => f.IsMainDocumentFile).Select(f => f.DocumentType).FirstOrDefault(),
					OwnerFullName = d.OwnerInfo.FullName, // EF Core will auto-join OwnerInfo if it is a navigation property
					IsFavorite = (docFav == null ? 0 : (docFav.Owner == UserId ? 1 : 0))
				};

			var count = await projectionQuery.Distinct().CountAsync();
			return count;
		}
		public async Task<List<ResponseDocumentList>> GetRecentDocument(string documentTitle, int categoryId = 0, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);
			var tenDaysAgo = DateTime.Now.AddDays(-10);
			var today = DateTime.Now.Date;

			//var favorites = await context.DocumentFavorites
			//	.Where(x => x.Owner == UserId)
			//	.Select(x => x.DocumentId)
			//	.ToListAsync();

			var query = context.Documents.Include(x => x.Categories).Include(x => x.DocumentFiles).Include(x => x.OwnerInfo)
				.Where(x => x.Owner == UserId && x.IsActive == true && x.InsertedAt >= tenDaysAgo)
				.WhereIf(!string.IsNullOrEmpty(documentTitle), $"DocumentTitle.Contains(@0)", documentTitle)
				.WhereIf(categoryId > 0, $"CategoryID ==", categoryId)
				.LeftJoin(context.User,
					left => left.InsertedBy,
					user => user.UserId,
					(document, userInsert) => new
					{
						Entity = document,
						DateDiff = document.ExpiryDate != null ? (document.ExpiryDate - today).Value.Days : 0,
						InsertedByUserName = userInsert.UserName,
						InsertedByFullName = userInsert.FullName,
						UpdatedByUserName = "",
						UpdatedByFullName = ""
					})
				.LeftJoin(context.User,
					"Entity.UpdatedBy",
					"UserId",
					(left, userUpdate) => new
					{
						left.Entity,
						left.DateDiff,
						left.InsertedByUserName,
						left.InsertedByFullName,
						UpdatedByUserName = userUpdate.UserName,
						UpdatedByFullName = userUpdate.FullName
					})
				.LeftJoin(context.DocumentFavorites.Where(fv => fv.Owner == UserId),
					new[] { "Entity.Id" },
					new[] { "DocumentId"},
					(left, ct) => new
					{
						left.Entity,
						left.DateDiff,
						left.InsertedByUserName,
						left.InsertedByFullName,
						left.UpdatedByUserName,
						left.UpdatedByFullName,
						FavoriteType = "Document",
						FavoriteDocumentID = (ct == null ? 0 : (ct.Owner == UserId ? 1 : 0))
					})
					.Select(x => new ResponseDocumentList()
					{
						DocumentId = x.Entity.Id,
						DocumentName = x.Entity.DocumentTitle,
						Description = x.Entity.DocumentDesc,
						Owner = x.Entity.Owner,
						OwnerName = x.Entity.OwnerInfo.UserName,
						LastUpdateDate = x.Entity.UpdatedAt,
						FilesSize = SizeFormatter.SizeSuffix(x.Entity.DocumentFiles.Where(c => c.IsActive == true).Sum(x => x.DocumentFileSize), 2),
						InsertedAt = x.Entity.InsertedAt,
						InsertedBy = x.Entity.InsertedBy,
						UpdatedAt = x.Entity.UpdatedAt,
						UpdatedBy = x.Entity.UpdatedBy,
						FileType = x.Entity.DocumentFiles.FirstOrDefault(me => me.IsMainDocumentFile).DocumentType,
						InsertedByUserName = x.InsertedByUserName,
						InsertedByFullName = x.InsertedByFullName,
						UpdatedByUserName = x.UpdatedByUserName,
						UpdatedByFullName = x.UpdatedByFullName,
						IsExpired = x.Entity.ExpiryDate != null && x.Entity.ExpiryDate.Value.Date <= today.Date,
						DaysUntilExpiry = x.DateDiff, // Menggunakan DateDiff yang sudah dihitung
						ExpiryDate = x.Entity.ExpiryDate,
						HighlightText = x.DateDiff <= 0 ? "Expired" : (x.DateDiff <= x.Entity.ReminderDays ? (x.DateDiff == 0 ? $"Expired Today" : $"Expire in {x.DateDiff} Day(s)") : ""),
						IsFavorite = (x.FavoriteDocumentID > 0),
						IsView = true,
						IsEdit = true,
						IsDelete = true
					}).Distinct();

			//if (!string.IsNullOrWhiteSpace(orderBy))
			// query = query.OrderBy(x => $"x.UpdatedAt DESC");
			query = query.OrderByDescending(x => x.UpdatedAt);

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			var list = await query.ToListAsync();
			list = await SetInfoApproval(list);
			return list;
		}

		public async Task<int> GetRecentDocumentCount(string documentTitle, int categoryId = 0)
		{
			var tenDaysAgo = DateTime.Now.AddDays(-10);

			var query = context.Documents.Include(x => x.Categories).Include(x => x.DocumentFiles).Include(x => x.OwnerInfo)
				.Where(x => x.Owner == UserId && x.IsActive == true && x.InsertedAt >= tenDaysAgo)
				.WhereIf(!string.IsNullOrEmpty(documentTitle), $"DocumentTitle.Contains(@0)", documentTitle)
				.WhereIf(categoryId > 0, $"CategoryID ==", categoryId)
				.LeftJoin(context.User,
					left => left.InsertedBy,
					user => user.UserId,
					(document, userInsert) => new
					{
						Entity = document,
						InsertedByUserName = userInsert.UserName,
						InsertedByFullName = userInsert.FullName,
						UpdatedByUserName = "",
						UpdatedByFullName = ""
					})
				.LeftJoin(context.User,
					"Entity.UpdatedBy",
					"UserId",
					(left, userUpdate) => new
					{
						left.Entity,
						left.InsertedByUserName,
						left.InsertedByFullName,
						UpdatedByUserName = userUpdate.UserName,
						UpdatedByFullName = userUpdate.FullName
					})
				.LeftJoin(context.DocumentFavorites.Where(fv => fv.Owner == UserId),
					new[] { "Entity.Id" },
					new[] { "DocumentId" },
					(left, ct) => new
					{
						left.Entity,
						left.InsertedByUserName,
						left.InsertedByFullName,
						left.UpdatedByUserName,
						left.UpdatedByFullName,
						FavoriteType = "Document",
						FavoriteDocumentID = (ct == null ? 0 : (ct.Owner == UserId ? 1 : 0))
					})
					.Select(x => x.Entity.Id).Distinct();

			//if (!string.IsNullOrWhiteSpace(orderBy))
			// query = query.OrderBy(x => $"x.UpdatedAt DESC");
			//query = query.OrderByDescending(x => x.UpdatedAt);

			var count = await query.CountAsync();
			return count;
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
						List<ApprovalFlows> NewFlowUser = new List<ApprovalFlows>();
						Approvals dataExist = await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == approval.DocumentID);
						if (dataExist != null)
						{
							dataExist.Notes = approval.Notes;
							dataExist.UpdatedBy = UserId;
							dataExist.UpdatedAt = DateTime.Now;
							dataExist.MaxStep = flows.Count();
							context.Approvals.Update(dataExist);
							await context.SaveChangesAsync();
							foreach (var flow in flows)
							{
								flow.InsertedAt = DateTime.Now;
								flow.InsertedBy = UserId;
								flow.UpdatedAt = DateTime.Now;
								flow.UpdatedBy = UserId;
								flow.ApprovalID = dataExist.Id;
								flow.IsFinalStep = flow.Step == flows.Count() ? true : false;
								//delete item from request if exsist in database
								ApprovalFlows existFlow = await context.ApprovalFlows.FirstOrDefaultAsync(d => d.ApprovalID == dataExist.Id && d.ApproverUserID == flow.ApproverUserID);
								//if (existFlow != null)
								//{
								//	flows.Remove(flow);
								//}
								if (existFlow == null)
								{
									NewFlowUser.Add(flow);
								}
							}
						}
						else
						{
							approval.InsertedAt = DateTime.Now;
							approval.InsertedBy = UserId;
							approval.UpdatedBy = UserId;
							approval.UpdatedAt = DateTime.Now;
							approval.MaxStep = flows.Count();
							await context.Approvals.AddAsync(approval);
							await context.SaveChangesAsync();
							dataExist = approval;

							ApprovalActivities activity = new ApprovalActivities()
							{
								ApprovalID = approval.Id,
								CurrentStep = 0,
								NextStep = 1,
								ApprovalActivity = ApprovalActivity.Draft,
								ApprovalActivityName = "Save as draft",
								InsertedAt = DateTime.Now,
								InsertedBy = UserId,
								Remark = "Add Workflow"
							};
							await context.ApprovalActivities.AddAsync(activity);
							await context.SaveChangesAsync();

							foreach (var flow in flows)
							{
								flow.InsertedAt = DateTime.Now;
								flow.InsertedBy = UserId;
								flow.UpdatedAt = DateTime.Now;
								flow.UpdatedBy = UserId;
								flow.ApprovalID = dataExist.Id;
							}

							await context.ApprovalFlows.AddRangeAsync(flows);
							await context.SaveChangesAsync();
						}

						if (NewFlowUser.Count > 0)
						{
							await context.ApprovalFlows.AddRangeAsync(NewFlowUser);
							await context.SaveChangesAsync();
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

		public async Task<int> DeleteWorkFlow(int documentId)
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
						approval = await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == documentId);
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

		public async Task<int> DeleteWorkFlowUser(int approvalId, int userId)
		{
			int result = 0;
			int DocumentID = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						ApprovalFlows flow = new ApprovalFlows();
						flow = await context.ApprovalFlows.FirstOrDefaultAsync(x => x.ApprovalID == approvalId && x.ApproverUserID == userId);
						if (flow != null)
						{
							context.ApprovalFlows.Remove(flow);
							context.SaveChanges();
						}

						Approvals ap = await context.Approvals.FirstOrDefaultAsync(x => x.Id == approvalId);
						DocumentID = (int)ap.DocumentID;

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
			return DocumentID;
		}

		public async Task<Approvals> CheckApprovalStatusDocument(int documentId)
		{
			return await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == documentId);
		}

		public async Task<ResponseSearchDocument> SearchListDocument(string SearchText, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);
			var userGroupIds = await context.UserGroup
					.Where(ug => ug.UserId == UserId)
					.Select(ug => ug.GroupId)
					.ToListAsync();

			var sharedCat = context.CategoriesShared
				.Where(cs => (cs.UserID == UserId && cs.ShareType == "user") ||
							 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value)))
				.Select(x => x.CategoryID);

			var sharedDocId = context.DocumentSharedPrivillege
				.Where(x => (x.UserID == UserId && x.ShareType == "user") ||
							(x.GroupID != null && x.ShareType == "group" && userGroupIds.Contains(x.GroupID.Value)))
				.Select(x => x.DocumentShared.DocumentID);


			var query = context.Documents.Include(x => x.OwnerInfo)
				.Where(x => x.IsActive == true && (x.Owner == UserId || sharedCat.Contains(x.CategoryID) || sharedDocId.Contains(x.Id)))
				.WhereIf(!string.IsNullOrEmpty(SearchText), x => (x.DocumentTitle.ToLower().Contains(SearchText.ToLower()) || x.DocumentDesc.Contains(SearchText.ToLower())))
				.Select(x => new
				{
					// Fetch raw data first
					Doc = x,
					CategoryName = x.Categories.CategoryName,
					OwnerName = x.OwnerInfo.UserName,
					InsertedByUserName = x.InsertedBy, // Assuming navigation properties exist
					InsertedByFullName = x.OwnerInfo.FullName,
					UpdatedByUserName = x.UpdatedByUserName,
					UpdatedByFullName = x.UpdatedByFullName,
					IsFavorite = context.DocumentFavorites.Any(f => f.DocumentId == x.Id && f.Owner == UserId),
					MainFileType = x.DocumentFiles.Where(me => me.IsMainDocumentFile).Select(me => me.DocumentType).FirstOrDefault()
				});

			ResponseSearchDocument rdoc = new ResponseSearchDocument() { TotalRecord = query.Count() };
			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			var rawResults = await query.ToListAsync();

			var finalResults = rawResults.Select(x => new DocumentList()
			{
				CategoryID = x.Doc.CategoryID,
				CategoryName = x.CategoryName,
				DocumentId = x.Doc.Id,
				DocumentName = x.Doc.DocumentTitle,
				Description = x.Doc.DocumentDesc,
				Owner = x.Doc.Owner,
				OwnerName = x.OwnerName,
				FilesSize = SizeFormatter.SizeSuffix((long)x.Doc.FileSize, 2), // Format in memory here!
				InsertedAt = x.Doc.InsertedAt,
				IsFavorite = x.IsFavorite,
				FileType = x.MainFileType,
				LastUpdateDate = x.Doc.UpdatedAt,
				UpdatedAt = x.Doc.UpdatedAt,
				UpdatedBy = x.Doc.UpdatedBy,
				InsertedByUserName = x.Doc.InsertedByUserName,
				InsertedByFullName = x.Doc.InsertedByFullName,
				UpdatedByUserName = x.Doc.UpdatedByUserName,
				UpdatedByFullName = x.Doc.UpdatedByFullName
			}).ToList();

			//if (!string.IsNullOrWhiteSpace(orderBy))
			//	query = query.OrderBy(x => $"x.{orderBy} {orderOrientation}");



			rdoc.Records = finalResults;
			return rdoc;
		}

		public async Task<ResponseSearchDocument> AdvSearchDocument(string DocumentTitle, DateTime? DtFrom, DateTime? DtTo, int? CategoryId, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);

			var userGroupIds = await context.UserGroup
					.Where(ug => ug.UserId == UserId)
					.Select(ug => ug.GroupId)
					.ToListAsync();

			var sharedCat = context.CategoriesShared
				.Where(cs => (cs.UserID == UserId && cs.ShareType == "user") ||
							 (cs.GroupID != null && cs.ShareType == "group" && userGroupIds.Contains(cs.GroupID.Value)))
				.Select(x => x.CategoryID);

			var sharedDocId = context.DocumentSharedPrivillege
				.Where(x => (x.UserID == UserId && x.ShareType == "user") ||
							(x.GroupID != null && x.ShareType == "group" && userGroupIds.Contains(x.GroupID.Value)))
				.Select(x => x.DocumentShared.DocumentID);


			var query = context.Documents.Include(x => x.OwnerInfo).Include(x => x.DocumentFiles)
				.Where(x => x.IsActive == true && (x.Owner == UserId || sharedCat.Contains(x.CategoryID) || sharedDocId.Contains(x.Id)))
				.Select(x => new
				{
					// Fetch raw data first
					Doc = x,
					CategoryName = x.Categories.CategoryName,
					OwnerName = x.OwnerInfo.UserName,
					InsertedByUserName = x.InsertedBy, // Assuming navigation properties exist
					InsertedByFullName = x.OwnerInfo.FullName,
					UpdatedByUserName = x.UpdatedByUserName,
					UpdatedByFullName = x.UpdatedByFullName,
					IsFavorite = context.DocumentFavorites.Any(f => f.DocumentId == x.Id && f.Owner == UserId),
					MainFileType = x.DocumentFiles.Where(me => me.IsMainDocumentFile).Select(me => me.DocumentType).FirstOrDefault()
				});


			//// Build base query with essential includes
			//var query = context.Documents
			//	.AsNoTracking() // Add this to improve performance for read-only queries
			//	.Include(x => x.Categories)
			//	.Include(x => x.DocumentFiles)
			//	.Include(x => x.OwnerInfo)
			//	.Where(x => x.Owner == UserId && x.IsActive == true);

			// Apply search filters
			if (!string.IsNullOrEmpty(DocumentTitle))
			{
				query = query.Where(x =>
					x.Doc.DocumentTitle.ToLower().Contains(DocumentTitle.ToLower()) ||
					x.Doc.DocumentDesc.ToLower().Contains(DocumentTitle.ToLower()) ||
					x.Doc.DocumentFiles.Any(f => f.SearchVector.Matches(EF.Functions.PlainToTsQuery("english", DocumentTitle)))
				);
			}

			if (DtFrom.HasValue && DtTo.HasValue)
			{
				query = query.Where(x => x.Doc.UpdatedAt.Date >= DtFrom.Value.Date && x.Doc.UpdatedAt.Date <= DtTo.Value.Date);
			}

			if (CategoryId.HasValue)
			{
				query = query.Where(x => x.Doc.CategoryID == CategoryId);
			}

			// Get document sharing privileges in a separate query
			var documentPrivileges = await context.DocumentSharedPrivillege
				.AsNoTracking()
				.Include(x => x.DocumentShared)
				.Where(x => x.UserID == UserId)
				.ToDictionaryAsync(x => x.DocumentShared.DocumentID);

			// Get favorites in a separate query
			var favorites = new HashSet<int>(await context.DocumentFavorites
				.AsNoTracking()
				.Where(x => x.Owner == UserId)
				.Select(x => x.DocumentId)
				.ToListAsync());

			// Get total count before pagination
			var totalCount = await query.CountAsync();

			// Apply pagination
			var documents = await query
				.OrderByDescending(x => x.Doc.UpdatedAt)
				.Skip(skip)
				.Take(limit)
				.Select(x => new DocumentList
				{
					CategoryID = x.Doc.CategoryID,
					CategoryName = x.Doc.Categories.CategoryName,
					DocumentId = x.Doc.Id,
					DocumentName = x.Doc.DocumentTitle,
					Description = x.Doc.DocumentDesc,
					Owner = x.Doc.Owner,
					OwnerName = x.Doc.OwnerInfo.UserName,
					LastUpdateDate = x.Doc.UpdatedAt,
					FilesSize = SizeFormatter.SizeSuffix((long)x.Doc.FileSize, 2),
					InsertedAt = x.Doc.InsertedAt,
					InsertedBy = x.Doc.InsertedBy,
					UpdatedAt = x.Doc.UpdatedAt,
					UpdatedBy = x.Doc.UpdatedBy,
					FileType = x.Doc.DocumentFiles != null ? x.Doc.DocumentFiles.FirstOrDefault(f => f.IsMainDocumentFile).DocumentType : null,
					InsertedByUserName = x.Doc.OwnerInfo.UserName,
					InsertedByFullName = x.Doc.OwnerInfo.FullName,
					UpdatedByUserName = x.Doc.OwnerInfo.UserName,
					UpdatedByFullName = x.Doc.OwnerInfo.FullName,
					IsFavorite = favorites.Contains(x.Doc.Id)
				})
				.ToListAsync();

			// Add privileges to results
			//foreach (var doc in documents)
			//{
			//	if (documentPrivileges.TryGetValue(doc.DocumentId, out var priv))
			//	{
			//		doc.Privillege = new RDocumentSharedPrivillege
			//		{
			//			IsView = priv.IsView,
			//			IsEdit = priv.IsEdit,
			//			IsDelete = priv.IsDelete
			//		};
			//	}
			//}

			return new ResponseSearchDocument
			{
				TotalRecord = totalCount,
				Records = documents
			};

			//if (!string.IsNullOrWhiteSpace(orderBy))
			//	query = query.OrderBy(x => $"x.{orderBy} {orderOrientation}");

			//ResponseSearchDocument rdoc = new ResponseSearchDocument() { TotalRecord = query.Count() };

			//if (page > 0 && limit > 0)
			//	query = query.Skip(skip).Take(limit);

			//rdoc.Records = await query.ToListAsync();
			//return rdoc;
		}

		public async Task<List<DropdownTextValue>> GetDropdownDocument(string search, int[] excludedIds)
		{
			var users = await context.Documents.Where(x => x.IsActive == true)
			.WhereIf(UserId > 0, x => x.Owner == UserId)
			.WhereIf(!string.IsNullOrWhiteSpace(search), x => x.DocumentTitle.ToLower().Contains(search.ToLower()))
			.WhereIf(excludedIds != null, record => !excludedIds.Contains(record.Id))

			.Select(x => new DropdownTextValue { Text = x.DocumentTitle, Value = x.Id }).ToListAsync();
			return users;
		}

		public async Task<int> SaveInfoRelatedDocument(List<DocumentRelated> relateddoc, int DocumentId)
		{
			int result = 0;
			if (relateddoc.Count > 0)
			{
				//if there is related document, insert to reletedDocumentIds
				//before that check if the related document already exist in the database
				//if in databse is exist more than param from relateddoc, then delete the existing related document
				var existingRelated = await context.RelatedDocuments.Where(d => d.DocumentID == DocumentId).ToListAsync();
				var existRemoved = existingRelated.Where(x => !relateddoc.Contains(x));
				if (existRemoved.Count() > 0)
				{
					context.RelatedDocuments.RemoveRange(existRemoved);
					context.SaveChanges();
				}

				context.RelatedDocuments.AddRange(relateddoc);
				result += await context.SaveChangesAsync();
			}
			else
			{
				var existingRelated = await context.RelatedDocuments.Where(d => d.DocumentID == DocumentId).ToListAsync();
				if (existingRelated.Count() > 0)
				{
					context.RelatedDocuments.RemoveRange(existingRelated);
					result += context.SaveChanges();
				}
			}
			return result;
		}

		public async Task<bool> MarkAsDeletedAsync(int documentId)
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{
					try
					{
						var filesEntity = await context.DocumentFiles.Where(x => x.Id == documentId).ToListAsync();
						foreach (var item in filesEntity)
						{
							item.IsActive = false;
							item.UpdatedAt = DateTime.Now;
							item.UpdatedBy = UserId;
							context.DocumentFiles.Update(item);
							result += await context.SaveChangesAsync();
						}

						var entity = await context.Documents.FirstOrDefaultAsync(x => x.Id == documentId && x.IsActive == true);
						if (entity != null)
						{
							entity.IsActive = false;
							entity.UpdatedAt = DateTime.Now;
							entity.UpdatedBy = UserId;
							context.Update(entity);
							result += await context.SaveChangesAsync();
						}

						var notification = new Notifications
						{
							Id = Guid.NewGuid(),
							DocumentID = entity.Id,
							NotificationType = (short)NotificationType.DocumentExp,
							NotifDescription = $"Document {entity.DocumentTitle} has deleted",
							NotifAction = "View",
							TargetActor = entity.Owner,
							NotifContent = JsonSerializer.Serialize(new
							{
								DocumentId = entity.Id,
								Message = "Deletion of document"
							})
						};
						await context.Notifications.AddAsync(notification);
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

			return result > 0;
		}

		public async Task<bool> MarkAsUnDeletedAsync(int documentId)
		{
			int result = 0;
			var entity = await context.Documents.Include(x =>x.DocumentFiles.Where(x => !x.IsActive)).FirstOrDefaultAsync(x => x.Id == documentId);
			if (entity != null)
			{
				entity.IsActive = true;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Update(entity);

				foreach (var file in entity.DocumentFiles)
				{
					file.IsActive = true;
					file.UpdatedAt = DateTime.Now;
					file.UpdatedBy = UserId;
					context.Update(file);
				}
			}
			result += await context.SaveChangesAsync();

			return result > 0;
		}

		public async Task<int> DeleteAsync(int documentId)
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			return await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				try
				{
					var utcNow = DateTime.Now;

					var recordsToDelete = await context.Documents.FirstOrDefaultAsync(x => x.Id == documentId && x.IsActive != true);
					if (recordsToDelete == null)
					{
						return 1;
					}

					// Document Logs
					var log = await context.DocumentLog.Where(x => x.DocumentID == documentId).ToListAsync();
					foreach (var item in log)
					{
						item.IsDeleted = true;
						item.DeletedBy = UserId;
						item.DeletedAt = utcNow;

						context.DocumentLog.Update(item);
						result += context.SaveChanges();
						//result += context.DocumentLog.Update(item);
					}


					// Approvals and related activities
					var approval = await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == documentId && x.CategoryID != null);
					if (approval != null)
					{
						var activities = await context.ApprovalActivities.Where(x => x.ApprovalID == approval.Id).ToListAsync();
						foreach (var activity in activities)
						{
							activity.IsDeleted = true;
							activity.DeletedBy = UserId;
							activity.DeletedAt = utcNow;
							context.ApprovalActivities.Update(activity);
							result += context.SaveChanges();
						}

						var flows = await context.ApprovalFlows.Where(x => x.ApprovalID == approval.Id).ToListAsync();
						foreach (var flow in flows)
						{
							flow.IsDeleted = true;
							flow.DeletedBy = UserId;
							flow.DeletedAt = utcNow;
							context.ApprovalFlows.Update(flow);
							result += context.SaveChanges();
						}


						approval.IsDeleted = true;
						approval.DeletedBy = UserId;
						approval.DeletedAt = utcNow;
						context.Approvals.Update(approval);
						result += context.SaveChanges();
					}

					// Document Attributes
					var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == documentId).ToListAsync();
					foreach (var attr in docAttr)
					{
						attr.IsDeleted = true;
						attr.DeletedBy = UserId;
						attr.DeletedAt = utcNow;

						context.DocumentAttributes.Update(attr);
						result += context.SaveChanges();
					}

					// Document Favorites
					var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == documentId).ToListAsync();
					foreach (var fav in docFavs)
					{
						fav.IsDeleted = true;
						fav.DeletedBy = UserId;
						fav.DeletedAt = utcNow;

						context.DocumentFavorites.Update(fav);
						result += context.SaveChanges();
					}


					var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == documentId).ToListAsync();
					foreach (var fl in docFiles)
					{
						fl.IsDeleted = true;
						fl.DeletedBy = UserId;
						fl.DeletedAt = utcNow;

						context.DocumentFiles.Update(fl);
						result += await context.SaveChangesAsync();
					}

					// Document Item List
					var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == documentId).ToListAsync();
					foreach (var item in docItemList)
					{
						item.IsDeleted = true;
						item.DeletedBy = UserId;
						item.DeletedAt = utcNow;

						context.DocumentItemList.Update(item);
						result += context.SaveChanges();
					}

					// Related Documents
					var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == documentId).ToListAsync();
					foreach (var related in docRelated)
					{
						related.IsDeleted = true;
						related.DeletedBy = UserId;
						related.DeletedAt = utcNow;

						context.RelatedDocuments.Update(related);
						result += context.SaveChanges();
					}

					// Document Reminders
					var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == documentId).ToListAsync();
					foreach (var reminder in docReminders)
					{
						reminder.IsDeleted = true;
						reminder.DeletedBy = UserId;
						reminder.DeletedAt = utcNow;

						context.DocumentReminders.Update(reminder);
						result += context.SaveChanges();
					}

					// Document Shared and Privileges
					var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == documentId);
					if (docShared != null)
					{
						var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
						foreach (var priv in sharePriv)
						{
							priv.IsDeleted = true;
							priv.DeletedBy = UserId;
							priv.DeletedAt = utcNow;

							context.DocumentSharedPrivillege.Update(priv);
							result += context.SaveChanges();
						}

						docShared.IsDeleted = true;
						docShared.DeletedBy = UserId;
						docShared.DeletedAt = utcNow;
						context.DocumentShared.Update(docShared);
						result += context.SaveChanges();
					}

					// Document itself
					recordsToDelete.IsDeleted = true;
					recordsToDelete.DeletedBy = UserId;
					recordsToDelete.DeletedAt = utcNow;
					context.Documents.Update(recordsToDelete);
					result += context.SaveChanges();

					transaction.Commit();
					return result;
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					return 0;
					//throw;
				}
			});
			///return result;
		}

		public async Task<int> DeleteAsyncOld(int documentId)
		{
			int result = 0;
			using var transaction = context.Database.BeginTransaction();
			{
				try
				{
					var recordsToDelete = await context.Documents.FirstOrDefaultAsync(x => x.Id == documentId && x.IsActive != true);
					if (recordsToDelete == null)
					{
						return 1;
					}

					var log = await context.DocumentLog.Where(x => x.DocumentID == documentId).ToListAsync();
					context.DocumentLog.RemoveRange(log);
					result += context.SaveChanges();

					var approval = await context.Approvals.FirstOrDefaultAsync(x => x.DocumentID == documentId && x.CategoryID != null);
					if (approval != null)
					{
						var activities = await context.ApprovalActivities.Where(x => x.ApprovalID == approval.Id).ToListAsync();
						context.ApprovalActivities.RemoveRange(activities);
						result += context.SaveChanges();

						var flows = await context.ApprovalFlows.Where(x => x.ApprovalID == approval.Id).ToListAsync();
						context.ApprovalFlows.RemoveRange(flows);
						result += context.SaveChanges();

						context.Approvals.Remove(approval);
						result += context.SaveChanges();
					}

					var docAttr = await context.DocumentAttributes.Where(x => x.DocumentID == documentId).ToListAsync();
					context.DocumentAttributes.RemoveRange(docAttr);
					result += context.SaveChanges();

					var docFavs = await context.DocumentFavorites.Where(x => x.DocumentId == documentId).ToListAsync();
					context.DocumentFavorites.RemoveRange(docFavs);
					result += context.SaveChanges();

					var model = await context.Categories.FirstOrDefaultAsync(x => x.Id == recordsToDelete.CategoryID);
					var docFiles = await context.DocumentFiles.Where(x => x.DocumentID == documentId).Include(x => x.InsertedByUser).ToListAsync();
					foreach (var fl in docFiles)
					{
						var theFile = DocumentFilesHelper.GetPhysicalPathForDocumentFile(fl.InsertedBy, fl.InsertedByUser.UserName, model.Id, model.CategoryName, fl.NewDocumentFileName);
						if (File.Exists(theFile))
						{
							File.Delete(theFile);
						}
					}
					context.DocumentFiles.RemoveRange(docFiles);
					result += await context.SaveChangesAsync();

					var docItemList = await context.DocumentItemList.Where(x => x.DocumentID == documentId).ToListAsync();
					context.DocumentItemList.RemoveRange(docItemList);
					result += context.SaveChanges();

					var docRelated = await context.RelatedDocuments.Where(x => x.DocumentID == documentId).ToListAsync();
					context.RelatedDocuments.RemoveRange(docRelated);
					result += context.SaveChanges();

					var docReminders = await context.DocumentReminders.Where(x => x.DocumentID == documentId).ToListAsync();
					context.DocumentReminders.RemoveRange(docReminders);
					result += context.SaveChanges();

					var docShared = await context.DocumentShared.FirstOrDefaultAsync(x => x.DocumentID == documentId);
					if (docShared == null)
					{
						var sharePriv = await context.DocumentSharedPrivillege.Where(x => x.DocumentSharedID == docShared.Id).ToListAsync();
						context.DocumentSharedPrivillege.RemoveRange(sharePriv);
						result += context.SaveChanges();

						context.DocumentShared.Remove(docShared);
						result += context.SaveChanges();
					}

					context.Documents.Remove(recordsToDelete);
					result += context.SaveChanges();

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

		public async Task<ResponseCategoryListItem> GetInfoCategoryDocumentAsync(int DocumentId)
		{
			return await context.Documents.Include(x => x.OwnerInfo).Include(x => x.Categories).Where(x => x.Id == DocumentId && x.IsActive == true)
				.Select(x => new ResponseCategoryListItem
				{
					Id = x.CategoryID,
					CategoryName = x.Categories.CategoryName,
					CategoryDesc = x.Categories.CategoryDesc,
					Owner = x.Owner,
					OwnerFullName = x.OwnerInfo.FullName,
					OwnerUserName = x.OwnerInfo.UserName,
					LastUpdateDate = x.UpdatedAt,
					InsertedAt = x.InsertedAt
				})
				.FirstOrDefaultAsync();
		}

		public async Task<bool> CheckOwnershipAndPrivilegesAsync(int documentId)
		{
			// Get the document and its owner
			var document = await context.Documents
				.AsNoTracking()
				.FirstOrDefaultAsync(d => d.Id == documentId);

			if (document == null)
				return false;

			if (document.Owner == UserId)
				return true;

			// Find the DocumentSharedPrivillege for the owner and document
			var priv = await context.DocumentSharedPrivillege
				.AsNoTracking()
				.FirstOrDefaultAsync(p =>
					p.DocumentShared.DocumentID == documentId &&
					p.UserID == UserId);
			//p.ApproverUserID == document.Owner);

			// Check if all privileges are true
			return priv != null && priv.IsView && priv.IsEdit && priv.IsDelete;
		}

		public async Task<int> PurgeDeletedDocuments(int batchSize, int purgingInMonth)
		{
			int deletedCount = 0;
			var cutoff = DateTime.Now.AddMonths(-purgingInMonth);

			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				// deferred query for eligible documents
				var baseQuery = context.Documents
					.Where(d => d.IsDeleted == true && d.DeletedAt.HasValue && d.DeletedAt.Value <= cutoff)
					.OrderBy(d => d.Id);

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

					var docIds = batch.Select(d => d.Id).Distinct().ToList();
					var categoryIds = batch.Select(d => d.CategoryID).Distinct().ToList();

					using var transaction = await context.Database.BeginTransactionAsync();
					try
					{
						// prefetch categories for file path resolution
						var categories = await context.Categories
							.Where(c => categoryIds.Contains(c.Id))
							.ToDictionaryAsync(c => c.Id);

						// Document files -> delete physical files first
						var files = await context.DocumentFiles.Where(f => docIds.Contains(f.DocumentID)).Include(x => x.InsertedByUser).ToListAsync();
						foreach (var f in files)
						{
							try
							{
								if (!string.IsNullOrEmpty(f.NewDocumentFileName))
								{
									var categoryId = batch.FirstOrDefault(d => d.Id == f.DocumentID)?.CategoryID;
									Categories cat = null;
									categories.TryGetValue(categoryId.Value, out cat);
									var path = DocumentFilesHelper.GetPhysicalPathForDocumentFile(f.InsertedBy, f.InsertedByUser.UserName, categoryId.Value, cat.CategoryName.Trim(), f.NewDocumentFileName);

									if (File.Exists(path))
										File.Delete(path);
								}
							}
							catch
							{
								// ignore filesystem errors so DB cleanup proceeds
							}
						}

						// Remove children in a safe order
						var logs = await context.DocumentLog.Where(x => docIds.Contains(x.DocumentID)).ToListAsync();
						if (logs.Count > 0) context.DocumentLog.RemoveRange(logs);

						var approvals = await context.Approvals.Where(a => a.DocumentID != null && docIds.Contains(a.DocumentID.Value)).ToListAsync();
						if (approvals.Count > 0)
						{
							var approvalIds = approvals.Select(a => a.Id).ToList();

							var activities = await context.ApprovalActivities.Where(a => approvalIds.Contains(a.ApprovalID)).ToListAsync();
							if (activities.Count > 0) context.ApprovalActivities.RemoveRange(activities);

							var flows = await context.ApprovalFlows.Where(f => approvalIds.Contains(f.ApprovalID)).ToListAsync();
							if (flows.Count > 0) context.ApprovalFlows.RemoveRange(flows);

							context.Approvals.RemoveRange(approvals);
						}

						var attrs = await context.DocumentAttributes.Where(x => docIds.Contains(x.DocumentID)).ToListAsync();
						if (attrs.Count > 0) context.DocumentAttributes.RemoveRange(attrs);

						var favs = await context.DocumentFavorites.Where(x => docIds.Contains(x.DocumentId)).ToListAsync();
						if (favs.Count > 0) context.DocumentFavorites.RemoveRange(favs);

						if (files.Count > 0) context.DocumentFiles.RemoveRange(files);

						var items = await context.DocumentItemList.Where(x => docIds.Contains(x.DocumentID)).ToListAsync();
						if (items.Count > 0) context.DocumentItemList.RemoveRange(items);

						var related = await context.RelatedDocuments.Where(x => docIds.Contains(x.DocumentID) || docIds.Contains(x.RelatedDocumentID)).ToListAsync();
						if (related.Count > 0) context.RelatedDocuments.RemoveRange(related);

						var reminders = await context.DocumentReminders.Where(x => docIds.Contains(x.DocumentID)).ToListAsync();
						if (reminders.Count > 0) context.DocumentReminders.RemoveRange(reminders);

						var sharedEntries = await context.DocumentShared.Where(ds => docIds.Contains(ds.DocumentID)).ToListAsync();
						if (sharedEntries.Count > 0)
						{
							var sharedIds = sharedEntries.Select(s => s.Id).ToList();
							var sharePrivs = await context.DocumentSharedPrivillege.Where(p => sharedIds.Contains(p.DocumentSharedID)).ToListAsync();
							if (sharePrivs.Count > 0) context.DocumentSharedPrivillege.RemoveRange(sharePrivs);

							context.DocumentShared.RemoveRange(sharedEntries);
						}

						var notifs = await context.Notifications.Where(n => n.DocumentID != null && docIds.Contains(n.DocumentID.Value)).ToListAsync();
						if (notifs.Count > 0) context.Notifications.RemoveRange(notifs);

						// Finally remove the document rows for this batch
						context.Documents.RemoveRange(batch);

						// persist changes for this page
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


		private async Task<List<ResponseDocumentList>> SetInfoApproval(List<ResponseDocumentList> list)
		{
			// fetch approvals for the documents returned (prefer document-level approval, fallback to category-level)
			if (list != null && list.Count > 0)
			{
				var docIds = list.Select(d => d.DocumentId).Distinct().ToList();
				var catIds = list.Select(d => d.CategoryID).Distinct().Where(id => id > 0).ToList();

				var approvals = await context.Approvals
					.Where(a =>
						(a.DocumentID != null && docIds.Contains(a.DocumentID.Value)) ||
						(a.DocumentID == null && a.CategoryID != null && catIds.Contains(a.CategoryID.Value))
					)
					.ToListAsync();

				foreach (var item in list)
				{
					// prefer approval tied to the document, otherwise use category-level approval (DocumentID == null)
					var approval = approvals.FirstOrDefault(a => a.DocumentID != null && a.DocumentID.Value == item.DocumentId)
						?? approvals.FirstOrDefault(a => a.DocumentID == null && a.CategoryID == item.CategoryID);

					if (approval != null)
					{
						// store numeric status (as string) so existing ResponseDocumentList shape is respected
						int ApprovalSts = (int)approval.Status;
						if (ApprovalSts == 1 || ApprovalSts == 3)
						{
							item.ApprovalStatus = ((Domain.Enum.ApprovalStatusEnum)approval.Status).ToString();
							item.HighlightText = ApprovalSts == 1 ? "Approval: Pending" : "Approval: Rejected";
						}
						else
						{
							item.ApprovalStatus = null;
						}
					}
					else
					{
						item.ApprovalStatus = null;
					}
				}
			}

			return list;
		}
	}
}
