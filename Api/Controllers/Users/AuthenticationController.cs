namespace Docubase.api.Controllers.Users
{
    using Api.Domain;
    using Api.Domain.Constants;
    using Api.Domain.EntityRequests.Authentications;
    using Api.Domain.EntityResponses.Users;
    using Api.Extensions;
    using Api.Services.Masters;
    using Api.Services.Systems;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    public class AuthenticationController(IHttpContextAccessor accessor, IConfiguration configuration, IMemoryCache memoryCache, UserService service, LanguageService language) : ControllerBase
    {
        [HttpPost("access-token")]
        public async Task<IActionResult> GetAccessToken([FromBody] RequestUserLogin request)
        {
            var loginAttempt = configuration["Login:Attempts"].ToInt32();
            var loginAllowed = configuration["Login:AllowedReloginAfterMinutes"].ToInt32();

            var cache = memoryCache.Get<RequestLoginAttempt>(request.UserName);
            if (cache == null)
            {
                cache = new RequestLoginAttempt
                {
                    UserName = request.UserName,
                    LastLogin = DateTime.Now,
                    LoginCount = 0
                };

                memoryCache.Set(request.UserName, cache, DateTimeOffset.Now.AddHours(1));
            }

            if (cache.LoginCount >= loginAttempt)
            {
                cache.LoginCount++;

                var minute = (DateTime.Now - cache.LastLogin).TotalMinutes;
                if (minute < loginAllowed)
                {
                    var mnt = ((loginAllowed - minute) * 60).ConvertToStringTime();
                    var lang = await language.GetLanguage(LangCodes.LoginExceeded);
                    return ResultFactory.Create($"{lang.En}. {cache.LoginCount} attempts. Login will be enabled in {mnt}", System.Net.HttpStatusCode.BadRequest);
                }
            }

            //ResponseLogin user = null;
            //var superAdmin = configuration["Login:SuperAdminUser"];
            //var superPswd = configuration["Login:SuperAdminPassword"].Decrypt();
            //if (request.UserName == superAdmin && request.Password == superPswd)
            //{
            //    user = new ResponseLogin
            //    {
            //        UserName = superAdmin,
            //        FullName = "Super Administrator",
            //        UserId = -1,
            //        CompanyId = "super-admin",
            //        CompanyName = "Super Administrator Company"
            //    };
            //}
            //else
            //{
            //    user = await service.ValidateUserLogin(request?.UserName, request?.Password);
            //}

            var user = await service.Login(request?.UserName, request?.Password);
            if (user == null)
            {
                cache.LoginCount++;
                memoryCache.Set(request.UserName, cache);

                var lang = await language.GetLanguage(LangCodes.LoginInvalid);
                return ResultFactory.Create($"{lang.En}. {cache.LoginCount} attempt(s)", System.Net.HttpStatusCode.Unauthorized);
            }

            memoryCache.Remove(request.UserName);

            var expiresIn = 120;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:SymmetricSecurityKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expired = DateTime.Now.AddMinutes(expiresIn);

            var exp = DateTimeOffset.Parse(expired.ToString("yyyy-MM-dd HH:mm:ss")).ToUnixTimeSeconds();
            var iat = DateTimeOffset.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).ToUnixTimeSeconds();
            var authClaims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.UserId.ToString()),
                    new(ClaimTypes.Country, "Indonesia"),
                    new(JwtRegisteredClaimNames.Name, user.FullName),
                    new("company_id", user.CompanyId),
                    new("company_name", user.CompanyName),
                    new(JwtRegisteredClaimNames.Exp, exp.ToString()),
                    new(JwtRegisteredClaimNames.Iat, iat.ToString()),
                    new(JwtRegisteredClaimNames.Sub, "SHUBA Application"),
                    new(JwtRegisteredClaimNames.Iss, "shuba.co.id"),
                    new(JwtRegisteredClaimNames.Email, request.UserName),
                    new(JwtRegisteredClaimNames.GivenName, user.UserName),
                    new(JwtRegisteredClaimNames.FamilyName, "PT. SHUBA MITRA SOLUSI"),
                    new(JwtRegisteredClaimNames.Website, "https://shuba.co.id")
                };

            var token = new JwtSecurityToken(
                "shuba.co.id",
                "shuba.co.id",
                authClaims,
                expires: expired,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return ResultFactory.Create(new
            {
                accessToken,
                expiresIn,
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = accessor.HttpContext?.User?.Identity?.Name.ToInt32() ?? 0;
            await service.Logout(userId);
            return ResultFactory.Create("Logout", System.Net.HttpStatusCode.OK);
        }
    }
}
