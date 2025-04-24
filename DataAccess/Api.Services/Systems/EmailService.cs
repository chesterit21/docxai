using Api.DataAccess.Models.Systems;
using Api.Domain.EntityRequests;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Systems
{
    public class EmailService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IEmailRepository repository) : BaseService(accessor, languageRepository)
    {
        public async Task<object> GetAll(ReqestFilter request)
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
    }
}
