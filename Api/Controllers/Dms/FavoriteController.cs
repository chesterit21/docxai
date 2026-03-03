using Api.Domain;
using Api.Domain.Attributes;
using Api.Domain.EntityRequests.Dms;
using Api.Services.Dms;
using Api.Services.Masters;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Docubase.api.Controllers.Dms
{
	[DisplayName("Favorite")]
	[Menu("MnFavorite")]
	[Route("[controller]")]
	[ApiController]
	public class FavoriteController(FavoriteService service) : ControllerBase
	{

		[UserAction(UserAction.Read)]
		[HttpGet("get-fav-categories")]
		public async Task<IActionResult> GetFavCategories([FromQuery] RequestFavoritesCategories filter)
		{
			var result = await service.GetFavoriteCategories(filter);
			return ResultFactory.Create(result);
		}

		[UserAction(UserAction.Read)]
		[HttpGet("get-fav-document")]
		public async Task<IActionResult> GetFavDocuments([FromQuery] RequestFavoritesDocument filter)
		{
			var result = await service.GetFavoriteDocument(filter);
			return ResultFactory.Create(result);
		}

		// GET api/<FavoriteController>/5
		//[HttpGet("{id}")]
		//      public string Get(int id)
		//      {
		//          return "value";
		//      }

		// POST api/<FavoriteController>
		//[HttpPost]
		//public void Post([FromBody] string value)
		//{
		//}

		//// PUT api/<FavoriteController>/5
		//[HttpPut("{id}")]
		//public void Put(int id, [FromBody] string value)
		//{
		//}

		//// DELETE api/<FavoriteController>/5
		//[HttpDelete("{id}")]
		//public void Delete(int id)
		//{
		//}
	}
}
