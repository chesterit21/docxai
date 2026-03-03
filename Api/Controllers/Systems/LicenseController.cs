using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Extensions.Services;
using Api.Domain.EntityRequests.Systems;
using Api.Repository.Masters;

namespace Api.Controllers.Systems
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "superadmin")]
    public class LicenseController : ControllerBase
    {
        private readonly ILicenseManager _licenseManager;
        private readonly IUserRepository _userRepository;

        public LicenseController(
            ILicenseManager licenseManager,
            IUserRepository userRepository)
        {
            _licenseManager = licenseManager;
            _userRepository = userRepository;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetLicenseInfo()
        {
            var currentUserCount = await _userRepository.CountAsync();
            var info = _licenseManager.GetLicenseInfo();
            
            return Ok(new
            {
                info.MaxUserCount,
                info.ExpiryDate,
                info.IsValid,
                CurrentUserCount = currentUserCount,
                RemainingLicenses = _licenseManager.GetRemainingLicenses((int)currentUserCount)
            });
        }
    }
}