using Api.DataAccess.Models.Systems;
using Api.Extensions;
using Api.Repository.Systems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Api.Services
{
    public class EmailBackgroundWorker(IServiceProvider serviceProvider, ILogger<EmailBackgroundWorker> logger) : BackgroundService
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
                int userId = 0;
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
                    var mails = await repository.GetAsync(x => x.SentStatus != 1);

                    if (mails.Count == 0)
                        continue;

                    var processedEmails = new List<Email>();

                    foreach (var mail in mails)
                    {
                        try
                        {
                            userId = mail.InsertedBy;

                            var emailService = new MailServiceBuilder()
                            .Subject(mail.Subject)
                            .Body(mail.Body, mail.IsHtml)
                            .AddRecipient(mail.To)
                            .AddCc(mail.Cc)
                            .Build();

                            var response = emailService.Send();
                            mail.StatusMessage = response;
                            mail.SentStatus = response.StartsWith("OK") ? 1 : 0;
                            processedEmails.Add(mail);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.GetExceptionMessages());

                            var type = "Internal Server Error";
                            if (ex.Message.Contains("No Such User Here", StringComparison.OrdinalIgnoreCase))
                            {
                                type = "Bad Request";
                                mail.SentStatus = 2;
                            }
                            else
                            {
                                mail.SentStatus = 3;
                            }

                            processedEmails.Add(mail);

                            await InsertLog(ex, type, JsonSerializer.Serialize(mail), mail.InsertedBy);
                        }
                    }

                    await repository.UpdateManyAsync(processedEmails);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);

                    await InsertLog(ex, "Internal Server Error", null, 0);
                }
                finally
                {
                    await Task.Delay(10000, stoppingToken);
                }
            }
        }

        private async Task InsertLog(Exception ex, string type, string parameter, int userId)
        {
            var error = new ApplicationLog
            {
                Endpoint = null,
                Message = ex.GetExceptionMessages(),
                StackTrace = ex.StackTrace,
                Type = type,
                Parameter = parameter,
                UserName = userId.ToString(),
                InsertedAt = DateTime.Now,
                InsertedBy = 0,
            };

            using var scope = serviceProvider.CreateScope();
            var log = scope.ServiceProvider.GetRequiredService<IApplicationLogRepository>();
            await log.InsertAsync(error);
            logger.LogError(ex.GetExceptionMessages());
        }
    }
}
