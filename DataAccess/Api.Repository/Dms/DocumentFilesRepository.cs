using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Formatters;
using Api.Extensions;
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MimeKit.Tnef;
using NPOI.POIFS.FileSystem;
using NPOI.POIFS.Properties;
using System;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Xml.Linq;

namespace Api.Repository.Masters
{
	public interface IDocumentFilesRepository : IRepository<DocumentFiles>
	{
		Task<List<ResponseDocumentFiles>> GetFileFromDocumentAsync(int DocumentId);
		Task<List<ResponseDocumentFiles>> GetFileFromCategorytAsync(int CategoryId);
		Task<int> UpdateAsync(RequestDocumentFiles entity);
		Task<int> DeleteAsync(int id);
		Task<bool> MarkAsDeletedAsync(int documentFileId);
		Task<bool> MarkAsUnDeletedAsync(int documentFileId);
		Task<ResponseDocumentFilesPagination> GetDocumentFileRecyclebinAsync(string search, int page, int limit);
		Task<bool> EmptyRecyclebin(int batchSize);
		Task<ResponseDocumentFiles> GetInfo(int DocFileId);
		Task<int> PurgeDeletedDocumentFiles(int batchSize, int purgingInMonth);
		Task<int> UpdateFileForExcelAsync(RequestDocumentExcelFile updatedFile);
	}

	public class DocumentFilesRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentFiles>(context, accessor), IDocumentFilesRepository
	{
		public async Task<List<ResponseDocumentFiles>> GetFileFromDocumentAsync(int DocumentId)
		{
			return await context.DocumentFiles.Where(d => d.DocumentID == DocumentId).Include(h => h.Documents).ThenInclude(d => d.Watermark)
			.LeftJoin(context.User,
				new[] { "UpdatedBy" }, // Outer key selector
				new[] { "UserId" }, // Inner key selector
				(files, user) => new
				{
					Files = files,
					UpdatedByUserName = user.UserName,
					UpdatedByFullName = user.FullName
				})
			.Where(x => x.Files.IsActive != false)
			.Select(f =>
				new ResponseDocumentFiles
				{
					Id = f.Files.Id,
					DocumentID = f.Files.DocumentID,
					DocumentFileName = f.Files.DocumentFileName,
					DocumentFileContent = f.Files.DocumentFileContent,
					DocumentFilePath = f.Files.DocumentFilePath,
					DocumentFileSize = SizeFormatter.SizeSuffix(f.Files.DocumentFileSize, 2),
					DocumentFileSizeInt = f.Files.DocumentFileSize,
					DocumentType = f.Files.DocumentType,
					IsMainDocumentFile = f.Files.IsMainDocumentFile,
					UpdateAt = f.Files.UpdatedAt,
					UpdatedByUserName = f.UpdatedByUserName,
					UpdatedByFullName = f.UpdatedByFullName,
					NewDocumentFileName = f.Files.NewDocumentFileName,
					WaterMark = f.Files.Documents.Watermark != null ? f.Files.Documents.Watermark.Text : ""
					//Documents = f.Documents
				}
			).ToListAsync();
		}

		public async Task<List<ResponseDocumentFiles>> GetFileFromCategorytAsync(int CategoryId)
		{
			return await context.Documents.Where(x => x.CategoryID == CategoryId).Join(context.DocumentFiles, dc => dc.Id, f => f.DocumentID, (dc, f) =>
			new { Document = dc, Files = f })
			.LeftJoin(
				context.User,
				new[] { "UpdateBy" }, // Outer key selector
				new[] { "Id" }, // Inner key selector
				(fl, user) => new
				{
					Document = fl.Document,
					Files = fl.Files,
					UpdatedByUserName = user.UserName,
					UpdatedByFullName = user.FullName
				})
			.Where(x => x.Files.IsActive != false)
			.Select(f =>
				new ResponseDocumentFiles
				{
					Id = f.Files.Id,
					DocumentID = f.Files.DocumentID,
					DocumentFileName = f.Files.DocumentFileName,
					DocumentFileContent = f.Files.DocumentFileContent,
					DocumentFilePath = f.Files.DocumentFilePath,
					DocumentFileSize = SizeFormatter.SizeSuffix(f.Files.DocumentFileSize, 2),
					DocumentFileSizeInt = f.Files.DocumentFileSize,
					DocumentType = f.Files.DocumentType,
					IsMainDocumentFile = f.Files.IsMainDocumentFile,
					UpdateAt = f.Files.UpdatedAt,
					UpdatedByUserName = f.UpdatedByUserName,
					UpdatedByFullName = f.UpdatedByFullName
					//Documents = f.Documents
				}
			)
			.ToListAsync();

			//return await context.DocumentFiles.Where(d => d.ca == DocumentIDs).Include(h => h.Documents).Select(f =>
			//	new ResponseDocumentFiles
			//	{
			//		DocumentIDs = f.DocumentIDs,
			//		DocumentFileName = f.DocumentFileName,
			//		DocumentFileContent = f.DocumentFileContent,
			//		DocumentFilePath = f.DocumentFilePath,
			//		DocumentFileSize = f.DocumentFileSize,
			//		DocumentType = f.DocumentType,
			//		IsMainDocumentFile = f.IsMainDocumentFile
			//		//Documents = f.Documents
			//	}
			//).ToListAsync();

		}

