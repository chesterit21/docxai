using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using static Api.Domain.EntityResponses.Dms.ResponseSearchDocument;
using static Api.Domain.EntityResponses.Dms.ResponseSearchNotification;

namespace Api.Repository.Masters
{
	public interface INotificationRepository : IRepository<Notifications>
	{
		Task<ResponseSearchNotification> GetListAsync(string documentTitle, DateTime? dtFrom, DateTime? dtTo, int page, int limit);
		Task<List<Notifications>> GetAsync(DateTime startDate, DateTime endDate, short approvaltTpe, int page, int limit);
		Task MarkAsRead(Guid notificationId);
	}

	public class NotificationRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Notifications>(context, accessor), INotificationRepository
	{
		public async Task<ResponseSearchNotification> GetListAsync(string documentTitle, DateTime? dtFrom, DateTime? dtTo, int page, int limit)
		{
			var skip = Skip(page, limit);
			DateTime utcDtFrom = DateTime.Now;
			DateTime utcDtTo = DateTime.Now;
			DateTime dt = DateTime.Now;

			if (dtFrom != null)
			{
				//DateTime rDatetime = dtFrom.Value;
				//utcDtFrom = new DateTime(rDatetime.Year, rDatetime.Month, rDatetime.Day, rDatetime.Hour, rDatetime.Minute, rDatetime.Second, DateTimeKind.Utc);
				utcDtFrom = dtFrom.Value;
			}

			if (dtTo != null)
			{
				//DateTime rDatetime2 = dtTo.Value;
				//utcDtTo = new DateTime(rDatetime2.Year, rDatetime2.Month, rDatetime2.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);
				utcDtTo = dtTo.Value;
			}

			var query = context.Notifications.Include(c => c.Document)
			.Where(x => x.TargetActor == UserId)
			.WhereIf(dtFrom != null && dtTo != null, x => x.InsertedAt >= utcDtFrom && x.InsertedAt <= utcDtTo)
			.LeftJoin(
				context.User,
				at => at.InsertedBy,
				us => us.UserId,
				(at, us) => new NotificationList
				{
					Id = at.Id,
					DocumentID = at.DocumentID,
					NotificationType = at.NotificationType,
					NotifDescription = at.NotifDescription,
					NotifAction = at.NotifAction,
					TargetActor = at.TargetActor,
					NotifContent = at.NotifContent,
					InsertedBy = at.InsertedBy,
					ActorUserName = us.UserName,
					ActorFullname = us.FullName,
					InsertedAt = at.InsertedAt,
					DocumentTitle = at.Document.DocumentTitle,
					DocumentFileSize = at.Document.FileSize == null ? SizeFormatter.SizeSuffix((long)0, 1) : SizeFormatter.SizeSuffix((long)at.Document.FileSize, 1)
				});

			//.OrderByDescending(x => x.InsertedAt);
			//.Skip(skip)
			//.Take(limit)
			//.ToListAsync();
			
			ResponseSearchNotification rnotif = new ResponseSearchNotification() { TotalRecord = query.Count() };
			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			rnotif.Records = await query.ToListAsync();
			return rnotif;
		}

		public async Task<List<Notifications>> GetAsync(DateTime startDate, DateTime endDate, short approvaltTpe, int page, int limit)
		{
			return await GetAsync(x => (x.InsertedAt.Date >= startDate && x.InsertedAt.Date <= endDate) && x.NotificationType == approvaltTpe, page, limit);
		}

		public async Task MarkAsRead(Guid notificationId)
		{
			var notification = await GetSingleAsync(x => x.Id == notificationId);
			if (notification != null)
			{
				await MarkAsDeletedAsync(notification);
			}
		}
	}
}