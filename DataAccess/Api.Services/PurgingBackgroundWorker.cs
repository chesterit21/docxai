using Api.DataAccess.Models.Systems;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Api.Services.Systems;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Api.Services
{
    public class PurgingBackgroundWorker(IServiceProvider serviceProvider, ILogger<PurgingBackgroundWorker> logger,
		IDatabaseGuard _dbGuard) : BackgroundService
    {

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			logger.LogInformation("PurgingBackgroundWorker started. Will run daily at 00:01.");
			while (!stoppingToken.IsCancellationRequested)
			{
				if (await _dbGuard.IsReadyAsync(stoppingToken))
				{
					logger.LogInformation("Database is ready. Starting background processing...");
					break; // Exit the "waiting" loop
				}

				logger.LogWarning("Database not ready or config missing. Retrying in 10 seconds...");
				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}

			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					// compute next 00:01 local server time
					var now = DateTime.Now;
					var nextRun = new DateTime(now.Year, now.Month, now.Day, 0, 1, 0);

					if (now >= nextRun)
						nextRun = nextRun.AddDays(1);

					var delay = nextRun - now;
					logger.LogInformation("PurgingBackgroundWorker next run scheduled at {NextRun} (in {Delay}).", nextRun, delay);

					try
					{
						await Task.Delay(delay, stoppingToken);
					}
					catch (OperationCanceledException)
					{
						// shutdown requested
						break;
					}

					// perform purge once
					try
					{
						await ProcessOnceAsync(stoppingToken);
					}
					catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
					{
						// graceful shutdown
						break;
					}
					catch (Exception ex)
					{
						logger.LogError(ex.GetExceptionMessages());
						if (await _dbGuard.IsReadyAsync(stoppingToken))
						{
							await InsertLog(ex, "Internal Server Error", null, null);
						}
					}

					// loop continues to compute next day's 00:01
				}
			}
			finally
			{
				logger.LogInformation("PurgingBackgroundWorker stopping.");
			}
		}

		private async Task ProcessOnceAsync(CancellationToken stoppingToken)
		{
			var settings = AppSettings.Read();
			var PurgeInMonth = settings.Maintenance.PurgingPeriodInMonth;
			var fiveDaysAgo = DateTime.Today.AddDays(-5);
			var monthsAgo = DateTime.Today.AddMonths(-PurgeInMonth);

			using var scope = serviceProvider.CreateScope();

			var logApp = scope.ServiceProvider.GetRequiredService<IApplicationLogRepository>();

			var logEmail = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
			var logTrans = scope.ServiceProvider.GetRequiredService<ITransactionLogRepository>();
			var logAudit = scope.ServiceProvider.GetRequiredService<IAuditTrailRepository>();

			var historyEmail = scope.ServiceProvider.GetRequiredService<IHistoryEmailRepository>();
			var historyTrans = scope.ServiceProvider.GetRequiredService<IHistoryTransactionLogRepository>();
			var historyAudit = scope.ServiceProvider.GetRequiredService<IHistoryAuditTrailRepository>();

			var user = scope.ServiceProvider.GetRequiredService<IUserRepository>();
			var category = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
			var document = scope.ServiceProvider.GetRequiredService<IDocumentsRepository>();			
			var documentFiles = scope.ServiceProvider.GetRequiredService<IDocumentFilesRepository>();

			var attributeCollections = scope.ServiceProvider.GetRequiredService<IAttributeCollectionsRepository>();
			var attribute = scope.ServiceProvider.GetRequiredService<IAttributesRepository>();
			var group = scope.ServiceProvider.GetRequiredService<IGroupRepository>();			

			// get records
			var mails = await logEmail.GetAsync(x => x.InsertedAt < fiveDaysAgo);
			var trans = await logTrans.GetAsync(x => x.InsertedAt < fiveDaysAgo);
			var audit = await logAudit.GetAsync(x => x.InsertedAt < fiveDaysAgo);

			// move to history
			if (mails?.Count > 0)
				await historyEmail.InsertManyAsync(mails.CopyProperties<List<HistoryEmail>>());

			if (trans?.Count > 0)
				await historyTrans.InsertManyAsync(trans.CopyProperties<List<HistoryTransactionLog>>());

			if (audit?.Count > 0)
				await historyAudit.InsertManyAsync(audit.CopyProperties<List<HistoryAuditTrail>>());

			// delete records
			if (mails?.Count > 0)
				await logEmail.DeleteManyAsync(mails);

			if (trans?.Count > 0)
				await logTrans.DeleteManyAsync(trans);

			if (audit?.Count > 0)
				await logAudit.DeleteManyAsync(audit);


			// purge document files deleted older than configure months
			await documentFiles.PurgeDeletedDocumentFiles(20, PurgeInMonth);

			// purge documents deleted older than configure months
			await document.PurgeDeletedDocuments(20, PurgeInMonth);

			// purge document deleted older than configure months
			await category.PurgeDeletedCategories(20, PurgeInMonth);

			// purge attribute collection deleted older than configure months
			await attributeCollections.PurgeDeletedAttributeCollections(20, PurgeInMonth);

			// purge attribute deleted older than configure months
			await attribute.PurgeDeletedAttributes(20, PurgeInMonth);

			// purge geoup deleted older than configure months
			await group.PurgeDeletedGroups(20, PurgeInMonth);

			// purge users deleted older than configured months
			await user.PurgeDeletedUsers(20, PurgeInMonth);

			// delete old application logs
			await logApp.AsQueryable().Where(x => x.InsertedAt < monthsAgo).ExecuteDeleteAsync();
		}

		#region old code
		//protected override Task ExecuteAsync(CancellationToken stoppingToken)
		//      {
		//          Process(stoppingToken);
		//          return Task.CompletedTask;
		//      }

		//   private async void Process(CancellationToken stoppingToken)
		//   {
		//       while (!stoppingToken.IsCancellationRequested)
		//       {
		//           try
		//           {
		//               var settings = AppSettings.Read();
		//               var PurgeInMonth = settings.Maintenance.PurgingPeriodInMonth;
		//var fiveDaysAgo = DateTime.Today.AddDays(-5);
		//               var monthsAgo = DateTime.Today.AddMonths(-PurgeInMonth);

		//               using var scope = serviceProvider.CreateScope();

		//               var logApp = scope.ServiceProvider.GetRequiredService<IApplicationLogRepository>();

		//               var logEmail = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
		//               var logTrans = scope.ServiceProvider.GetRequiredService<ITransactionLogRepository>();
		//               var logAudit = scope.ServiceProvider.GetRequiredService<IAuditTrailRepository>();

		//               var historyEmail = scope.ServiceProvider.GetRequiredService<IHistoryEmailRepository>();
		//               var historyTrans = scope.ServiceProvider.GetRequiredService<IHistoryTransactionLogRepository>();
		//               var historyAudit = scope.ServiceProvider.GetRequiredService<IHistoryAuditTrailRepository>();

		//               var user = scope.ServiceProvider.GetRequiredService<IUserRepository>();

		////get records
		//var mails = await logEmail.GetAsync(x => x.InsertedAt < fiveDaysAgo);
		//               var trans = await logTrans.GetAsync(x => x.InsertedAt < fiveDaysAgo);
		//               var audit = await logAudit.GetAsync(x => x.InsertedAt < fiveDaysAgo);

		//               //move to history
		//               await historyEmail.InsertManyAsync(mails.CopyProperties<List<HistoryEmail>>());
		//               await historyTrans.InsertManyAsync(mails.CopyProperties<List<HistoryTransactionLog>>());
		//               await historyAudit.InsertManyAsync(mails.CopyProperties<List<HistoryAuditTrail>>());

		//               //delete records
		//               await logEmail.DeleteManyAsync(mails);
		//               await logTrans.DeleteManyAsync(trans);
		//               await logAudit.DeleteManyAsync(audit);
		//               await user.PurgeDeletedUsers(20, PurgeInMonth);


		//await logApp.AsQueryable().Where(x => x.InsertedAt < monthsAgo).ExecuteDeleteAsync();
		//           }
		//           catch (Exception ex)
		//           {
		//               logger.LogError(ex.Message);

		//               await InsertLog(ex, "Internal Server Error", null, null);
		//           }
		//           finally
		//           {
		//               await Task.Delay(60000, stoppingToken);
		//           }
		//       }
		//   }
		#endregion

		private async Task InsertLog(Exception ex, string type, string parameter, string username)
        {
            var error = new ApplicationLog
            {
                Endpoint = null,
                Message = ex.GetExceptionMessages(),
                StackTrace = ex.StackTrace,
                Type = type,
                Parameter = parameter,
                UserName = username,
                InsertedAt = DateTime.Now
            };

            using var scope = serviceProvider.CreateScope();
            var log = scope.ServiceProvider.GetRequiredService<IApplicationLogRepository>();
            await log.InsertAsync(error);
            logger.LogError(ex.GetExceptionMessages());
        }
	}
}
