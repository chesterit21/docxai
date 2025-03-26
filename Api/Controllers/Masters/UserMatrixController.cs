namespace Docubase.api.Controllers.Masters
{
    using Api.DataAccess.Models.Masters;
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Domain.EntityRequests.Masters;
    using Api.Services.Masters;
    using Microsoft.AspNetCore.Mvc;
    using System.ComponentModel;

    [DisplayName("Privilege User - User Menu | Matrix")]
    [Menu("MnMsUserPriv")]
    [Route("[controller]")]
    [ApiController]
    public class UserMatrixController(UserMatrixService service) : ControllerBase
    {
        [UserAction(UserAction.Read)]
        [HttpGet]
        public async Task<IActionResult> Get(int userId)
        {
            var result = await service.Get(userId);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Read)]
        [HttpGet("filter")]
        public async Task<IActionResult> GetFIlter([FromQuery] ReqestFilter filter)
        {
            var result = await service.GetAll(filter);
            return ResultFactory.Create(result);

        }
        [UserAction(UserAction.Insert)]
        [HttpPost]
        public async Task<IActionResult> Post(RequestUserMatrix request)
        {
            await service.UpsertDelete(request);
            return ResultFactory.Create();
        }
    }
}
