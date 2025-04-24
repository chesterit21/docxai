namespace Docubase.api.Controllers.Users
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Services.Masters;
    using Microsoft.AspNetCore.Mvc;

    [Menu("MnAdmin")]
    [Route("[controller]")]
    [ApiController]
    public class MatrixController(IHttpContextAccessor accessor, UserMatrixService userMatrixService) : ControllerBase
    {
        [UserAction(UserAction.Read)]
        [HttpGet]
        public async Task<IActionResult> GetUserMatrix()
        {
            var userId = int.Parse(accessor?.HttpContext?.User?.Identity?.Name ?? "0");
            var result = await userMatrixService.GetFinalMatrixMenu(userId);
            return ResultFactory.Create(result);
        }

        [UserAction(UserAction.Read)]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserMatrix(int userId)
        {
            var result = await userMatrixService.GetFinalMatrixUser(userId);
            return ResultFactory.Create(result);
        }
    }
}
