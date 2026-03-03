namespace Docubase.api.Controllers.Transactions
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Services.Systems;
    using Microsoft.AspNetCore.Mvc;

	//[Menu("MnEmail")]
	[Menu("MnSystemLog")]
	[Route("[controller]")]
    [ApiController]
    public class EmailController(EmailService service) : ControllerBase
    {
        //[UserAction(UserAction.Read)]
        //[HttpGet("filter")]
        //public async Task<IActionResult> GetFIlter([FromQuery] RequestFilter filter)
        //{
        //    var result = await service.GetAll(filter);
        //    return ResultFactory.Create(result);
        //}

		[UserAction(UserAction.Read)]
		[HttpGet("filter")]
		public async Task<IActionResult> GetFIlter([FromQuery] RequestSystemLogsPagination filter)
		{
            DateTime st = DateTime.Today;
            if (filter.StartDate != null)
				st = filter.StartDate.Value; // filter.StartDate.Value;
			else
				st = st.AddDays(-1);

			DateTime end = DateTime.Now;
			if (filter.EndDate != null)
				end = filter.EndDate.Value;
			//else
			//	end = st.AddDays(-1);

			var result = await service.Get(st, end, filter.Page, filter.Limit);
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

		[HttpPut("resend")]
		public async Task<IActionResult> ReSendEmail([FromQuery]Guid id)
		{
			await service.ReSendEmailTable(id);
			return ResultFactory.Create();
		}

		[HttpPost("multi-resend")]
		public async Task<IActionResult> ReSendMultiEmail([FromBody] RequestResendMultiEmail ids)
		{
			await service.ReSendMultiEmailTable(ids);
			return ResultFactory.Create();
		}
	}
}
