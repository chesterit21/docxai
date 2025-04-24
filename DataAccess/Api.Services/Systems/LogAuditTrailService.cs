using Api.Domain.EntityRequests;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Systems;
using Api.Repository.Masters;
using Api.Repository.Systems;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Api.Services.Systems
{
    public class LogAuditTrailService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IAuditTrailRepository repository) : BaseService(accessor, languageRepository)
    {
        public async Task<ResponsePagination> Get(DateTime startDate, DateTime endDate, int page, int limit)
        {
            await ValidateInputDateRangeAsync(startDate, endDate);

            var totalRecords = await repository.CountAsync(startDate, endDate);
            var records = await repository.GetAllAsync(startDate, endDate, page, limit);

            var data = records.Select(x => new ResponseAuditTrail
            {
                Id = x.Id,
                TransactionLogId = x.TransactionLogId,
                TableName = x.TableName,
                Command = x.Command,
                Before = x.Before == null ? null : JsonSerializer.Deserialize<JsonDocument>(x.Before.ToString()),
                After = x.After == null ? null : JsonSerializer.Deserialize<JsonDocument>(x.After.ToString()),
                InsertedBy = x.InsertedBy,
                InsertedAt = x.InsertedAt,
                InsertedByUserName = x.InsertedByUserName,
                InsertedByFullName = x.InsertedByFullName
            });

            return new ResponsePagination
            {
                TotalRecords = totalRecords,
                TotalPages = GetTotalPages(totalRecords, limit),
                Data = data
            };
        }

        public async Task<ResponsePagination> Get(ReqestFilter request)
        {
            await ValidateInputRequestAsync(request);
            await ValidateInputDateRangeAsync(request.StartDate.Value, request.EndDate.Value);

            var totalRecords = await repository.CountAsync(request.StartDate.Value, request.EndDate.Value);
            var records = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);

            var data = records.Select(x => new ResponseAuditTrail
            {
                Id = x.Id,
                TransactionLogId = x.TransactionLogId,
                TableName = x.TableName,
                Command = x.Command,
                Before = x.Before == null ? null : JsonSerializer.Deserialize<JsonDocument>(x.Before.ToString()),
                After = x.After == null ? null : JsonSerializer.Deserialize<JsonDocument>(x.After.ToString()),
                InsertedBy = x.InsertedBy,
                InsertedAt = x.InsertedAt,
                InsertedByUserName = x.InsertedByUserName,
                InsertedByFullName = x.InsertedByFullName
            });

            return new ResponsePagination
            {
                TotalRecords = totalRecords,
                TotalPages = GetTotalPages(totalRecords, request.Limit),
                Data = data
            };
        }
    }
}
