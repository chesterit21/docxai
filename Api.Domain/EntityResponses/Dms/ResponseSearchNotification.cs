using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Domain.EntityRequests.Dms;

namespace Api.Domain.EntityResponses.Dms
{
	public class ResponseSearchNotification
	{
		public List<NotificationList> Records{ get; set; }
		public long TotalRecord { get; set; }		
	}
	public class NotificationList
	{
		public Guid Id { get; set; }
		public int? DocumentID { get; set; }
		public string DocumentTitle { get; set; }
		public string DocumentFileSize { get; set; }
		public short NotificationType { get; set; }
		public string NotifDescription { get; set; }
		public string NotifAction { get; set; }
		public int TargetActor { get; set; }
		public string NotifContent { get; set; }
		public int InsertedBy { get; set; }
		public DateTime? InsertedAt { get; set; }
		public string ActorUserName { get; set; }
		public string ActorFullname { get; set; }
		public string TargetActorUserName { get; set; }
		public string TargetActorFullname { get; set; }
	}
}
