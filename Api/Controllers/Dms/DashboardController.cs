using Api.Domain;
using Api.Domain.Attributes;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Miscellaneous;
using Api.Repository;
using Api.Services.Dms;
using Api.Services.Masters;
using Api.Services.Systems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using System.Net;

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Dashboard")]
	[Route("[controller]")]
	[ApiController]
	public class DashboardController(DashboardService service) : ControllerBase
	{
		[HttpGet]
		[Route("get-dashboard-counter")]
		public async Task<IActionResult> GetDashboardCounterAsync([FromQuery] int? userId)
		{
			var result = await service.GetDashboardCounterAsync(userId);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet]
		[Route("get-recent-activity")]
		public async Task<IActionResult> GetRecentActivities([FromQuery] RequestRecentAcvtivity filter)
		{

			//DateTime st = DateTime.Now;
			if (filter.StartDate != null)
				filter.StartDate = filter.StartDate.Value;
			//else
			//	st = st.AddDays(-1);

			//DateTime end = DateTime.Now;
			if (filter.EndDate != null)
				filter.EndDate = filter.EndDate.Value;

			var result = await service.GetRecentActivities(filter.DocumentID, filter.UserID, filter.StartDate, filter.EndDate, filter.Page, filter.Limit);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet]
		[Route("get-most-download-user")]
		public async Task<IActionResult> GetTop10UserMostDownload([FromQuery] RequestDatetimeRange filter)
		{

			//DateTime st = DateTime.Now;
			if (filter.StartDate != null)
				filter.StartDate = filter.StartDate.Value;
			//else
			//	st = st.AddDays(-1);

			//DateTime end = DateTime.Now;
			if (filter.EndDate != null)
				filter.EndDate = filter.EndDate.Value;

			var result = await service.GetTop10UserMostDownload(filter.StartDate, filter.EndDate);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet]
		[Route("get-most-storage-user")]
		public async Task<IActionResult> ResponseDashboardTop10UserMostStorages([FromQuery] RequestDatetimeRange filter)
		{

			//DateTime st = DateTime.Now;
			if (filter.StartDate != null)
				filter.StartDate = filter.StartDate.Value;
			//else
			//	st = st.AddDays(-1);

			//DateTime end = DateTime.Now;
			if (filter.EndDate != null)
				filter.EndDate = filter.EndDate.Value;

			var result = await service.ResponseDashboardTop10UserMostStorages(filter.StartDate, filter.EndDate);
			return ResultFactory.Create(result);
		}

		[HttpGet]
		[Route("document-growth-over-time")]
		public async Task<IActionResult> GetDocumentGrowthByMonth([FromQuery] RequestDocumentGrowthOverTime filter)
		{
			var result = await service.GetDocumentGrowthByMonth(filter.Year, filter.UserId);
			return ResultFactory.Create(result);
		}

		[HttpGet]
		[Route("document-count-by-category")]
		public async Task<IActionResult> GetDocumentByCategory([FromQuery] RequestDocumentCountByCategory filter)
		{
			if (filter.StartDate != null)
				filter.StartDate = filter.StartDate.Value;

			if (filter.EndDate != null)
				filter.EndDate = filter.EndDate.Value;

			var result = await service.GetDocumentCountByCategory(filter.UserId, filter.StartDate, filter.EndDate);
			return ResultFactory.Create(result);
		}
		
		[HttpGet]
		[Route("document-count-approval-status")]
		public async Task<IActionResult> GetDocumentByCategory([FromQuery] RequestDocumentCountByApprovalStatus filter)
		{
			if (filter.StartDate != null)
				filter.StartDate = filter.StartDate.Value;

			if (filter.EndDate != null)
				filter.EndDate = filter.EndDate.Value;

			var result = await service.GetDocumentCountByApprovalStatus(filter.UserId, filter.StartDate, filter.EndDate);
			return ResultFactory.Create(result);
		}

		[HttpGet]
		[Route("document-expiring-by-period")]
		public async Task<IActionResult> GetExpiringDocumentCount([FromQuery] RequestDocumentExpiringCount filter)
		{
			if (filter.StartDate != null)
				filter.StartDate = filter.StartDate.Value.Date;

			if (filter.EndDate != null)
				filter.EndDate = filter.EndDate.Value.Date;

			var result = await service.GetExpiringDocumentCount(filter.UserId, filter.Period, filter.StartDate, filter.EndDate);
			return ResultFactory.Create(result);
		}


		[HttpGet]
		[Route("get-most-active-user")]
		public async Task<IActionResult> GetMostActiveUsersAsync([FromQuery] RequestDatetimeRange filter)
		{
			filter.StartDate = DateTime.Today.AddDays(-30);
			filter.EndDate = DateTime.Today;

			if (filter.StartDate != null)
				filter.StartDate = filter.StartDate.Value;

			if (filter.EndDate != null)
				filter.EndDate = filter.EndDate.Value;

			var result = await service.GetMostActiveUsersAsync(filter.StartDate.Value, filter.EndDate.Value);
			return ResultFactory.Create(result);
		}

		[HttpGet]
		[Route("get-most-download-document")]
		public async Task<IActionResult> GetMostDownloadedDocumentsAsync([FromQuery] RequestDatetimeRange filter)
		{
			filter.StartDate = DateTime.Now.AddDays(-30);
			filter.EndDate = DateTime.Today;

			if (filter.StartDate != null)
				filter.StartDate = filter.StartDate.Value;

			if (filter.EndDate != null)
				filter.EndDate = filter.EndDate.Value;

			var result = await service.GetMostDownloadedDocumentsAsync(filter.StartDate.Value, filter.EndDate.Value);
			return ResultFactory.Create(result);
		}

		[HttpGet]
		[Route("get-most-viewed-document")]
		public async Task<IActionResult> GetMostViewedDocumentsAsync([FromQuery] RequestDatetimeRange filter)
		{
			filter.StartDate = DateTime.Now.AddDays(-30);
			filter.EndDate = DateTime.Today;

			if (filter.StartDate != null)
				filter.StartDate = filter.StartDate.Value;

			if (filter.EndDate != null)
				filter.EndDate = filter.EndDate.Value;

			var result = await service.GetMostViewedDocumentsAsync(filter.StartDate.Value, filter.EndDate.Value);
			return ResultFactory.Create(result);
		}
	}
}
