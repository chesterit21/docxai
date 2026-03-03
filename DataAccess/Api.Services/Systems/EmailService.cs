using Api.DataAccess.Models.Systems;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityResponses;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Systems
{
    public class EmailService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IEmailRepository repository) : BaseService(accessor, languageRepository)
    {
        public async Task<object> GetAll(RequestFilter request)
        {
            await ValidateInputRequestAsync(request);
            var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            return ObjectFlatter.Flatten(result);
        }

        public async Task<string> SendEmailDirect(RequestEmail request)
        {
            await ValidateInputRequestAsync(request);
            var mail = new MailServiceBuilder()
                .Subject(request.Subject)
                .Body(request.Body, request.IsHtml)
                .AddRecipient(request.To)
                .AddCc(request.Cc)
                .Build();

            await repository.LogTransaction($"Send direct email", Domain.Attributes.UserAction.Insert);

            return mail.Send();
        }

        public async Task SendEmailTable(RequestEmail request)
        {
            await ValidateInputRequestAsync(request);

            var email = request.CopyProperties<Email>();

            await repository.LogTransactionAndAuditTrail($"Insert new email data for indirect email", Domain.Attributes.UserAction.Insert, email);
            await repository.InsertAsync(email);
        }

		public async Task<object> Get(DateTime startDate, DateTime endDate, int page, int limit)
		{
			await ValidateInputDateRangeAsync(startDate, endDate);

			var totalRecords = await repository.CountAsync(startDate, endDate);
			var records = await repository.GetAsync(x => x.InsertedAt >= startDate && x.InsertedAt <= endDate, page, limit, "InsertedAt", "desc");

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, limit),
				Data = records
			};
		}

		public async Task ReSendEmailTable(Guid id)
		{
			await ValidateInputRequestAsync(id);
            var request = await repository.GetSingleAsync(x => x.Id == id);
            if (request == null)
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}, ID : {id}");
			}

			var email = request.CopyProperties<Email>();
            email.SentStatus = 0;
            email.StatusMessage = null;
            email.UpdatedAt = DateTime.Now;
            email.UpdatedBy = GetUser();

			await repository.LogTransaction($"Re-send email data for indirect email, ID : " + id, Domain.Attributes.UserAction.Insert);
			await repository.UpdateAsync(email);
		}

		public async Task ReSendMultiEmailTable(RequestResendMultiEmail data)
		{
			await ValidateInputRequestAsync(data);
			foreach (var id in data.Ids)
			{
				var request = await repository.GetSingleAsync(x => x.Id == id);
				if (request == null)
				{
					continue;
				}

				var email = request.CopyProperties<Email>();
				email.SentStatus = 0;
				email.StatusMessage = null;
				email.UpdatedAt = DateTime.Now;
				email.UpdatedBy = GetUser();

				await repository.LogTransaction($"Re-send (multi) email data for indirect email, ID : " + id, Domain.Attributes.UserAction.Insert);
				await repository.UpdateAsync(email);
			}
		}
	}
}
