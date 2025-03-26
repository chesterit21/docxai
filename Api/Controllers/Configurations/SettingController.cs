namespace Docubase.api.Controllers.Configurations
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Domain.EntityRequests.Authentications;
    using Api.Services.Systems;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.ComponentModel;


    [AllowAnonymous]
    [DisplayName("Configurations - Application Setting")]
    [Menu("MnSetting")]
    [Route("[controller]")]
    [ApiController]
    public class SettingController(IHttpContextAccessor accessor, IConfiguration configuration, LanguageService language, SettingService settingService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RequestUserLogin request)
        {
            var login = await settingService.Login(request);
            return ResultFactory.Create(login);
        }

        [UserAction(UserAction.Read)]
        [HttpGet("config")]
        public IActionResult GetSettingWithoutLogin()
        {
            var setting = settingService.GetConfig();
            return ResultFactory.Create(setting);
        }

        [UserAction(UserAction.Read)]
        [HttpGet]
        public async Task<IActionResult> GetSettingAsync()
        {
            await settingService.CheckToken();

            var setting = settingService.Get();
            return ResultFactory.Create(setting);
        }

        [UserAction(UserAction.Insert)]
        [HttpPost]
        public async Task<IActionResult> SaveSettingAsync([FromBody] RequestSetting request)
        {
            await settingService.CheckToken();

            settingService.Save(request);
            return ResultFactory.Create("Settings saved", System.Net.HttpStatusCode.OK);
        }
    }
}