		public async Task<int> UpdateAsync(RequestDocumentFiles entity)
		{
			int result = 0;

			DateTime DateNow = DateTime.Now;
			IFormFile file = entity.file;
			var fileName = Path.GetFileNameWithoutExtension(file.FileName);
			var fileExtension = Path.GetExtension(file.FileName);

			DocumentFiles DocFileEx = await context.DocumentFiles.Include(x => x.Documents).ThenInclude(x => x.Categories).FirstOrDefaultAsync(x => x.Id == entity.Id);

			//string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			//var categoryPathFolder = Path.Combine(baseDir, "upload", $"{DocFileEx.Documents.CategoryID}-{DocFileEx.Documents.Categories.CategoryName.Trim()}");
			//var uploadPath = Path.Combine(baseDir, categoryPathFolder);
			//if (!Directory.Exists(uploadPath))
			//{
			//	Directory.CreateDirectory(uploadPath);
			//}

			string uploadPath = DocumentFilesHelper.GetPhysicalPathForCategory(
				DocFileEx.Documents.Owner,
				UserName,
				DocFileEx.Documents.CategoryID,
				DocFileEx.Documents.Categories.CategoryName,
				true);

			string exsistFilePath = Path.Combine(uploadPath, DocFileEx.NewDocumentFileName);

			string newFileName = Guid.NewGuid().ToString() + fileExtension;
			var filePath = Path.Combine(uploadPath, newFileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			string resourcePath = DocumentFilesHelper.GetResourcePathForDocumentFile(
				DocFileEx.Documents.Owner,
				UserName,
				DocFileEx.Documents.CategoryID,
				DocFileEx.Documents.Categories.CategoryName,
				newFileName);


			FileInfo fin = new FileInfo(filePath);
			DocFileEx.DocumentType = fin.Extension;
			DocFileEx.DocumentFileName = fileName;
			DocFileEx.NewDocumentFileName = fin.Name;
			DocFileEx.DocumentFilePath = resourcePath;
			DocFileEx.DocumentFileSize = (int)fin.Length;
			//DocFileEx.IsMainDocumentFile = updatedFile.IsMainDocumentFile;
			DocFileEx.UpdatedBy = UserId;
			DocFileEx.UpdatedAt = DateNow;

			context.DocumentFiles.Update(DocFileEx);
			result += await context.SaveChangesAsync();

			if (File.Exists(exsistFilePath))
			{
				File.Delete(exsistFilePath);
			}

			//remarked, change to trigger
			//Documents parent = await context.Documents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.DocumentID);
			//parent.UpdatedBy = UserId;
			//parent.UpdatedAt = DateNow;
			//int newFileSize = context.DocumentFiles.AsNoTracking().Where(x => x.DocumentID == entity.DocumentID).Sum(x => x.DocumentFileSize);
			//parent.FileSize = newFileSize;
			//var tracked = context.ChangeTracker.Entries<Documents>().FirstOrDefault(e => e.Entity.Id == parent.Id);
			//if (tracked != null)
			//	tracked.State = EntityState.Detached;
			//context.Documents.Update(parent);
			//result += await context.SaveChangesAsync();

			return result;
		}


		public async Task<int> DeleteAsync(int id)
		{
			int result = 0;

			// Fetch the file with related document and category
			var docFileEx = await context.DocumentFiles
				.Include(x => x.Documents)
				.ThenInclude(x => x.Categories)
				.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == false);

			if (docFileEx == null)
			{
				return 1; // or handle as needed
			}

			// Soft delete the file record
			docFileEx.IsDeleted = true;
			docFileEx.DeletedBy = UserId;
			docFileEx.DeletedAt = DateTime.Now;
			context.DocumentFiles.Update(docFileEx);
			result += await context.SaveChangesAsync();

			//remarked, change to trigger
			//// Update document document
			//if (docFileEx.Documents != null)
			//{
			//	//context.Entry(document).State = EntityState.Detached;
			//	Documents documents = docFileEx.Documents;
			//	documents.UpdatedBy = UserId;
			//	documents.UpdatedAt = DateTime.Now;

			//	int newFileSize = await context.DocumentFiles
			//		.Where(x => x.DocumentID == docFileEx.DocumentID && x.IsActive != false)
			//		.SumAsync(x => x.DocumentFileSize);

			//	documents.FileSize = newFileSize;

			//	// Check if no active files remain
			//	bool hasRemainingFiles = await context.DocumentFiles.AsNoTracking()
			//		.AnyAsync(x => x.DocumentID == docFileEx.DocumentID && x.IsActive != false);

			//	if (!hasRemainingFiles)
			//	{
			//		documents.IsDeleted = true;
			//		documents.DeletedBy = UserId;
			//		documents.DeletedAt = DateTime.Now;
			//	}

			//	//context.Attach(document);
			//	context.Documents.Update(documents);
			//	result += await context.SaveChangesAsync();
			//}

			return result;
		}

