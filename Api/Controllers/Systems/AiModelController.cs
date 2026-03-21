namespace Docubase.api.Controllers.Systems
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Domain.EntityRequests.Systems;
    using Api.Services.Systems;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.ComponentModel;

    [DisplayName("AI Model Configuration")]
    [Menu("MnSyAiModel")]
    [Route("[controller]")]
    [ApiController]
    public class AiModelController(AiModelService service) : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("filter")]
        public async Task<IActionResult> GetFilter([FromQuery] RequestFilter filter)
        {
            var result = await service.GetAll(filter);
            return ResultFactory.Create(result);
        }

        [AllowAnonymous]
        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetSingle(Guid id)
        {
            var result = await service.Get(id);
            return ResultFactory.Create(result);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Post(RequestAiModel request)
        {
            var result = await service.Insert(request);
            return ResultFactory.Create(result);
        }

        [AllowAnonymous]
        [HttpPut]
        public async Task<IActionResult> Put(RequestAiModel request)
        {
            var result = await service.Update(request);
            return ResultFactory.Create(result);
        }

        [AllowAnonymous]
        [HttpPut("delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await service.SoftDelete(id);
            return ResultFactory.Create(result);
        }
    }
}
