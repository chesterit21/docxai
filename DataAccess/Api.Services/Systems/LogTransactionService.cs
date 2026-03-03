using Api.Domain.EntityResponses;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Microsoft.AspNetCore.Http;


namespace Api.Services.Systems
{
    public class LogTransactionService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, ITransactionLogRepository repository) : BaseService(accessor, languageRepository)
    {
        public async Task<ResponsePagination> Get(int page, int limit)
        {
            await ValidateInputAsync([page, limit]);

            var totalRecords = await repository.CountAsync();
            var records = await repository.GetAsync(page, limit);

            return new ResponsePagination
            {
                TotalRecords = totalRecords,
                TotalPages = GetTotalPages(totalRecords, limit),
                Data = records
            };
        }

        public async Task<object> Get(DateTime startDate, DateTime endDate, int page, int limit)
        {
            await ValidateInputDateRangeAsync(startDate, endDate);

            //var totalRecords = await repository.CountAsync(startDate.ToUniversalTime(), endDate.ToUniversalTime());
            var response = await repository.GetAsync(startDate, endDate, page, limit);

            return new ResponsePagination
            {
                TotalRecords = response.TotalRecord,
                TotalPages = GetTotalPages(response.TotalRecord, limit),
                Data = response.Record
			};
        }
    }
}
