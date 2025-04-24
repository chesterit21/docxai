namespace Docubase.api.Controllers.Configurations
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests.Masters;
    using Api.Services.Systems;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.ComponentModel;

    [AllowAnonymous]
    [Menu("MnLang")]
    [DisplayName("Configurations - Language")]
    [Route("[controller]")]
    [ApiController]
    public class LanguageController(LanguageService languageService) : ControllerBase
    {
        readonly string messageType = "Message";
        readonly string menuType = "Menu";

        #region MESSAGE
        [AllowAnonymous]
        [HttpGet("message/all")]
        public async Task<IActionResult> GetLanguagesAsync()
        {
            var data = await languageService.GetLanguages(messageType);
            return ResultFactory.Create(data);
        }

        [AllowAnonymous]
        [HttpGet("message/code")]
        public async Task<ActionResult> GetLanguageAsync(string code)
        {
            var data = await languageService.GetLanguage(code, messageType);
            return ResultFactory.Create(data);
        }

        [AllowAnonymous]
        [HttpGet("message/download")]
        public async Task<IActionResult> DownloadLanguage()
        {
            var data = await languageService.DownloadJsonLanguageAsync(messageType);
            return File(data, "application/json", $"lang.json");
        }

        [Menu("MnLangMsg")]
        [HttpPut("message/update")]
        public async Task<ActionResult> UpdateLanguageAsync(RequestLang request)
        {
            await languageService.UpdateLanguage(request, messageType);
            return ResultFactory.Create("Success", System.Net.HttpStatusCode.OK);
        }

        [Menu("MnLangMsg")]
        [HttpPut("message/update-mass")]
        public async Task<ActionResult> UpdateMassLanguageAsync(List<RequestLang> requests)
        {
            await languageService.UpdateLanguage(requests, messageType);
            return ResultFactory.Create("Success", System.Net.HttpStatusCode.OK);
        }

        [Menu("MnLangMsg")]
        [HttpPost("message/upload-json")]
        public async Task<ActionResult> UploadJsonLanguage([FormFileValidations([".json", ".txt"], "5MB")] IFormFile file)
        {
            await languageService.UploadLanguageFromJson(file, messageType);
            return ResultFactory.Create("Success", System.Net.HttpStatusCode.OK);
        }

        [Menu("MnLangMsg")]
        [HttpPost("message/upload-excel")]
        public async Task<ActionResult> UploadExcelLanguageAsync([FormFileValidations([".xlsx", ".xls"], "20MB")] IFormFile file)
        {
            await languageService.UploadLanguageFromExcel(file, messageType);
            return ResultFactory.Create("Success", System.Net.HttpStatusCode.OK);
        }
        #endregion

        #region MENU
        [AllowAnonymous]
        [HttpGet("menu/all")]
        public async Task<IActionResult> GetLanguageMenusAsync()
        {
            var data = await languageService.GetLanguages(menuType);
            return ResultFactory.Create(data);
        }

        [AllowAnonymous]
        [HttpGet("menu/code")]
        public async Task<ActionResult> GetLanguageMenuAsync(string code)
        {
            var data = await languageService.GetLanguage(code, menuType);
            return ResultFactory.Create(data);
        }

        [Menu("MnLangMenu")]
        [HttpGet("menu/download")]
        public async Task<IActionResult> DownloadMenuAsync()
        {
            var data = await languageService.DownloadJsonLanguageAsync(menuType);
            return File(data, "application/json", $"lang.json");
        }

        [Menu("MnLangMenu")]
        [HttpPut("menu/update")]
        public async Task<ActionResult> UpdateMenuAsync(RequestLang request)
        {
            await languageService.UpdateLanguage(request, menuType);
            return ResultFactory.Create("Success", System.Net.HttpStatusCode.OK);
        }

        [Menu("MnLangMenu")]
        [HttpPost("menu/upload-excel")]
        public async Task<ActionResult> UploadExcelMenuAsync([FormFileValidations([".xlsx", ".xls"], "20MB")] IFormFile file)
        {
            await languageService.UploadLanguageFromExcel(file, menuType);
            return ResultFactory.Create("Success", System.Net.HttpStatusCode.OK);
        }
        #endregion
    }
}
