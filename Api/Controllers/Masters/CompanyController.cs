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


    [DisplayName("Master Company")]
    [Menu("MnMsCompany")]
    [Route("[controller]")]
    [ApiController]
    public class CompanyController(CompanyService service) : ControllerBase
    {
		//[AllowAnonymous]
		//[HttpGet("{page}/{limit}")]
		//public async Task<IActionResult> GetCompany(int page, int limit)
		//{
		//	var result = await service.GetAll(page, limit);
		//	return ResultFactory.Create(result);
		//}

		[AllowAnonymous]
		[HttpGet("filter")]
        public async Task<IActionResult> GetFIlter([FromQuery] RequestFilter filter)
        {
            var result = await service.GetAll(filter);
            return ResultFactory.Create(result);
        }

		[AllowAnonymous]
		[HttpGet("get-by-id")]
		public async Task<IActionResult> GetSingle(string id)
		{
			var result = await service.Get(id);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> Post(RequestCompany request)
		{
			var result = await service.Insert(request);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpPut]
		public async Task<IActionResult> Put(RequestCompany request)
		{
			var result = await service.Update(request);
			return ResultFactory.Create(result);
		}

		[AllowAnonymous]
		[HttpPut("delete")]
        public async Task<IActionResult> Delete(string userId)
        {
            var result = await service.SoftDelete(userId);
            return ResultFactory.Create(result);
        }
    }
}
