namespace Docubase.api.Controllers.Masters
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Domain.EntityRequests.Authentications;
	using Api.Domain.EntityRequests.Masters;
	using Api.Services.Masters;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using System.ComponentModel;
    using System.Net;

	[AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    public class VerificationController(UserService service) : ControllerBase
    {

		//[UserAction(UserAction.Insert)]
		//[ApiExplorerSettings(IgnoreApi = true)]
		[AllowAnonymous]
		[HttpPut("verify")]
		public async Task<IActionResult> VerifyEmail([FromQuery]string xd)
		{
			var result = await service.VerifyEmail(xd);
			return ResultFactory.Create(result);
		}
	}
}
