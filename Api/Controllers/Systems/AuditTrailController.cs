namespace Docubase.api.Controllers.Systems
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Services.Systems;
    using Microsoft.AspNetCore.Mvc;
    using Api.Domain.EntityRequests.Systems;


	[Menu("MnSystemLog")]
    [Route("[controller]")]
    [ApiController]
    public class AuditTrailController(LogAuditTrailService service) : ControllerBase
    {
        [UserAction(UserAction.Read)]
        [HttpGet("filter")]
        public async Task<IActionResult> GetFIlter([FromQuery] RequestAuditTrailLogsPagination filter)
        {
            var result = await service.GetByTransactionLogID(filter);
            return ResultFactory.Create(result);
        }
    }
}
