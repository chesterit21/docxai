namespace Docubase.api.Controllers.Systems
{
    using Api.Domain;
    using Api.Domain.Attributes;
    using Api.Domain.EntityRequests;
    using Api.Services.Systems;
    using Microsoft.AspNetCore.Mvc;

    [Menu("MnSystemLog")]
    [Route("[controller]")]
    [ApiController]
    public class TransactionLogController(LogTransactionService service) : ControllerBase
    {
        [UserAction(UserAction.Read)]
        [HttpGet("filter")]
        public async Task<IActionResult> GetFIlter([FromQuery] RequestSystemLogsPagination filter)
        {
            DateTime st = DateTime.Today;
            if (filter.StartDate != null)
                st = filter.StartDate.Value;
			else
				st = st.AddDays(-1);

			DateTime end = DateTime.Today;
			if (filter.EndDate != null)
				end = filter.EndDate.Value;
			//else
			//	end = st.AddDays(-1);

			var result = await service.Get(st, end, filter.Page, filter.Limit);
            return ResultFactory.Create(result);
        }
    }
}