		public async Task<int> DeleteAsyncT(int id)
		{
			int result = 0;

			DocumentFiles DocFileEx = await context.DocumentFiles.Include(x => x.Documents).ThenInclude(x => x.Categories).FirstOrDefaultAsync(x => x.Id == id && x.IsActive == false);

			//string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			//var categoryPathFolder = Path.Combine(baseDir, "upload", $"{DocFileEx.Documents.CategoryID}-{DocFileEx.Documents.Categories.CategoryName.Trim()}");
			//var uploadPath = Path.Combine(baseDir, categoryPathFolder);
			//string exsistFilePath = Path.Combine(uploadPath, DocFileEx.NewDocumentFileName);

			string exsistFilePath = DocumentFilesHelper.GetResourcePathForDocumentFile(
				DocFileEx.Documents.Owner,
				UserName,
				DocFileEx.Documents.CategoryID,
				DocFileEx.Documents.Categories.CategoryName,
				DocFileEx.NewDocumentFileName);

			context.DocumentFiles.Remove(DocFileEx);
			result += await context.SaveChangesAsync();

			if (File.Exists(exsistFilePath))
			{
				File.Delete(exsistFilePath);
			}

			//remarked, change to trigger
			//Documents parent = context.Documents.FirstOrDefault(x => x.Id == DocFileEx.DocumentID);

			//parent.UpdatedBy = UserId;
			//parent.UpdatedAt = DateTime.Now;

			//int newFileSize = context.DocumentFiles.Where(x => x.DocumentID == DocFileEx.DocumentID).Sum(x => x.DocumentFileSize);
			//parent.FileSize = newFileSize;

			//if (parent != null)
			//{
			//	context.Entry(parent).State = EntityState.Detached;
			//}
			//context.Attach(parent);
			//context.Documents.Update(parent);
			//result += await context.SaveChangesAsync();

			return result;
		}

		public async Task<bool> MarkAsDeletedAsync(int documentFileId)
		{
			int result = 0;
			var entity = await context.DocumentFiles.FirstOrDefaultAsync(x => x.Id == documentFileId);
			if (entity != null)
			{
				entity.IsActive = false;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Update(entity);
				result += await context.SaveChangesAsync();
			}
			return result > 0;
		}

