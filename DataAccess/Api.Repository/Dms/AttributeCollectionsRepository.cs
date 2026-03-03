using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityResponses.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
	public interface IAttributeCollectionsRepository : IRepository<AttributeCollections>
	{
		Task<ResponseAttributeCollectionPagination> GetListAsync(string search, bool isActive, int page, int limit);
		Task<List<AttributeCollections>> GetListAttributs();
		Task<AttributeCollections> MarkAsDeletedAsync(Guid id);
		Task<AttributeCollections> MarkAsUnDeletedAsync(Guid id);
		Task<object> GetDropdownAttributeCollection(string search);
		Task<int> DeleteAsync(Guid id);
		Task<bool> EmptyRecyclebin(int batchSize);
		Task<int> PurgeDeletedAttributeCollections(int batchSize, int purgingInMonth);
	}

	public class AttributeCollectionsRepository(DataContext context, IHttpContextAccessor accessor) : Repository<AttributeCollections>(context, accessor), IAttributeCollectionsRepository
	{
		public async Task<object> GetDropdownAttributeCollection(string search)
		{
			var users = await context.AttributeCollections.Where(x => x.IsActive == true)
			.WhereIf(!string.IsNullOrWhiteSpace(search), x => x.CollectionName.ToLower().Contains(search.ToLower()))

			.Select(x => new { x.CollectionName, x.CollectionDescription, x.AttributeElementCollection }).ToListAsync();
			return users;
		}

		public async Task<List<AttributeCollections>> GetListAttributs()
		{
			return await context.AttributeCollections.Where(x => x.IsActive == true).ToListAsync();
		}

		public async Task<ResponseAttributeCollectionPagination> GetListAsync(string search, bool isActive, int page, int limit)
		{
			var skip = Skip(page, limit);
			var query = context.AttributeCollections.Where(x => x.IsActive == isActive && !x.IsDeleted)
				.WhereIf(!string.IsNullOrWhiteSpace(search), x => (x.CollectionName.ToLower().Contains(search.ToLower()) || x.CollectionDescription.ToLower().Contains(search.ToLower())
					|| x.AttributeElementCollection.ToLower().Contains(search.ToLower())))
				.LeftJoin(context.User, left => left.InsertedBy, user => user.UserId,
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
					});

			ResponseAttributeCollectionPagination responseAttrCollectionPagination = new ResponseAttributeCollectionPagination()
			{
				TotalRecord = query.Count()
			};

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			responseAttrCollectionPagination.Record = await query.Select(c => new ResponseAttributeCollectionList()
			{
				Id = c.Entity.Id,
				CollectionName = c.Entity.CollectionName,
				CollectionDescription = c.Entity.CollectionDescription,
				AttributeElementCollection = c.Entity.AttributeElementCollection,
				InsertedByByFullName = c.InsertedByFullName,
				UpdatedByFullName = c.UpdatedByFullName,
				InsertedAt = c.Entity.InsertedAt,
				UpdatedAt = c.Entity.UpdatedAt
			}).ToListAsync();

			return responseAttrCollectionPagination;
		}

		public async Task<AttributeCollections> MarkAsDeletedAsync(Guid id)
		{
			var entity = await context.AttributeCollections.FirstOrDefaultAsync(x => x.Id == id);
			if (entity != null)
			{
				entity.IsActive = false;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Update(entity);
				await context.SaveChangesAsync();
			}
			return entity;
		}

		public async Task<AttributeCollections> MarkAsUnDeletedAsync(Guid id)
		{
			var entity = await context.AttributeCollections.FirstOrDefaultAsync(x => x.Id == id);
			if (entity != null)
			{
				entity.IsActive = true;
				entity.UpdatedAt = DateTime.Now;
				entity.UpdatedBy = UserId;
				context.Update(entity);
				await context.SaveChangesAsync();
			}
			return entity;
		}

		public async Task<int> DeleteAsync(Guid id)
		{
			int result = 0;
			var entity = await context.AttributeCollections.FirstOrDefaultAsync(x => x.Id == id);
			if (entity != null)
			{
				entity.IsDeleted = true;
				entity.DeletedAt = DateTime.Now;
				entity.DeletedBy = UserId;
				context.Update(entity);
				result += await context.SaveChangesAsync();
			}
			return result;
		}

		public async Task<bool> EmptyRecyclebin(int batchSize)
		{
			int result = 0;
			using var transaction = context.Database.BeginTransaction();
			{
				try
				{
					var recordsToDelete = await context.AttributeCollections
						.Where(x => x.IsActive == false).ToListAsync();
					if (recordsToDelete.Count == 0)
					{
						return true;
					}

					for (int i = 0; i < recordsToDelete.Count; i += batchSize)
					{
						var batch = recordsToDelete.Skip(i).Take(batchSize).ToList();

						foreach (var attc in batch)
						{
							//AttributeCollections model = attc;
							//var docReminders = await context.AttributeCollections.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
							attc.IsDeleted = true;
							attc.DeletedBy = UserId;
							attc.DeletedAt = DateTime.Now;
						}

						result += context.SaveChanges();
						//context.AttributeCollections.RemoveRange(batch);
						//result += context.SaveChanges();
					}

					transaction.Commit();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw;
				}
			}
			return result > 0;
		}

		public async Task<int> PurgeDeletedAttributeCollections(int batchSize, int purgingInMonth)
		{
			int deletedCount = 0;
			var cutoff = DateTime.Today.AddMonths(-purgingInMonth);
			var strategy = context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				var query = context.AttributeCollections
					.Where(a => a.IsDeleted == true && a.DeletedAt.HasValue && a.DeletedAt.Value <= cutoff);

				var total = await query.CountAsync();
				if (total == 0)
					return;

				var totalPages = (int)Math.Ceiling((double)total / batchSize);

				for (int page = 1; page <= totalPages; page++)
				{
					int skip = Skip(page, batchSize);
					var batch = await query.OrderBy(a => a.Id).Skip(skip).Take(batchSize).ToListAsync();
					if (batch.Count == 0) continue;

					using var transaction = await context.Database.BeginTransactionAsync();
					try
					{
						// perform permanent removal for this batch
						context.AttributeCollections.RemoveRange(batch);
						deletedCount += await context.SaveChangesAsync();

						await transaction.CommitAsync();
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