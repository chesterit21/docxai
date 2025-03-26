namespace Docubase.api.Controllers.Masters
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Domain.EntityRequests.Masters;
    using Api.Services.Masters;
    using Microsoft.AspNetCore.Mvc;
    using System.ComponentModel;


    [DisplayName("## Master Menu")]
    [Menu("MnMsMenu")]
    [Route("[controller]")]
    [ApiController]
    public class MenuController(MenuService service) : ControllerBase
    {
        [UserAction(UserAction.Read)]
        [HttpGet]
        public async Task<IActionResult> Get(int level)
        {
            var result = await service.Get(level);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Read)]
        [HttpGet("nested")]
        public async Task<IActionResult> GetNested()
        {
            var result = await service.GetNested();
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Read)]
        [HttpGet("plain")]
        public async Task<IActionResult> GetAll()
        {
            var result = await service.Get();
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
        public async Task<IActionResult> Post(RequestMenu request)
        {
            var result = await service.Insert(request);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Update)]
        [HttpPut]
        public async Task<IActionResult> Put(RequestMenu request)
        {
            var result = await service.Update(request);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Delete)]
        [HttpPut("delete")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await service.SoftDelete(id);
            //var result = await service.DeleteWithChildren(ids);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Delete)]
        [HttpPut("undelete")]
        public async Task<IActionResult> UnDelete(string id)
        {
            var result = await service.SoftUndelete(id);
            return ResultFactory.Create(result);
        }
    }
}
