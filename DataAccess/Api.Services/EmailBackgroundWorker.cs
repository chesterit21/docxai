using Api.DataAccess;
using Api.DataAccess.Models.Systems;
using Api.Extensions;
using Api.Repository.Systems;
using Api.Services.Systems;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.SS.Formula.Functions;
using System;
using System.Text.Json;

namespace Api.Services
{
    public class EmailBackgroundWorker(IServiceProvider serviceProvider, ILogger<EmailBackgroundWorker> logger,
		IDatabaseGuard _dbGuard) : BackgroundService
    {

		// Poll interval when no work
		private static readonly TimeSpan IdleDelay = TimeSpan.FromMinutes(1);
		// Delay between sending emails to respondents
		private static readonly TimeSpan PerEmailDelay = TimeSpan.FromSeconds(3);

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return ProcessAsync(stoppingToken);
		}

		private async Task ProcessAsync(CancellationToken stoppingToken)
		{
			logger.LogInformation("EmailBackgroundWorker started...");
			while (!stoppingToken.IsCancellationRequested)
			{
				if (await _dbGuard.IsReadyAsync(stoppingToken))
				{
					logger.LogInformation("Database is ready. Starting Email background processing...");
					break; // Exit the "waiting" loop
				}

				logger.LogWarning("Database not ready or config missing. Retrying in 10 seconds...");
				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = serviceProvider.CreateScope();
					//var db = scope.ServiceProvider.GetRequiredService<DataContext>();
					//var data = await db.Email.ToListAsync()

					var repository = scope.ServiceProvider.GetRequiredService<IEmailRepository>();
					var mails = await repository.GetAsync(x => x.SentStatus != 1);

					if (mails == null || mails.Count == 0)
					{
						await Task.Delay(IdleDelay, stoppingToken);
						continue;
					}

					foreach (var mail in mails)
					{
						if (stoppingToken.IsCancellationRequested)
							break;

						try
						{
							var emailService = new MailServiceBuilder()
								.Subject(mail.Subject)
								.Body(mail.Body, mail.IsHtml)
								.AddRecipient(mail.To)
								.AddCc(mail.Cc)
								.Build();

							string response = await Task.Run(() => emailService.Send(), stoppingToken);

							mail.StatusMessage = response;
							mail.SentStatus = response != null && response.StartsWith("OK", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
						}
						catch (Exception ex)
						{
							logger.LogError(ex, "Failed to send email to {To}", mail?.To);

							var type = "Internal Server Error";
							if (ex.Message.Contains("No Such User Here", StringComparison.OrdinalIgnoreCase))
							{
								type = "Bad Request";
								mail.SentStatus = 3; // invalid recipient
							}
							else
							{
								mail.SentStatus = 2; // failed
								var msg = ex.Message;
								if (!string.IsNullOrEmpty(msg) && msg.Length > 1000)
									msg = msg.Substring(0, 1000);
								mail.StatusMessage = msg;
							}
						}

						try
						{
							await repository.UpdateAsync(mail);
						}
						catch (Exception uex)
						{
							// Log and continue — do not fail the whole loop because of DB issue for one email
							logger.LogError(uex, "Failed to update email status for {Id}", mail?.Id);
						}

						// Wait between sending emails to avoid rapid-fire
						try
						{
							await Task.Delay(PerEmailDelay, stoppingToken);
						}
						catch (TaskCanceledException)
						{
							break;
						}
					}
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch(ArgumentException ex)
				{
					logger.LogError(ex, "Background email worker error: {Message}", ex.GetExceptionMessages());
					//break;					
				}
				catch (Exception ex)
				{
					// Log and continue after idle delay (avoid tight exception loop)
					logger.LogError(ex, "Background email worker error: {Message}", ex.GetExceptionMessages());
					if (await _dbGuard.IsReadyAsync(stoppingToken))
					{
						await InsertLog(ex, "Internal Server Error", null, 0);
					}
				}

				// small pause before next polling cycle (if not cancelled)
				try
				{
					await Task.Delay(IdleDelay, stoppingToken);
				}
				catch (TaskCanceledException)
				{
					break;
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
