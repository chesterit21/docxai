using Api.Domain;
using Api.Domain.Constants;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Text.Json.Nodes;

namespace Api.Services
{
    public abstract class BaseService(IHttpContextAccessor accessor, ILanguageRepository repository, IConfiguration configuration = null)
    {
        protected string Lang
        {
            get
            {
                accessor.HttpContext.Request.Headers.TryGetValue("x-lang", out var lang);
                if (lang == StringValues.Empty)
                    return "ID";

                return lang.ToString().ToUpper();
            }
        }

        protected async Task<string> GetMessage(string code)
        {
            var lang = await repository.GetSingleAsync(x => x.Code == code);
            return Lang == "EN" ? lang.En : lang.Id;
        }

        protected async Task ValidateInputAsync(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException(message);
            }
        }

        protected async Task ValidateInputAsync(Guid item)
        {
            if (item == Guid.Empty)
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException(message);
            }
        }

        protected async Task ValidateInputAsync(List<Guid> items)
        {
            foreach (var item in items)
                if (item == Guid.Empty)
                {
                    var message = await GetMessage(LangCodes.InputEmpty);
                    throw new ApiException(message);
                }
        }

        protected async Task ValidateInputAsync(int item)
        {
            if (item == 0)
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException(message);
            }
        }

        protected async Task ValidateInputAsync(bool? item)
        {
            if (item == null)
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException(message);
            }
        }

        protected async Task ValidateInputAsync(List<string> items)
        {
            if (items.ToArray().HasEmptyString())
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException(message);
            }
        }

        protected async Task ValidateInputAsync(List<int> items)
        {
            foreach (var item in items)
                if (item == 0)
                {
                    var message = await GetMessage(LangCodes.InputEmpty);
                    throw new ApiException(message);
                }
        }

        protected async Task ValidateInputRequestAsync(List<object> requests)
        {
            if (requests == null || requests.Count == 0 || requests.HasEmptyRequiredProperty())
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException(message);
            }
        }

        protected async Task ValidateInputRequestAsync(object request)
        {
            if (request == null || request.HasEmptyRequiredProperty())
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException(message);
            }
        }

        protected string MaskPassword(string json, bool descendNested = true)
        {
            if (string.IsNullOrWhiteSpace(json))
                return json;

            json = json.Trim();
            if ((json.StartsWith("{") && json.EndsWith("}")) || (json.StartsWith("[") && json.EndsWith("]")))
                return JsonNode.Parse(json).UpdateNode("password", "*****", descendNested)?.ToJsonString();
            return json;
        }

        protected async Task ValidateInputDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate || endDate > DateTime.Now)
            {
                var message = await GetMessage(LangCodes.InputInvalidDateRange);
                throw new ApiException(message);
            }
        }

        protected int GetTotalPages(int totalRecords, int limit) => GetTotalPages(Convert.ToInt64(totalRecords), Convert.ToInt64(limit));

        protected int GetTotalPages(long totalRecords, long limit)
        {
            if (totalRecords < 1)
                return 0;

            return (int)Math.Ceiling((double)totalRecords / limit);
        }

        protected async Task ValidateUploadAsync(IFormFile file)
        {
            string ext = configuration.GetValue<string>("FileOption:FileExt");

            if (file == null || file.Length == 0)
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException(message);
            }

            // Batasan Ukuran File 20MB (20 * 1024 * 1024 bytes) = 20971520 bytes
            if (file.Length > 20971520)
            {
                throw new ApiException("File size cannot be more than 20 MB!");
            }

            var fileName = file.FileName;
            var fileExtension = Path.GetExtension(fileName);
            if (ext.IndexOf(fileExtension) == -1)
            {
                var message = await GetMessage(LangCodes.UploadExtInvalid);
                throw new ApiException(message);
            }
        }

        protected int GetUser() => accessor.HttpContext?.User?.Identity?.Name.ToInt32() ?? 0;

        protected string UserName => accessor?.HttpContext?.GetClaim<string>("given_name");
    }
}

