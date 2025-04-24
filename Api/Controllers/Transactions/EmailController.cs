namespace Docubase.api.Controllers.Transactions
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Services.Systems;
    using Microsoft.AspNetCore.Mvc;

    [Menu("MnEmail")]
    [Route("[controller]")]
    [ApiController]
    public class EmailController(EmailService service) : ControllerBase
    {
        [UserAction(UserAction.Read)]
        [HttpGet("filter")]
        public async Task<IActionResult> GetFIlter([FromQuery] ReqestFilter filter)
        {
            var result = await service.GetAll(filter);
            return ResultFactory.Create(result);
        }

        [HttpPost("direct")]
        public async Task<IActionResult> SendEmail(RequestEmail request)
        {
            await service.SendEmailDirect(request);
            return Ok();
        }

        [HttpPost("dump")]
        public async Task<IActionResult> SendEmailTable(RequestEmail request)
        {
            await service.SendEmailTable(request);
            return Ok();
        }
    }
}
