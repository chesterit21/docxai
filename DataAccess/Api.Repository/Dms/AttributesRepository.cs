using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
	public interface IAttributesRepository : IRepository<Attributes>
	{
		Task<List<Attributes>> GetListAttributs();
		Task<ResponseAttributePagination> GetListAsync(string search, bool isActive, int page, int limit);
		Task<Attributes> MarkAsDeletedAsync(int AttrId);
		Task<Attributes> MarkAsUnDeletedAsync(int AttrId);
		Task<object> GetDropdownAttribute(string search);
		Task<int> DeleteAsync(int id);
		Task<bool> EmptyRecyclebin(int batchSize);
		Task<int> PurgeDeletedAttributes(int batchSize, int purgingInMonth);
	}

	public class AttributesRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Attributes>(context, accessor), IAttributesRepository
	{
		public async Task<object> GetDropdownAttribute(string search)
		{
			var users = await context.Attributtes.Where(x => x.IsActive == true)
			.WhereIf(!string.IsNullOrWhiteSpace(search), x => x.AttributeName.ToLower().Contains(search.ToLower()))

			.Select(x => new { x.AttributeName, x.AttributeElement, x.AttributeType }).ToListAsync();
			return users;
		}

		public async Task<List<Attributes>> GetListAttributs()
		{
			return await context.Attributtes.Where(x => x.IsActive == true).ToListAsync();
		}

		public async Task<ResponseAttributePagination> GetListAsync(string search, bool isActive, int page, int limit)
		{
			var skip = Skip(page, limit);
			var query = context.Attributtes
				.Where(x => x.IsActive == isActive)
				.WhereIf(!isActive, x => x.IsActive == isActive && x.IsDeleted != true)
				.WhereIf(!string.IsNullOrWhiteSpace(search), x => (x.AttributeName.ToLower().Contains(search.ToLower()) || x.AttributeElement.ToLower().Contains(search.ToLower())))
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

			ResponseAttributePagination responseAttrPagination = new ResponseAttributePagination()
			{
				TotalRecord = query.Count()
			};

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			responseAttrPagination.Record = await query.Select(c => new ResponseAttributeList()
			{
				Id = c.Entity.Id,
				AttributeName = c.Entity.AttributeName,
				AttributeElement = c.Entity.AttributeElement,
				AttributeType = c.Entity.AttributeType,
				IsActive = c.Entity.IsActive,
				
				InsertedByByFullName = c.InsertedByFullName,
				UpdatedByFullName = c.UpdatedByFullName,
				InsertedAt = c.Entity.InsertedAt,
				UpdatedAt = c.Entity.UpdatedAt,
				IsSystem = c.Entity.IsSystem
			}).ToListAsync();

			return responseAttrPagination;
		}

		public async Task<Attributes> MarkAsDeletedAsync(int attrId)
		{
			var entity = await context.Attributtes.FirstOrDefaultAsync(x => x.Id == attrId);
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

		public async Task<Attributes> MarkAsUnDeletedAsync(int attrId)
		{
			var entity = await context.Attributtes.FirstOrDefaultAsync(x => x.Id == attrId);
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

		public async Task<int> DeleteAsync(int id)
		{
			int result = 0;
			var entity = await context.Attributtes.FirstOrDefaultAsync(x => x.Id == id);
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
					var recordsToDelete = await context.Attributtes
						.Where(x => x.IsActive == false).ToListAsync();
					if (recordsToDelete.Count == 0)
					{
						return true;
					}					

					for (int i = 0; i < recordsToDelete.Count; i += batchSize)
					{
						var batch = recordsToDelete.Skip(i).Take(batchSize).ToList();
						
						foreach (var reminder in batch)
						{
							//Attributes model = reminder;
							//var docReminders = await context.Attributtes.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
							reminder.IsDeleted = true;
							reminder.DeletedBy = UserId;
							reminder.DeletedAt = DateTime.Now;
						}
						result += context.SaveChanges();

						//context.Attributtes.RemoveRange(batch);
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

		public async Task<int> PurgeDeletedAttributes(int batchSize, int purgingInMonth)
		{
			int deletedCount = 0;
			var cutoff = DateTime.Today.AddMonths(-purgingInMonth);
			var strategy = context.Database.CreateExecutionStrategy();

			await strategy.ExecuteAsync(async () =>
			{
				// build query (deferred execution)
				var query = context.Attributtes
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
						context.Attributtes.RemoveRange(batch);
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