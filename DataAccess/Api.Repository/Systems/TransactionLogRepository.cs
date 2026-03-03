using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Systems;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.EntityResponses.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Api.Repository.Systems
{
	public interface ITransactionLogRepository : IRepository<TransactionLog>
	{
		Task<object> GetAllAsync(DateTime startDate, DateTime endDate, int page, int limit);
		Task<ResponseTransactionLogPagination> GetAsync(DateTime startDate, DateTime endDate, int page, int limit);
	}

	public class TransactionLogRepository(DataContext context, IHttpContextAccessor accessor) : Repository<TransactionLog>(context, accessor), ITransactionLogRepository
	{
		public async Task<ResponseTransactionLogPagination> GetAsync(DateTime startDate, DateTime endDate, int page, int limit)
		{
			//return await GetAsync(x => x.InsertedAt.Date >= startDate && x.InsertedAt.Date <= endDate, page, limit);
			//startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
			//endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc); //endDate.ToUniversalTime();
			var skip = Skip(page, limit);

			var query = context.TransactionLog
			.Where(x => x.InsertedAt.Date >= startDate.Date && x.InsertedAt.Date <= endDate.Date)
			.Include(s => s.Audit).ThenInclude(s => s.InsertedByUser)
			.LeftJoin(
				context.User,
				at => at.InsertedBy,
				us => us.UserId,
				(at, us) => new ResponseTransactionLog
				{
					Id = at.Id,
					IPAddress = at.IPAddress,
					UserAgent = at.UserAgent,
					Action = at.Action,
					Description = at.Description,
					Path = at.Path,
					Parameter = at.Parameter,
					InsertedAt = at.InsertedAt,
					InsertedByFullName = us.FullName,
					Audit = at.Audit == null ? null : new ResponseAuditTrail
					{
						Id = at.Audit.Id,
						Before = at.Audit.Before,
						After = at.Audit.After,
						Command = at.Audit.Command,
						InsertedAt = at.Audit.InsertedAt,
						InsertedByFullName = at.Audit.InsertedByUser != null ? at.Audit.InsertedByUser.FullName : string.Empty
					}
				})
			;
			//.Skip(skip)
			//.Take(limit);

			ResponseTransactionLogPagination responsePagination = new ResponseTransactionLogPagination()
			{
				TotalRecord = query.Count()
			};

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit).OrderByDescending(x => x.InsertedAt);


			responsePagination.Record = await query.ToListAsync();			
			return responsePagination;
		}

		public async Task<object> GetAllAsync(DateTime startDate, DateTime endDate, int page, int limit)
		{
			var skip = Skip(page, limit);

			return await context.TransactionLog
			.Where(x => x.InsertedAt >= startDate && x.InsertedAt <= endDate).Include(s => s.Audit)
			.LeftJoin(
				context.User,
				at => at.InsertedBy,
				us => us.UserId,
				(at, us) => new
				{
					at.Id,
					at.IPAddress,
					at.UserAgent,
					at.Action,
					at.Description,
					at.Path,
					at.Parameter,
					at.InsertedBy,
					at.InsertedAt,
					us.UserName,
					us.FullName,
					at.Audit
				})
			.OrderByDescending(x => x.InsertedAt)
			.Skip(skip)
			.Take(limit)
			.ToListAsync();
		}
	}
}
