using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Enum;
using Api.Domain.Formatters;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Repository;
using Api.Services.Masters;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Http;
using NPOI.POIFS.Properties;
using System.Collections.Generic;
using System.Reflection;
using static NPOI.HSSF.Util.HSSFColor;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Services.Dms
{
	public class DashboardService : BaseService
	{
		private readonly ILanguageRepository languageRepository;
		private readonly IDashboardRepository repository;
		private readonly int _UserId;

		public DashboardService(IHttpContextAccessor accessor, ILanguageRepository languageRepository,
			IDashboardRepository repository) : base(accessor, repository: languageRepository)
		{
			this.languageRepository = languageRepository;
			this.repository = repository;
			_UserId = GetUser();
		}

		public async Task<ResponseDashboardCounter> GetDashboardCounterAsync(int? UserId)
		{
			return await repository.GetDashboardCounterAsync(UserId);
		}

		public async Task<List<ResponseDashboardRecentActivity>> GetRecentActivities(int? documentId, int? userId , DateTime? startDate = null, DateTime? endDate = null, int? page = 0, int? limit = 0)
		{
			return await repository.GetRecentActivities(documentId, userId, startDate, endDate, page.Value, limit.Value);
		}
		
		public async Task<List<ResponseDashboardTop10UserMostDownloads>> GetTop10UserMostDownload(DateTime? startDate = null, DateTime? endDate = null)
		{
			return await repository.GetTop10UserMostDownload(startDate, endDate);
		}

		public async Task<List<ResponseDashboardTop10UserMostStorage>> ResponseDashboardTop10UserMostStorages(DateTime? startDate = null, DateTime? endDate = null)
		{
			return await repository.ResponseDashboardTop10UserMostStorages(startDate, endDate);
		}

		public async Task<List<ResponseDocumentMonthlyGrowth>> GetDocumentGrowthByMonth(int year, int? userId)
		{
			if (year == 0)
			{
				var message = await GetMessage(LangCodes.InputEmpty);
				throw new ApiException(message);
			}
			if(userId == null)
				userId = _UserId;
			//await ValidateInputAsync(year > 2000, "Invalid Year");
			return await repository.GetDocumentGrowthByMonth(year, userId.Value);
		}

		public async Task<List<ResponseDocumenCountByCategory>> GetDocumentCountByCategory(int? userId, DateTime? startDate = null, DateTime? endDate = null)
		{
			if (userId == null)
				userId = _UserId;
			return await repository.GetDocumentCountByCategory(userId, startDate, endDate);
		}

		public async Task<List<ResponseDocumenCountByApprovalStatus>> GetDocumentCountByApprovalStatus(int? userId, DateTime? startDate = null, DateTime? endDate = null)
		{
			if (userId == null)
				userId = _UserId;
			return await repository.GetDocumentCountByApprovalStatus(userId.Value, startDate, endDate);
		}

		public async Task<List<ResponseDocumentExpiringCount>> GetExpiringDocumentCount(int? userId, int period, DateTime? startDate = null, DateTime? endDate = null)
		{
			if (userId == null)
				userId = _UserId;
			ExpirationPeriod period1 = (ExpirationPeriod)period;
			return await repository.GetExpiringDocumentCount(userId.Value, period1, startDate, endDate);
		}

		public async Task<List<ResponseMostActiveUsers>> GetMostActiveUsersAsync(DateTime startDate, DateTime endDate, int topN = 10)
		{
			return await repository.GetMostActiveUsersAsync(startDate, endDate, topN);
		}

		public async Task<List<ResponseMostDownloadDocument>> GetMostDownloadedDocumentsAsync(DateTime startDate, DateTime endDate, int topN = 10)
		{
			return await repository.GetMostDownloadedDocumentsAsync(startDate, endDate, topN);
		}

		public async Task<List<ResponseMostViewedDocument>> GetMostViewedDocumentsAsync(DateTime startDate, DateTime endDate, int topN = 10)
		{
			return await repository.GetMostViewedDocumentsAsync(startDate, endDate, topN);
		}
	}
}
