using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityResponses.Dms;
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.PTG;
using System.Collections.Generic;

namespace Api.Repository.Masters
{
	public interface IDocumentReminderRepository : IRepository<DocumentReminders>
	{
		Task<List<DocumentReminders>> GetByDocumentAsync(int? DocumentID = null);
		Task<bool> MarkAsDeletedAsync(int id);
		Task<ResponseDocumenReminderListPagination> GetRemindersByOwner(DateTime? from, DateTime? to, string search = null, int page = 0, int limit = 0);
	}

	public class DocumentReminderRepository(DataContext context, IHttpContextAccessor accessor) : Repository<DocumentReminders>(context, accessor), IDocumentReminderRepository
	{
		private readonly DbSet<DocumentReminders> dbset = context.Set<DocumentReminders>();

		public async Task<List<DocumentReminders>> GetByDocumentAsync(int? DocumentID = null)
		{
			var result = new List<DocumentReminders>();
			return await dbset.Where(x => x.DocumentID == DocumentID).ToListAsync();
		}

		public async Task<ResponseDocumenReminderListPagination> GetRemindersByOwner(DateTime? from, DateTime? to, string search, int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);

			var result = new List<DocumentReminders>();
			int ownerId = UserId;

			var query = context.DocumentReminders
				.Join(
					context.Documents.Include(x => x.OwnerInfo),
					reminder => reminder.DocumentID,
					document => document.Id,
					(reminder, document) => new { Reminder = reminder, Document = document }
				)
				.Where(x => x.Document.Owner == ownerId)
				.WhereIf(!string.IsNullOrEmpty(search), x => x.Reminder.ReminderDesc.Contains(search))
				.WhereIf(from.HasValue && to.HasValue, x => x.Reminder.ReminderDateTime.Date >= from.Value.Date && x.Reminder.ReminderDateTime.Date <= to.Value.Date)
				//.Where(x => x.Document.ExpiryDate != null &&
				//x.Reminder.ReminderDateTime.Date == x.Document.ExpiryDate.Value.Date.AddDays(-10))
				.Select(x => new ResponseDocumenReminderListList
				{
					ReminderId = x.Reminder.Id,
					ReminderDateTime = x.Reminder.ReminderDateTime,
					ReminderDesc = x.Reminder.ReminderDesc,
					DocumentId = x.Document.Id,
					DocumentTitle = x.Document.DocumentTitle,
					DocumentOwner = x.Document.OwnerInfo != null ? x.Document.OwnerInfo.FullName : null
					// Add other fields as needed
				});

			ResponseDocumenReminderListPagination response = new ResponseDocumenReminderListPagination()
			{
				TotalRecord = query.Count()
			};


			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			response.Record = await query.ToListAsync();

			return response;
		}

		public async Task<bool> MarkAsDeletedAsync(int id)
		{
			int result = 0;
			var entity = await context.DocumentReminders.FirstOrDefaultAsync(x => x.Id == id);
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
	}
}