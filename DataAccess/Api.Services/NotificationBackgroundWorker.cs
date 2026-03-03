using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Systems;
using Api.Domain.Enum;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Api.Services.Systems;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;

namespace Api.Services
{
    public class NotificationBackgroundWorker(IServiceProvider serviceProvider,
			ILogger<NotificationBackgroundWorker> logger,
			IHttpClientFactory httpClientFactory, IDatabaseGuard _dbGuard) : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
			logger.LogInformation("NotificationBackgroundWorker started (daily at 02:00).");
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
					// compute next 02:00 (local time). Use DateTime.Now if you prefer UTC scheduling.
					var now = DateTime.Now;
					//var nextRun = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0);
					var nextRun = new DateTime(now.Year, now.Month, now.Day, 0, 1, 0);
					//var nextRun = new DateTime(now.Year, now.Month, now.Day, 8, 44, 0);
					if (now >= nextRun)
						nextRun = nextRun.AddDays(1);

					var delay = nextRun - now;
					logger.LogInformation("Next notification run scheduled at {NextRun} (in {Delay}).", nextRun, delay);

					try
					{
						// wakes earlier if cancellation requested
						await Task.Delay(delay, stoppingToken); 
					}
					catch (OperationCanceledException)
					{
						// shutting down
						break;
					}

