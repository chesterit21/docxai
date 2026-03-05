using Api.Domain.Attributes;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.Miscellaneous;
using Api.Domain;
using Api.Services.Masters;
using Api.Services.Systems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace Docubase.api.Controllers
{
    //[Menu("MnCategories")]
    [Route("[controller]")]
    [ApiController]
    public class CategoryController(IMemoryCache memoryCache,
        UserService userService,
        MenuService menuService,
        LogTransactionService logTransactionService,
        LogAuditTrailService logAuditTrailService,
        IHubContext<MessageHub> hub) : ControllerBase
    {
        [UserAction(UserAction.Read)]
        [HttpGet("log-transaction")]
        public async Task<IActionResult> GetLogTransaction([FromQuery] DateTime startdate, DateTime enddate, int page, int limit)
        {
            var result = await logTransactionService.Get(startdate, enddate, page, limit);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Read)]
        [HttpGet("log-audit")]
        public async Task<IActionResult> GetLogAudit([FromQuery] DateTime startdate, DateTime enddate, int page, int limit)
        {
            var result = await logAuditTrailService.Get(startdate, enddate, page, limit);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Read)]
        [HttpGet("log-audit-search")]
        public async Task<IActionResult> GetLogAuditAdvancedSearch([FromQuery] RequestFilter request)
        {
            var result = await logAuditTrailService.Get(request);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Read)]
        [HttpPut("user-menu")]
        public async Task<IActionResult> GetByUserName(int userId)
        {
            var result = await menuService.GetNested(userId);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Update)]
        [HttpPut("user/reset-login-attempts")]
        public IActionResult ResetUserLogin(string username)
        {
            memoryCache.Remove(username);
            return ResultFactory.Create("Email has been reset", HttpStatusCode.OK);
        }

        [UserAction(UserAction.Update)]
        [HttpPut("user/force-verify-email")]
        public async Task<IActionResult> VerifyEmail(int userId)
        {
            var result = await userService.VerifyEmail(userId);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Insert)]
        [HttpPost("user/broadcast")]
        public async Task<IActionResult> BroadcastMessage(string message)
        {
            await hub.Clients.All.SendAsync("broadcast", message);
            return ResultFactory.Create(LangCodes.MessageSent, System.Net.HttpStatusCode.OK);
        }

        [UserAction(UserAction.Update)]
        [HttpPut("user/reset-login")]
        public async Task<IActionResult> Logout(int userId)
        {
            await userService.Logout(userId);
            return ResultFactory.Create("Logout", HttpStatusCode.OK);
        }
    }
}
