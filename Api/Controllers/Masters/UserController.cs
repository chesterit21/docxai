namespace Docubase.api.Controllers.Masters
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Domain.EntityRequests.Authentications;
    using Api.Services.Masters;
    using Microsoft.AspNetCore.Mvc;
    using System.ComponentModel;
    using System.Net;


    [DisplayName("#3 Master User - User")]
    [Menu("MnMsUser")]
    [Route("[controller]")]
    [ApiController]
    public class UserController(UserService service) : ControllerBase
    {
        [UserAction(UserAction.Read)]
        [HttpGet("id")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await service.GetUserAsResponse(id);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Read)]
        [HttpGet("email")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var result = await service.GetUserAsResponse(email);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Read)]
        [HttpGet("{page}/{limit}")]
        public async Task<IActionResult> GetUsers(int page, int limit)
        {
            var result = await service.GetUsersAsResponse(page, limit);
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
        public async Task<IActionResult> CreateUser(RequestUserCreate request)
        {
            await service.Create(request);
            return ResultFactory.Create("Please, verify your account", HttpStatusCode.OK);
        }

        [UserAction(UserAction.Update)]
        [HttpPut]
        public async Task<IActionResult> UpdateUser(RequestUserUpdate request)
        {
            var result = await service.Update(request);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Delete)]
        [HttpPut("delete")]
        public async Task<IActionResult> Delete(int userId)
        {
            var result = await service.SoftDelete(userId);
            //var result = await service.DeleteWithChildren(ids);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Update)]
        [HttpPut("undelete")]
        public async Task<IActionResult> UnDelete(int userId)
        {
            var result = await service.SoftUndelete(userId);
            return ResultFactory.Create(result);
        }
    }
}