					try
					{
						await ProcessOnceAsync(stoppingToken);
					}
					catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
					{
						// shutdown requested
						break;
					}
					catch (ArgumentException ex)
					{
						logger.LogError(ex, "Background email worker error: {Message}", ex.GetExceptionMessages());
					}
					catch (Exception ex)
					{
						logger.LogError(ex.GetExceptionMessages());
					}
					// loop will compute next 2 AM and wait again
				}
			}
			finally
			{
				logger.LogInformation("NotificationBackgroundWorker stopping.");
			}
		}

		private async Task ProcessOnceAsync(CancellationToken stoppingToken)
		{
			using var scope = serviceProvider.CreateScope();

			var documentsRepo = scope.ServiceProvider.GetRequiredService<IDocumentsRepository>();
			//var documentReminderRepo = scope.ServiceProvider.GetRequiredService<IDocumentReminderRepository>();
			var documentSharedRepo = scope.ServiceProvider.GetRequiredService<IDocumentSharedRepository>();
			var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
			var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
			var userGroupRepo = scope.ServiceProvider.GetRequiredService<IUserGroupRepository>();
			var emailRepo = scope.ServiceProvider.GetRequiredService<IEmailRepository>();

			var appSettings = AppSettings.Read();
			var wahaApiKey = appSettings?.WAHA?.ApiKey ?? appSettings?.Authentication?.APIKey ?? string.Empty;
			var wahaBaseUrl = appSettings?.WAHA?.BaseUrl ?? appSettings?.ApplicationInfoData?.ApplicationUrl ?? string.Empty;

			// throttle between WA sends
			var wahaThrottle = TimeSpan.FromMinutes(1);

			var today = DateTime.Now.Date;

			// Expiring documents - only notify once per day per document
			//var expiringDocs = await documentsRepo.GetAsync(x =>
			//	x.ExpiryDate.HasValue &&
			//	x.ReminderDays.HasValue &&
			//	(x.ExpiryDate.Value.Date - DateTime.Now.Date).Days <= x.ReminderDays.Value &&
			//	(x.ExpiryDate.Value.Date - DateTime.Now.Date).Days >= 0
			//);
			var expiringDocs = await documentsRepo.GetAsync(d =>
				d.ExpiryDate.HasValue &&
				(
					(d.ReminderDays.HasValue && today >= d.ExpiryDate.Value.AddDays(-d.ReminderDays.Value) && today < d.ExpiryDate.Value)
					||
					today == d.ExpiryDate.Value.Date
				)
			);

			await ProcessDocumentExpirationsAsync(expiringDocs, documentSharedRepo, userGroupRepo, emailRepo, userRepo);

			foreach (var doc in expiringDocs)
			{
				if (stoppingToken.IsCancellationRequested) break;

				bool alreadyNotified = await notificationRepo.AnyAsync(n =>
					n.DocumentID == doc.Id &&
					n.NotificationType == (short)NotificationType.DocumentExp &&
					n.InsertedAt.Date == today
				);

				if (alreadyNotified) continue;

				var notification = new Notifications
				{
					Id = Guid.NewGuid(),
					DocumentID = doc.Id,
					NotificationType = (short)NotificationType.DocumentExp,
					NotifDescription = $"Document {doc.DocumentTitle} has expired",
					NotifAction = "View",
					TargetActor = doc.InsertedBy,
					NotifContent = JsonSerializer.Serialize(new
					{
						DocumentId = doc.Id,
						ExpiryDate = doc.ExpiryDate
					}),
					InsertedAt = DateTime.Now,
					InsertedBy = 0
				};

				await notificationRepo.InsertAsync(notification);

				// send WAHA once
				try
				{
					var user = await userRepo.GetUser(doc.InsertedBy);
					if (user != null)
					{
						var phone = (user.PhoneNumber ?? string.Empty).Trim();
						if (!string.IsNullOrEmpty(phone))
						{
							var chatId = phone.Contains("@") ? phone : $"{phone}@c.us";
							await SendWahaAsync(wahaBaseUrl, wahaApiKey, chatId, $"Document expired: {doc.DocumentTitle}");

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

			//// Reminders - only notify once per day per reminder (use reminder.Id to be specific)
			//var reminders = await documentReminderRepo.GetAsync(x =>
			//	x.ReminderDateTime.Date == today && !x.IsDeleted);

			//foreach (var reminder in reminders)
			//{
			//	if (stoppingToken.IsCancellationRequested) break;

			//	// Check existence of a notification for this reminder today (match by DocumentID + NotificationType + presence of reminder.Id in NotifContent)
			//	bool alreadyNotified = await notificationRepo.AnyAsync(n =>
			//		n.DocumentID == reminder.DocumentID &&
			//		n.NotificationType == (short)NotificationType.DocumentReminder &&
			//		n.InsertedAt.Date == today &&
			//		(n.NotifContent != null && n.NotifContent.Contains(reminder.Id.ToString()))
			//	);

			//	if (alreadyNotified) continue;

			//	var notification = new Notifications
			//	{
			//		Id = Guid.NewGuid(),
			//		DocumentID = reminder.DocumentID,
			//		NotificationType = (short)NotificationType.DocumentReminder,
			//		NotifDescription = $"Reminder for document: {reminder.ReminderDesc}",
			//		NotifAction = "View",
			//		TargetActor = reminder.InsertedBy,
			//		NotifContent = JsonSerializer.Serialize(new
			//		{
			//			DocumentId = reminder.DocumentID,
			//			ReminderId = reminder.Id,
			//			ReminderDateTime = reminder.ReminderDateTime
			//		}),
			//		InsertedAt = DateTime.Now,
			//		InsertedBy = 0
			//	};

			//	await notificationRepo.InsertAsync(notification);

			//	// send WAHA once
			//	try
			//	{
			//		var user = await userRepo.GetUser(reminder.InsertedBy);
			//		if (user != null)
			//		{
			//			var phone = (user.UserName ?? string.Empty).Trim();
			//			if (!string.IsNullOrEmpty(phone))
			//			{
			//				var chatId = phone.Contains("@") ? phone : $"{phone}@c.us";
			//				await SendWahaAsync(wahaBaseUrl, wahaApiKey, chatId, $"Reminder: {reminder.ReminderDesc}");

			//				// throttle: wait before next WAHA send
			//				logger.LogInformation("Throttling WAHA sends for {Delay}", wahaThrottle);
			//				try
			//				{
			//					await Task.Delay(wahaThrottle, stoppingToken);
			//				}
			//				catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			//				{
			//					// cancellation requested - rethrow to stop processing
			//					throw;
			//				}
			//			}
			//		}
			//	}
			//	catch (Exception ex)
			//	{
			//		logger.LogError(ex.GetExceptionMessages());
			//	}
			//}
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

		#region old code
		//private async void Process(CancellationToken stoppingToken)
		//      {
		//          while (!stoppingToken.IsCancellationRequested)
		//          {
		//              try
		//              {
		//                  using var scope = _serviceProvider.CreateScope();
		//                  var documentsRepo = scope.ServiceProvider.GetRequiredService<IDocumentsRepository>();
		//                  var documentReminderRepo = scope.ServiceProvider.GetRequiredService<IDocumentReminderRepository>();
		//                  var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
		//			var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

		//			// read settings (API key + base url)
		//			var appSettings = AppSettings.Read();
		//			var wahaApiKey = appSettings?.WAHA?.ApiKey ?? string.Empty;
		//			var wahaBaseUrl = appSettings?.WAHA?.BaseUrl ?? "http://localhost:3000";

		//			// Check for document expiration
		//			//var expiringDocs = await documentsRepo.GetAsync(x =>
		//			//	x.ExpiryDate.HasValue &&
		//			//	x.ExpiryDate.Value.Date == DateTime.Now.Date);

		//			var expiringDocs = await documentsRepo.GetAsync(x => 
		//                      x.ExpiryDate.HasValue &&
		//                      x.ReminderDays.HasValue &&
		//                      (x.ExpiryDate.Value.Date - DateTime.Now.Date).Days <= x.ReminderDays.Value &&
		//                      (x.ExpiryDate.Value.Date - DateTime.Now.Date).Days >= 0
		//                  );

		//                  foreach (var doc in expiringDocs)
		//                  {
		//                      var notification = new Notifications
		//                      {
		//                          Id = Guid.NewGuid(),
		//                          DocumentID = doc.Id,
		//                          NotificationType = (short)NotificationType.DocumentExp,
		//                          NotifDescription = $"Document {doc.DocumentTitle} has expired",
		//                          NotifAction = "View",
		//                          TargetActor = doc.InsertedBy,
		//                          NotifContent = JsonSerializer.Serialize(new { 
		//                              DocumentId = doc.Id, 
		//                              ExpiryDate = doc.ExpiryDate 
		//                          })
		//                      };

		//                      await notificationRepo.InsertAsync(notification);
		//				// send WA notification
		//				try
		//				{
		//					var user = await userRepo.GetUser(doc.InsertedBy);
		//					if (user != null)
		//					{
		//						// assumption: user's phone number is stored in UserName when it's numeric
		//						// adjust this if you have a dedicated Phone field (replace with that field)
		//						var phone = user.PhoneNumber?.Trim();
		//						if (!string.IsNullOrEmpty(phone))
		//						{
		//							var chatId = phone.Contains("@") ? phone : $"{phone}@c.us";

		//							//using var http = new HttpClient { BaseAddress = new Uri(wahaBaseUrl)};

		//							//if (!string.IsNullOrEmpty(apiKey))
		//							//	http.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", apiKey);

		//							var http = _httpClientFactory.CreateClient("Waha");
		//							// if base url wasn't configured in client registration, set per-call
		//							if (!string.IsNullOrEmpty(wahaBaseUrl) && http.BaseAddress == null)
		//								http.BaseAddress = new Uri(wahaBaseUrl);

		//							if (!string.IsNullOrEmpty(wahaApiKey))
		//								http.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", wahaApiKey);

		//							var payload = new
		//							{
		//								chatId = chatId,
		//								reply_to = (string?)null,
		//								text = $"Document expired: {doc.DocumentTitle}",
		//								linkPreview = true,
		//								linkPreviewHighQuality = false,
		//								session = "default"
		//							};

		//							// Post using your extension helper
		//							await http.PostAsJsonAsync<string>("/api/sendText", payload);
		//						}
		//					}
		//				}
		//				catch (Exception ex)
		//				{
		//					logger.LogError(ex.GetExceptionMessages());
		//				}
		//			}

		//                  // Check for document reminders
		//                  var today = DateTime.Now;
		//                  var reminders = await documentReminderRepo.GetAsync(x => 
		//                      x.ReminderDateTime.Date == today.Date && 
		//                      !x.IsDeleted);

		//                  foreach (var reminder in reminders)
		//                  {
		//                      var notification = new Notifications
		//                      {
		//                          Id = Guid.NewGuid(),
		//                          DocumentID = reminder.DocumentID,
		//                          NotificationType = (short)NotificationType.DocumentReminder,
		//                          NotifDescription = $"Reminder for document: {reminder.ReminderDesc}",
		//                          NotifAction = "View",
		//                          TargetActor = reminder.InsertedBy,
		//                          NotifContent = JsonSerializer.Serialize(new { 
		//                              DocumentId = reminder.DocumentID, 
		//                              ReminderId = reminder.Id,
		//                              ReminderDateTime = reminder.ReminderDateTime 
		//                          })
		//                      };

		//                      await notificationRepo.InsertAsync(notification);

		//				try
		//				{
		//					var user = await userRepo.GetUser(reminder.InsertedBy);
		//					if (user != null)
		//					{
		//						var phone = user.UserName?.Trim();
		//						if (!string.IsNullOrEmpty(phone))
		//						{
		//							var chatId = phone.Contains("@") ? phone : $"{phone}@c.us";

		//							//using var http = new HttpClient { BaseAddress = new Uri(wahaBaseUrl) };
		//							//if (!string.IsNullOrEmpty(apiKey))
		//							//	http.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", apiKey);
		//							var http = _httpClientFactory.CreateClient("Waha");

		//							if (!string.IsNullOrEmpty(wahaBaseUrl) && http.BaseAddress == null)
		//								http.BaseAddress = new Uri(wahaBaseUrl);

		//							if (!string.IsNullOrEmpty(wahaApiKey))
		//								http.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", wahaApiKey);

		//							var payload = new
		//							{
		//								chatId = chatId,
		//								reply_to = (string?)null,
		//								text = $"Reminder: {reminder.ReminderDesc}",
		//								linkPreview = true,
		//								linkPreviewHighQuality = false,
		//								session = "default"
		//							};

		//							await http.PostAsJsonAsync<string>("/api/sendText", payload);
		//						}
		//					}
		//				}
		//				catch (Exception ex)
		//				{
		//					logger.LogError(ex.GetExceptionMessages());
		//				}
		//			}
		//              }
		//              catch (Exception ex)
		//              {
		//                  logger.LogError(ex.GetExceptionMessages());
		//              }
		//              finally
		//              {
		//                  // Check every minute
		//                  await Task.Delay(60000, stoppingToken);
		//              }
		//          }
		//      }
		#endregion


		public async Task ProcessDocumentExpirationsAsync(List<Documents> listDocs, IDocumentSharedRepository documentSharedRepo, 
			IUserGroupRepository userGroupRepo, IEmailRepository emailRepo, IUserRepository userRepo)
		{
			var today = DateTime.Today;

			foreach (var doc in listDocs)
			{
				var recipientEmails = new HashSet<string>();
				var ownerInfo = await userRepo.GetSingleAsync(x => x.UserId == doc.Owner);
				doc.OwnerInfo = ownerInfo;

				if (doc.OwnerInfo != null && !string.IsNullOrEmpty(doc.OwnerInfo.EmailAddress))
				{
					recipientEmails.Add(doc.OwnerInfo.EmailAddress);
				}

				// Get Privileged Users (from DocumentSharedPrivillege)
				var privileges = await documentSharedRepo.GetSharedUsersByDocumentIDAsync(doc.Id);

				foreach (var priv in privileges.SharedUser)
				{
					if (priv.ShareType == "user" && priv.Email != null)
					{
						recipientEmails.Add(priv.Email);
					}
					else if (priv.ShareType == "group" && priv.GroupID.HasValue)
					{
						var groupUsers = await userGroupRepo.GetUsersByGroupId(priv.GroupID.Value);
						foreach (var gUser in groupUsers)
						{
							recipientEmails.Add(gUser.User.EmailAddress);
						}
					}
				}

				if (recipientEmails.Any())
				{
					var emailEntry = new Email
					{
						Id = Guid.NewGuid(),
						Subject = $"Document Expiry Alert: {doc.DocumentTitle}",
						To = string.Join(";", recipientEmails), // Semicolon separated list
						Body = $@"
							<h3>Document Expiration Notice</h3>
							<p>The document <b>{doc.DocumentTitle}</b> is set to expire on {doc.ExpiryDate?.ToString("f")}.</p>
							<p>Please take the necessary actions.</p>",
						IsHtml = true,
						SentStatus = 0, // Pending
					};

					try
					{
						logger.LogInformation("Creating email notification for document '{DocumentTitle}' to: {Recipients}",
							doc.DocumentTitle, emailEntry.To);
						await emailRepo.InsertAsync(emailEntry);
					}
					catch
					{
						// ignore logging errors
					}					
				}
			}
		}
	}
}