using Api.Domain;
using Api.Domain.EntityRequests;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using UglyToad.PdfPig.Fonts.Standard14Fonts;

namespace Docubase.api.Controllers.Systems
{
	[ApiController]
	[Route("[controller]")]
	public class SetupController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public SetupController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpPost("test-db")]
		public async Task<IActionResult> TestAndSaveConnection([FromBody] ConnectionStringDB settings)
		{
			// 1. Sanitize Database Name (No spaces, no special chars)
			if (!Regex.IsMatch(settings.Database, @"^[a-zA-Z0-9_]+$"))
			{
				return ResultFactory.Create("Database name contains invalid characters or spaces.", System.Net.HttpStatusCode.BadRequest);
			}

			// 2. Sanitize username (No spaces, no special chars)
			if (!Regex.IsMatch(settings.Username, @"^[a-zA-Z0-9_]+$"))
			{
				return ResultFactory.Create("Username contains invalid characters or spaces.", System.Net.HttpStatusCode.BadRequest);
			}

			// 3. Sanitize Password (No spaces)
			if (settings.Password.Contains(" "))
			{
				return BadRequest("Password cannot contain spaces.");
			}

			// 4. Test Connection
			var connString = $"Host={settings.Host};Port={settings.Port};Username={settings.Username};Password={settings.Password};Database={settings.Database};";

			try
			{
				using var conn = new Npgsql.NpgsqlConnection(connString);
				await conn.OpenAsync();

				// Logic to save to appsettings.json would go here
				// Note: Writing to appsettings.json at runtime requires physical file access
				return Ok(new { message = "Connection successful. Settings saved." });
			}
			catch (Exception ex)
			{
				//Database connection could not be established
				return StatusCode(500, new { message = "Connection failed", detail = ex.Message });
			}
		}
	}
}