		public async Task<bool> MarkAsUnDeletedAsync(int documentFileId)
		{
			int result = 0;
			var entity = await context.DocumentFiles.FirstOrDefaultAsync(x => x.Id == documentFileId);
			if (entity != null)
			{
				entity.IsActive = true;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Update(entity);
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
						List<DocumentFiles> recordsToDelete = new List<DocumentFiles>();
						if (UserRoleInApp == "user")
						{
							recordsToDelete = await context.DocumentFiles.Include(d => d.Documents).Where(x => x.IsActive == false && x.Documents.Owner == UserId).ToListAsync();
						}
						if (UserRoleInApp == "superadmin")
						{
							recordsToDelete = await context.DocumentFiles.Where(x => x.IsActive == false).ToListAsync();
						}

						//if (recordsToDelete.Count == 0)
						//{
						//	return true;
						//}

						//List<int> DocumentIdPerBatch = new List<int>();

						for (int i = 0; i < recordsToDelete.Count; i += batchSize)
						{
							var batch = recordsToDelete.Skip(i).Take(batchSize).ToList();
							int batchCount = batch.Count();
							for (int x = 0; x < batchCount; x++)
							{
								int idDocumentFile = batch[x].Id;

								var docFile = await context.DocumentFiles.Where(x => x.Id == idDocumentFile).FirstOrDefaultAsync();

								docFile.IsDeleted = true;
								docFile.DeletedBy = UserId;
								docFile.DeletedAt = DateTime.Now;
								context.DocumentFiles.Update(docFile);
								result += context.SaveChanges();

								//if (!DocumentIdPerBatch.Any(x => x == docFile.DocumentID))
								//	DocumentIdPerBatch.Add(docFile.DocumentID);
							}

							//remarked, change to trigger
							//if (DocumentIdPerBatch.Count > 0)
							//{
							//	foreach (var docID in DocumentIdPerBatch)
							//	{
							//		Documents parent = context.Documents.FirstOrDefault(x => x.Id == docID);

							//		parent.UpdatedBy = UserId;
							//		parent.UpdatedAt = DateTime.Now;

							//		int newFileSize = context.DocumentFiles.Where(x => x.DocumentID == docID && x.IsDeleted == false).Sum(x => x.DocumentFileSize);
							//		parent.FileSize = newFileSize;

							//		if (parent != null)
							//		{
							//			context.Entry(parent).State = EntityState.Detached;
							//		}
							//		context.Attach(parent);
							//		context.Documents.Update(parent);
							//		result += await context.SaveChangesAsync();
							//	}
							//}
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

		public async Task<ResponseDocumentFilesPagination> GetDocumentFileRecyclebinAsync(string search, int page, int limit)
		{
			var skip = Skip(page, limit);
			var query = context.DocumentFiles.Include(x => x.Documents)
				.Where(x => x.IsActive == false && x.IsDeleted == false)
				.WhereIf(!string.IsNullOrWhiteSpace(search), x => x.DocumentFileName.ToLower().Contains(search.ToLower()))
				.WhereIf(UserRoleInApp == "user", src => src.Documents.Owner == UserId)
				.LeftJoin(context.User, left => left.InsertedBy, user => user.UserId,
					(left, userInsert) => new
					{
						Files = left,
						InsertedByUserName = userInsert.UserName,
						InsertedByFullName = userInsert.FullName,
						UpdatedByUserName = "",
						UpdatedByFullName = ""
					})
				.LeftJoin(context.User,
					"Files.UpdatedBy",
					"UserId",
					(left, userUpdate) => new
					{
						left.Files,
						left.InsertedByUserName,
						left.InsertedByFullName,
						UpdatedByUserName = userUpdate.UserName,
						UpdatedByFullName = userUpdate.FullName
					});

			ResponseDocumentFilesPagination responsePagination = new ResponseDocumentFilesPagination()
			{
				TotalRecord = query.Count()
			};

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			responsePagination.Record = await query.Select(f => new ResponseDocumentFiles()
			{
				Id = f.Files.Id,
				DocumentID = f.Files.DocumentID,
				DocumentFileName = f.Files.DocumentFileName,
				DocumentFileContent = f.Files.DocumentFileContent,
				DocumentFilePath = f.Files.DocumentFilePath,
				DocumentFileSize = SizeFormatter.SizeSuffix(f.Files.DocumentFileSize, 2),
				DocumentFileSizeInt = f.Files.DocumentFileSize,
				DocumentType = f.Files.DocumentType,
				IsMainDocumentFile = f.Files.IsMainDocumentFile,
				UpdateAt = f.Files.UpdatedAt,
				UpdatedByUserName = f.UpdatedByUserName,
				UpdatedByFullName = f.UpdatedByFullName,
				NewDocumentFileName = f.Files.NewDocumentFileName
			}).ToListAsync();

			return responsePagination;
		}

		public async Task<ResponseDocumentFiles> GetInfo(int DocFileId)
		{
			var res = await context.DocumentFiles.Include(x => x.Documents).ThenInclude(x => x.Categories).Include(x => x.Documents).ThenInclude(c => c.Watermark)
				.Include(x => x.Documents).ThenInclude(x => x.OwnerInfo)
				.Where(x => x.IsActive == true && x.Id == DocFileId).Select(f => new ResponseDocumentFiles()
				{
					Id = f.Id,
					DocumentID = f.DocumentID,
					DocumentFileName = f.DocumentFileName,
					DocumentFileContent = f.DocumentFileContent,
					DocumentFilePath = f.DocumentFilePath,
					DocumentFileSize = SizeFormatter.SizeSuffix(f.DocumentFileSize, 2),
					DocumentFileSizeInt = f.DocumentFileSize,
					DocumentType = f.DocumentType,
					IsMainDocumentFile = f.IsMainDocumentFile,
					UpdateAt = f.UpdatedAt,
					UpdatedByUserName = f.UpdatedByUserName,
					UpdatedByFullName = f.UpdatedByFullName,
					NewDocumentFileName = f.NewDocumentFileName,
					CategoryID = f.Documents.Categories.Id,
					CategoryName = f.Documents.Categories.CategoryName,
					WaterMark = f.Documents.Watermark != null ? f.Documents.Watermark.Text : "",
					OwnerUserName = f.Documents.OwnerInfo != null ? f.Documents.OwnerInfo.UserName : "",
					OwnerUserID = f.Documents.Owner
				}).FirstOrDefaultAsync();

			return res;
		}

		public async Task<int> PurgeDeletedDocumentFiles(int batchSize, int purgingInMonth)
		{
			int deletedCount = 0;
			var cutoff = DateTime.Now.AddMonths(-purgingInMonth);
			var strategy = context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				// deferred query with optional role filter
				var baseQuery = context.DocumentFiles
					.Include(df => df.Documents)
						.ThenInclude(d => d.Categories)
					.Where(df => df.IsDeleted == true && df.DeletedAt.HasValue && df.DeletedAt.Value <= cutoff)
					.WhereIf(UserRoleInApp == "user", df => df.Documents.Owner == UserId)
					.OrderBy(df => df.Id);

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

					var parentDocIds = batch.Select(b => b.DocumentID).Distinct().ToList();

					using var transaction = await context.Database.BeginTransactionAsync();
					try
					{
						// Attempt best-effort physical deletes first
						foreach (var df in batch)
						{
							try
							{
								var baseDir = AppDomain.CurrentDomain.BaseDirectory;
								var catId = df.Documents?.Categories?.Id ?? df.Documents?.CategoryID ?? 0;
								var catName = df.Documents?.Categories?.CategoryName?.Trim() ?? "unknown";

								var filePath = DocumentFilesHelper.GetPhysicalPathForDocumentFile(
									df.Documents.Owner,
									UserName,
									catId,
									catName,
									df.NewDocumentFileName);

								if (File.Exists(filePath))
								{
									try { File.Delete(filePath); } catch { /* ignore fs errors */ }
								}
							}
							catch
							{
								// ignore filesystem errors - continue DB cleanup
							}
						}

						// Permanently remove DB rows for this batch
						context.DocumentFiles.RemoveRange(batch);
						await context.SaveChangesAsync();

						// Update document document' filesize and mark document deleted if no remaining active files
						foreach (var docId in parentDocIds)
						{
							var parent = await context.Documents.FirstOrDefaultAsync(d => d.Id == docId);
							if (parent == null)
								continue;

							//var remainingSize = await context.DocumentFiles
							//	.Where(f => f.DocumentID == docId && f.IsDeleted == false && f.IsActive != false)
							//	.SumAsync(f => (int?)f.DocumentFileSize) ?? 0;

							//parent.FileSize = remainingSize;
							//parent.UpdatedBy = UserId;
							//parent.UpdatedAt = DateTime.Now;

							var hasRemainingFiles = await context.DocumentFiles.AnyAsync(f => f.DocumentID == docId && f.IsDeleted == false && f.IsActive != false);
							if (!hasRemainingFiles)
							{
								context.Documents.Remove(parent);
								await context.SaveChangesAsync();
							}

							
						}

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

		public async Task<int> UpdateFileForExcelAsync(RequestDocumentExcelFile updatedFile)
		{
			int result = 0;

			DocumentFiles documentFiles = null;
			var currentFile = await context.DocumentFiles
				.Include(x => x.Documents)
				.ThenInclude(x => x.Categories).FirstOrDefaultAsync(x => x.Id == updatedFile.Id);

			if (currentFile == null) return 0;
			//string uploadPath = DocumentFilesHelper.GetPhysicalPathForCategory(
			//	currentFile.Documents.Owner,
			//	UserName,
			//	currentFile.Documents.CategoryID,
			//	currentFile.Documents.Categories.CategoryName,
			//	true);

			string[] partofDocFilePath = currentFile.DocumentFilePath.Split('/');
			int partLenght = partofDocFilePath.Length;
			
			string basePath = string.Empty;
			for (int i = 1; i < (partLenght - 1); i++)
			{
				basePath += partofDocFilePath[i] + "\\";
			}

			var setting = AppSettings.Read();
			string AppUploadFolder = setting.ApplicationInfoData.AppUploadFolder;
			string uploadPath = basePath.Replace("resource\\", "");
			uploadPath = $"{AppUploadFolder}\\{uploadPath}";

			string filePath = string.Empty;
			string resourcePath = string.Empty;

			if (updatedFile.IsNewVersion)
			{
				documentFiles = new DocumentFiles();

				var fileName = currentFile.DocumentFileName;
				var fileExtension = currentFile.DocumentType;

				//string newFileName = Guid.NewGuid().ToString() + fileExtension;
				//Syncfusion.EJ2.Spreadsheet.ContentType.Xlsx
				string extEj2 = updatedFile.FileContentType.Trim().ToLower();
				string newFileName = Guid.NewGuid().ToString() + fileExtension;
				//string newFileName = Guid.NewGuid().ToString() + extEj2;
				filePath = Path.Combine(uploadPath, newFileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await updatedFile.streamOfTheFile.CopyToAsync(stream);
				}

				//resourcePath = DocumentFilesHelper.GetResourcePathForDocumentFile(
				//	currentFile.Documents.Owner,
				//	UserName,
				//	currentFile.Documents.CategoryID,
				//	currentFile.Documents.Categories.CategoryName,
				//	newFileName);
				resourcePath = $"/{basePath.Replace("\\","/")}{newFileName}";

				if (fileName.Contains("V_"))
				{
					string vrsionStr = fileName.Substring(2, 1);
					int vrsion = int.Parse(vrsionStr) + 1;
					string oriname = fileName.Replace($"V_{vrsionStr}_", "");
					documentFiles.DocumentFileName = $"V_{vrsion}_{oriname}";
				}
				else
				{
					string vrsionStr = $"V_1_{fileName}";
					documentFiles.DocumentFileName = vrsionStr;
				}
				documentFiles.InsertedBy = currentFile.InsertedBy;//replace from claim seharusnya
				documentFiles.DocumentFilePath = resourcePath;
				documentFiles.NewDocumentFileName = newFileName;
				documentFiles.DocumentID = currentFile.DocumentID;

			}
			else
			{
				int us = UserId;
				string exsistFilePath = Path.Combine(uploadPath, currentFile.NewDocumentFileName);
				using (var stream = new FileStream(exsistFilePath, FileMode.Create))
				{
					await updatedFile.streamOfTheFile.CopyToAsync(stream);
				}
				filePath = exsistFilePath;

				documentFiles = currentFile;
			}


			DateTime DateNow = DateTime.Now;
			FileInfo fin = new FileInfo(filePath);
			documentFiles.DocumentType = fin.Extension;
			documentFiles.DocumentFileSize = (int)fin.Length;
			//documentFiles.UpdatedBy = UserId;
			documentFiles.UpdatedBy = documentFiles.InsertedBy;//replace from claim seharusnya
			documentFiles.UpdatedAt = DateNow;

			if (updatedFile.IsNewVersion)
			{
				context.DocumentFiles.Add(documentFiles);
			}
			//else 
			//{	
			//	context.DocumentFiles.Update(documentFiles);
			//}
			//result += await context.SaveChangesAsync();

			//remarked, change to trigger
			//Documents document = currentFile.Documents;
			//document.UpdatedBy = UserId;
			//document.UpdatedAt = DateNow;

			//int newFileSize = await context.DocumentFiles
			//		.Where(x => x.DocumentID == document.Id)
			//		.SumAsync(x => x.DocumentFileSize);

			//document.FileSize = newFileSize;

			result += await context.SaveChangesAsync();

			return result;
		}
	}
}
