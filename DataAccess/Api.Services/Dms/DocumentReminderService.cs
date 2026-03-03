using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses;
using Api.Domain.Enum;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Services.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Dms
{
	public class DocumentReminderService : BaseService
	{
		private readonly ILanguageRepository languageRepository;
		private readonly IDocumentReminderRepository repository;
		private readonly IDocumentsRepository documentRepo;

		public DocumentReminderService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IDocumentReminderRepository repository, IDocumentsRepository documentRepo) 
			: base(accessor, repository: languageRepository)
		{
			this.languageRepository = languageRepository;
			this.repository = repository;
			this.documentRepo = documentRepo;
		}

		public async Task<object> GetAll(RequestDocumentReminderNewList request)
		{
			await ValidateInputRequestAsync(request);
			var records = await repository.GetAsync(x => x.DocumentID == request.DocumentID && x.IsActive == true, request.Page, request.Limit);
			
			var totalRecords = await repository.CountAsync(x => x.DocumentID == request.DocumentID && x.IsActive == true);
			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = records
			};
		}

		public async Task<object> GetDocumentReminderInTopPanel(RequestDocumentReminderInTop request)
		{
			await ValidateInputRequestAsync(request);
			var response = await repository.GetRemindersByOwner(request.DTFrom, request.DTTo, request.ReminderDesc, request.Page, request.Limit);
			var totalRecords = response.TotalRecord;

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = response.Record
			};
		}

		public async Task<DocumentReminders> Get(int id)
		{
			await ValidateInputAsync(id);
			await repository.LogTransaction($"Get document reminder by id {id}", Domain.Attributes.UserAction.Read);
			return await repository.GetSingleAsync(x => x.Id == id);
		}

		public async Task<object> Create(RequestDocumentRemindersNew request)
		{
			await ValidateInputRequestAsync(request);
			var entity = request.CopyProperties<DocumentReminders>();
			//DateTime rDatetime = request.ReminderDateTime.ToUniversalTime();
			//DateTime utcDateTime = new DateTime(rDatetime.Year, rDatetime.Month, rDatetime.Day, rDatetime.Hour, rDatetime.Minute, rDatetime.Second, DateTimeKind.Utc);

			entity.ReminderDateTime = request.ReminderDateTime;
			Guid auditTrl = await repository.LogTransactionAndAuditTrail($"Insert new document reminders", Domain.Attributes.UserAction.Insert, entity);
			await documentRepo.LogDocumentInsert(auditTrl, request.DocumentID, LogDocumentAction.AddReminder);
			return await repository.InsertAsync(entity);
		}

		public async Task<DocumentReminders> Update(RequestDocumentRemindersUpdate request)
		{
			await ValidateInputRequestAsync(request);

			var entity = request.CopyProperties<DocumentReminders>();
			//DateTime rDatetime = request.ReminderDateTime;
			//DateTime utcDateTime = new DateTime(rDatetime.Year, rDatetime.Month, rDatetime.Day, rDatetime.Hour, rDatetime.Minute, rDatetime.Second, DateTimeKind.Utc);

			entity.ReminderDateTime = request.ReminderDateTime;
			await repository.LogTransactionAndAuditTrail($"Update document reminder", Domain.Attributes.UserAction.Update, entity);
			return await repository.UpdateAsync(entity);
		}

		public async Task<bool> InActivated(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);

			var entity = await repository.GetSingleAsync(x => x.Id == id);

			await repository.LogTransactionAndAuditTrail($"Delete existing Document File with id {id}", Domain.Attributes.UserAction.Update, entity);
			return await repository.MarkAsDeletedAsync(id);
		}

		private async Task CheckIfExist(int id)
		{
			var any = await repository.AnyAsync(x => x.Id == id);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Document reminder Id {id}");
			}
		}

	}
}
