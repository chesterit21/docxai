using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Systems;
using Api.Domain.Enum;
using Api.Extensions;
using Api.Repository.Masters;
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
using System.Net.Http;
using System.Text.Json;

namespace Api.Services
{
    public class ReminderbackgroundWorker(IServiceProvider serviceProvider, ILogger<EmailBackgroundWorker> logger,
		IDatabaseGuard _dbGuard, IHttpClientFactory httpClientFactory) : BackgroundService
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
			logger.LogInformation("reminder Background worker started...");
			while (!stoppingToken.IsCancellationRequested)
			{
				if (await _dbGuard.IsReadyAsync(stoppingToken))
				{
					logger.LogInformation("Database is ready. Starting reminder document background processing...");
					break; // Exit the "waiting" loop
				}

				logger.LogWarning("Database not ready or config missing. Retrying in 10 seconds...");
				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}

			// throttle between WA sends
			var wahaThrottle = TimeSpan.FromMinutes(1);
			var today = DateTime.Now.Date;

			var appSettings = AppSettings.Read();
			var wahaApiKey = appSettings?.WAHA?.ApiKey ?? appSettings?.Authentication?.APIKey ?? string.Empty;
			var wahaBaseUrl = appSettings?.WAHA?.BaseUrl ?? appSettings?.ApplicationInfoData?.ApplicationUrl ?? string.Empty;

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = serviceProvider.CreateScope();

					var documentReminderRepo = scope.ServiceProvider.GetRequiredService<IDocumentReminderRepository>();
					var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
					var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

					// Reminders - only notify once per day per reminder (use reminder.Id to be specific)
					var reminders = await documentReminderRepo.GetAsync(x =>
						x.ReminderDateTime.Date == today && !x.IsDeleted);

					foreach (var reminder in reminders)
					{
						if (stoppingToken.IsCancellationRequested) break;

						// Check existence of a notification for this reminder today (match by DocumentID + NotificationType + presence of reminder.Id in NotifContent)
						bool alreadyNotified = await notificationRepo.AnyAsync(n =>
							n.DocumentID == reminder.DocumentID &&
							n.NotificationType == (short)NotificationType.DocumentReminder &&
							n.InsertedAt.Date == today &&
							(n.NotifContent != null && n.NotifContent.Contains(reminder.Id.ToString()))
						);

						if (alreadyNotified) continue;

						var notification = new Notifications
						{
							Id = Guid.NewGuid(),
							DocumentID = reminder.DocumentID,
							NotificationType = (short)NotificationType.DocumentReminder,
							NotifDescription = $"Reminder for document: {reminder.ReminderDesc}",
							NotifAction = "View",
							TargetActor = reminder.InsertedBy,
							NotifContent = JsonSerializer.Serialize(new
							{
								DocumentId = reminder.DocumentID,
								ReminderId = reminder.Id,
								ReminderDateTime = reminder.ReminderDateTime
							}),
							InsertedAt = DateTime.Now,
							InsertedBy = 0
						};

						await notificationRepo.InsertAsync(notification);

						// send WAHA once
						try
						{
							var user = await userRepo.GetUser(reminder.InsertedBy);
							if (user != null)
							{
								var phone = (user.PhoneNumber ?? string.Empty).Trim();
								if (!string.IsNullOrEmpty(phone))
								{
									var chatId = phone.Contains("@") ? phone : $"{phone}@c.us";
									await SendWahaAsync(wahaBaseUrl, wahaApiKey, chatId, $"Reminder: {reminder.ReminderDesc}");

									// throttle: wait before next WAHA send
									logger.LogInformation("Throttling WAHA sends for {Delay}", wahaThrottle);
									try
									{
										await Task.Delay(wahaThrottle, stoppingToken);
									}
									catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
									{
										// cancellation requested - rethrow to stop processing
										throw;
									}
								}
							}
						}
						catch (Exception ex)
						{
							logger.LogError(ex.GetExceptionMessages());
						}
					}
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch(ArgumentException ex)
				{
					logger.LogError(ex, "Background reminder worker error: {Message}", ex.GetExceptionMessages());
					//break;					
				}
				catch (Exception ex)
				{
					// Log and continue after idle delay (avoid tight exception loop)
					logger.LogError(ex, "Background reminder worker error: {Message}", ex.GetExceptionMessages());
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

		private async Task SendWahaAsync(string baseUrl, string apiKey, string chatId, string text)
		{
			var client = httpClientFactory.CreateClient("Waha");
			if (client.BaseAddress == null && !string.IsNullOrEmpty(baseUrl))
				client.BaseAddress = new Uri(baseUrl);

			// ensure header
			if (!string.IsNullOrEmpty(apiKey))
				client.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", apiKey);

			var payload = new
			{
				chatId = chatId,
				reply_to = (string?)null,
				text = text,
				linkPreview = true,
				linkPreviewHighQuality = false,
				session = "default"
			};

			// Use existing extension helper that returns deserialized result
			try
			{
				await client.PostAsJsonAsync<string>("/api/sendText", payload);
			}
			catch (Exception ex)
			{
				logger.LogError("WAHA send failed: {0}", ex.GetExceptionMessages());
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
