using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses.Systems;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace Api.Services.Systems
{
    public class LanguageService(IHttpContextAccessor accessor, ILanguageRepository repository) : BaseService(accessor, repository)
    {
        public async Task<ResponseLang> GetLanguage(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException($"{message}. Code {code}");
            }

            var result = await repository.GetSingleAsync(x => x.Code == code);
            if (result == null)
                return null;

            return result.CopyProperties<ResponseLang>();
        }

        public async Task<ResponseLang> GetLanguage(string code, string type)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException($"{message}. Code {code}");
            }

            var result = await repository.Get(code, type);
            if (result == null)
                return null;

            return result.CopyProperties<ResponseLang>();
        }


        public async Task<object> GetLanguages(string type)
        {
            var result = await repository.GetAsync(x => x.Type == type);
            return (result);
        }

        public async Task<byte[]> DownloadJsonLanguageAsync(string type) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(await GetLanguages(type)));

        public async Task UpdateLanguage(RequestLang request, string type)
        {
            await ValidateInputRequestAsync(request);
            var entity = request.CopyProperties<DataAccess.Models.Masters.Language>();
            entity.Type = type;
            await repository.UpsertAsync(entity);
        }

        public async Task UpdateLanguage(List<RequestLang> request, string type)
        {
            await ValidateInputRequestAsync(request);
            foreach (var item in request)
            {
                if (string.IsNullOrWhiteSpace(item.Code) || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.En))
                {
                    var message = await GetMessage(LangCodes.InputEmpty);
                    throw new ApiException($"{message}. Code/ID/EN");
                }
            }

            var entities = request.CopyProperties<List<DataAccess.Models.Masters.Language>>();
            foreach (var entity in entities)
                entity.Type = type;

            await repository.UpsertManyAsync(entities);
        }

        public async Task UploadLanguageFromJson(IFormFile file, string type)
        {
            if (file == null)
                throw new Exception(LangCodes.InputEmpty);

            using var stream = file.OpenReadStream();

            if (stream == null || stream.Length == 0)
                throw new Exception(LangCodes.InputEmpty);

            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(json) || json == "[]" || json == "{}")
            {
                var message = await GetMessage(LangCodes.InputEmpty);
                throw new ApiException($"{message}. @File");
            }

            var entities = JsonSerializer.Deserialize<List<DataAccess.Models.Masters.Language>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            foreach (var entity in entities)
                entity.Type = type;

            await repository.UpsertManyAsync(entities);
        }

        public async Task UploadLanguageFromExcel(IFormFile file, string type)
        {
            if (file == null)
                throw new Exception(LangCodes.InputEmpty);

            using var stream = file.OpenReadStream();

            if (stream == null || stream.Length == 0)
                throw new Exception(LangCodes.InputEmpty);

            var entities = stream.ExcelToEntity<DataAccess.Models.Masters.Language>().ToList();
            foreach (var entity in entities)
                entity.Type = type;

            await repository.UpsertManyAsync(entities);
        }
    }


    //public class LanguageService : BaseService
    //{
    //    public JsonLanguage GetLanguage(string code)
    //    {
    //        if (string.IsNullOrWhiteSpace(code))
    //            throw new ApiException(LangCodes.InputEmpty);

    //        return JsonLanguage.GetLanguage(code);
    //    }

    //    public List<JsonLanguage> GetLanguages()
    //    {
    //        return JsonLanguage.GetLanguages();
    //    }

    //    public byte[] DownloadJsonLanguage()
    //    {
    //        return File.ReadAllBytes("lang.json");
    //    }

    //    public void UpdateLang(RequestLang request)
    //    {
    //        await ValidateInputRequest(request);
    //        JsonLanguage.UpdateLanguage(request.Code, request.Id, request.En);
    //    }

    //    public void UpdateLang(List<RequestLang> request)
    //    {
    //        await ValidateInputRequest(request);
    //        foreach (var item in request)
    //        {
    //            if (string.IsNullOrWhiteSpace(item.Code) || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.En))
    //                throw new ApiException(LangCodes.InputEmpty);
    //        }

    //        var lang = request.CopyProperties<List<JsonLanguage>>();

    //        if (lang.Count == 0)
    //            throw new ApiException(LangCodes.InputEmpty);

    //        JsonLanguage.UpdateLanguage(lang);
    //    }

    //    public async Task UploadJson(IFormFile file)
    //    {
    //        if (file == null)
    //            throw new Exception(LangCodes.InputEmpty);

    //        using var stream = file.OpenReadStream();

    //        if (stream == null || stream.Length == 0)
    //            throw new Exception(LangCodes.InputEmpty);

    //        using var reader = new StreamReader(stream);
    //        var json = await reader.ReadToEndAsync();

    //        if (string.IsNullOrWhiteSpace(json) || json == "[]" || json == "{}")
    //            throw new ApiException(LangCodes.InputEmpty);

    //        try
    //        {
    //            var doc = JsonDocument.Parse(json);

    //            var requests = JsonSerializer.Deserialize<List<RequestLang>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    //            UpdateLang(requests);
    //        }
    //        catch (JsonException)
    //        {
    //            throw new ApiException(LangCodes.InputInvalidFormat);
    //        }
    //    }

    //    public void UploadExcelLang(IFormFile file)
    //    {
    //        if (file == null)
    //            throw new Exception(LangCodes.InputEmpty);

    //        using var stream = file.OpenReadStream();

    //        if (stream == null || stream.Length == 0)
    //            throw new Exception(LangCodes.InputEmpty);

    //        var requests = stream.ExcelToEntity<RequestLang>().ToList();
    //        UpdateLang(requests);
    //    }

    //    public JsonLanguage GetMenu(string code)
    //    {
    //        if (string.IsNullOrWhiteSpace(code))
    //            throw new ApiException(LangCodes.InputEmpty);

    //        return JsonLanguage.GetMenu(code);
    //    }

    //    public List<JsonLanguage> GetMenus() => JsonLanguage.GetMenus();

    //    public byte[] DownloadJsonMenu() => File.ReadAllBytes("lang_menu.json");

    //    public void UpdateMenu(RequestLang request)
    //    {
    //        await ValidateInputRequest(request);
    //        JsonLanguage.UpdateMenu(request.Code, request.Id, request.En);
    //    }

    //    public void UpdateMenu(List<RequestLang> request)
    //    {
    //        await ValidateInputRequest(request);
    //        foreach (var item in request)
    //        {
    //            if (string.IsNullOrWhiteSpace(item.Code) || string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.En))
    //                throw new ApiException(LangCodes.InputEmpty);
    //        }

    //        var lang = request.CopyProperties<List<JsonLanguage>>();

    //        if (lang.Count == 0)
    //            throw new ApiException(LangCodes.InputEmpty);

    //        JsonLanguage.UpdateMenu(lang);
    //    }

    //    public void UploadExcelMenu(IFormFile file)
    //    {
    //        if (file == null)
    //            throw new Exception(LangCodes.InputEmpty);

    //        using var stream = file.OpenReadStream();

    //        if (stream == null || stream.Length == 0)
    //            throw new Exception(LangCodes.InputEmpty);

    //        var requests = stream.ExcelToEntity<RequestLang>().ToList();
    //        UpdateMenu(requests);
    //    }
    //}
}
