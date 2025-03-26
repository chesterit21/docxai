using Api.DataAccess.Models.Systems;
using Api.Extensions;
using Api.Repository.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Services
{
    public class PurgingBackgroundWorker(IServiceProvider serviceProvider, ILogger<PurgingBackgroundWorker> logger) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Process(stoppingToken);
            return Task.CompletedTask;
        }

        private async void Process(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var settings = AppSettings.Read();
                    var fiveDaysAgo = DateTime.Today.AddDays(-5);
                    var monthsAgo = DateTime.Today.AddMonths(-settings.Maintenance.PurgingPeriodInMonth);

                    using var scope = serviceProvider.CreateScope();

                    var logApp = scope.ServiceProvider.GetRequiredService<IApplicationLogRepository>();

                    var logEmail = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
                    var logTrans = scope.ServiceProvider.GetRequiredService<ITransactionLogRepository>();
                    var logAudit = scope.ServiceProvider.GetRequiredService<IAuditTrailRepository>();

                    var historyEmail = scope.ServiceProvider.GetRequiredService<IHistoryEmailRepository>();
                    var historyTrans = scope.ServiceProvider.GetRequiredService<IHistoryTransactionLogRepository>();
                    var historyAudit = scope.ServiceProvider.GetRequiredService<IHistoryAuditTrailRepository>();

                    //get records
                    var mails = await logEmail.GetAsync(x => x.InsertedAt < fiveDaysAgo);
                    var trans = await logTrans.GetAsync(x => x.InsertedAt < fiveDaysAgo);
                    var audit = await logAudit.GetAsync(x => x.InsertedAt < fiveDaysAgo);

                    //move to history
                    await historyEmail.InsertManyAsync(mails.CopyProperties<List<HistoryEmail>>());
                    await historyTrans.InsertManyAsync(mails.CopyProperties<List<HistoryTransactionLog>>());
                    await historyAudit.InsertManyAsync(mails.CopyProperties<List<HistoryAuditTrail>>());

                    //delete records
                    await logEmail.DeleteManyAsync(mails);
                    await logTrans.DeleteManyAsync(trans);
                    await logAudit.DeleteManyAsync(audit);

                    await logApp.AsQueryable().Where(x => x.InsertedAt < monthsAgo).ExecuteDeleteAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);

                    await InsertLog(ex, "Internal Server Error", null, null);
                }
                finally
                {
                    await Task.Delay(60000, stoppingToken);
                }
            }
        }

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
