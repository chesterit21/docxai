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


	[DisplayName("Master-User")]
	[Menu("MnUserGroup")]
	[Route("[controller]")]
	[ApiController]
	
	public class UserController(UserService service) : ControllerBase
	{
		[UserAction(UserAction.Read)]
		//[AllowAnonymous]
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

		//[UserAction(UserAction.Read)]
		//[HttpGet("{page}/{limit}")]
		//public async Task<IActionResult> GetUsers(int page, int limit)
		//{
		//    var result = await service.GetUsersAsResponse(page, limit);
		//    return ResultFactory.Create(result);
		//}

		//[UserAction(UserAction.Read)]
		//[HttpGet("filter")]
		//public async Task<IActionResult> GetFilter([FromQuery] RequestFilter filter)
		//{
		//    var result = await service.GetAll(filter);
		//    return ResultFactory.Create(result);
		//}

		[UserAction(UserAction.Read)]
		[HttpGet("filter")]
		public async Task<IActionResult> GetFilter([FromQuery] RequestUserList filter)
		{
			var result = await service.GetUserList(filter, false);
			return ResultFactory.Create(result);
		}

		//[UserAction(UserAction.Insert)]
		//[AllowAnonymous]
		//[HttpPost("CreateUser")]
		//public async Task<IActionResult> CreateNewUser(RequestNewUserCreate request)
		//{
		//	var result = await service.CreateNewUser(request);
		//	return ResultFactory.Create(result);
		//}

		//[UserAction(UserAction.Insert)]
		//[AllowAnonymous]
		//[HttpPut("UpdateUser")]
		//public async Task<IActionResult> UpdateNewUser(RequestUpdateNewUser request)
		//{
		//	var result = await service.UpdateNewUser(request);
		//	return ResultFactory.Create(result);
		//}

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

		//[AllowAnonymous]
		[UserAction(UserAction.Delete)]
		[HttpPut("delete")]
		public async Task<IActionResult> Delete(int userId)
		{
			var result = await service.SoftDelete(userId);
			//var result = await service.DeleteWithChildren(ids);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("recyclebin")]
		public async Task<IActionResult> GetDeleted([FromQuery] RequestUserList filter)
		{
			var result = await service.GetUserList(filter, true);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Update)]
		[HttpPut("undelete")]
		public async Task<IActionResult> UnDelete(int userId)
		{
			var result = await service.SoftUndelete(userId);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Delete)]
		[HttpDelete("hard-delete")]
		public async Task<IActionResult> HardDelete(int id)
		{
			var result = await service.HardDelete(id);
			return ResultFactory.Create();
		}

		[HttpDelete("empty-recyclebin")]
		public async Task<IActionResult> EmptyRecyclebin()
		{
			var result = await service.EmptyRecyclebin();
			return ResultFactory.Create(result);
		}

		#region Profile
		[UserAction(UserAction.Read)]
		[HttpGet("get-profile")]
		public async Task<IActionResult> GetProfile(int id)
		{
			var result = await service.GetProfileAsResponse(id);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Update)]
		[HttpPost("update-profile")]
		public async Task<IActionResult> UpdateProfile(RequestUpdateUserInfo request)
		{
			var result = await service.UpdateProfile(request);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		[UserAction(UserAction.Update)]
		[HttpPost("upload-profile")]
		public async Task<IActionResult> UploadProfile(IFormFile request)
		{
			var result = await service.UploadProfile(request);
			return ResultFactory.Create(result);
		}

		//[UserAction(UserAction.UpdateDocument)]
		[AllowAnonymous]
		[HttpPost("request-change-password")]
		public async Task<IActionResult> RequestChangePassword([FromBody] RequestResetPassword request)
		{
			var result = await service.SendVerificationResetPassword(request);
			return ResultFactory.Create(result);
		}

		//[AllowAnonymous]
		//[HttpGet("return-enc-reset-password")]
		//public async Task<IActionResult> ReturnEncResetPassword([FromQuery] string enc)
		//{
		//	var result = await service.ReturnEncResetPassword(enc);
		//	return ResultFactory.Create(result);
		//}

		[AllowAnonymous]
		[HttpPost("validate-reset-password")]
		public async Task<IActionResult> CheckVerificationCodeResetPassword([FromQuery] string vcode, [FromQuery] string enc)
		{
			var result = await service.CekVerificationCodeResetPassword(vcode, enc);
			return ResultFactory.Create(result);
		}

		//[UserAction(UserAction.UpdateDocument)]
		[AllowAnonymous]
		[HttpPost("change-password")]
		public async Task<IActionResult> ChangePassword(RequestUserIdUpdatePassword request)
		{
			var result = await service.UpdateResetPasswordProfile(request);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Update)]
		[HttpPost("change-password-profile")]
		public async Task<IActionResult> ChangePasswordProfile(RequestUserChangePasswordProfile request)
		{
			var result = await service.ChangePasswordProfile(request);
			return ResultFactory.Create(result);
		}

		#endregion
	}
}
