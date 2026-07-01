using CelebrateHub.Services.DTOs;
using CelebrateHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CelebrateHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service) => _service = service;

        /// <summary>GET /api/dashboard — Full dashboard data in one call.</summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _service.GetDashboardAsync();
            return Ok(ApiResponse<DashboardDto>.Ok(data));
        }

        /// <summary>GET /api/dashboard/todays-birthdays</summary>
        [HttpGet("todays-birthdays")]
        public async Task<IActionResult> TodaysBirthdays()
        {
            var data = await _service.GetTodaysBirthdaysAsync();
            return Ok(ApiResponse<IEnumerable<BirthdayDto>>.Ok(data));
        }

        /// <summary>GET /api/dashboard/upcoming-birthdays?days=7</summary>
        [HttpGet("upcoming-birthdays")]
        public async Task<IActionResult> UpcomingBirthdays([FromQuery] int days = 7)
        {
            if (days < 1 || days > 30)
                return BadRequest(ApiResponse<object>.Fail("Days must be between 1 and 30."));

            var data = await _service.GetUpcomingBirthdaysAsync(days);
            return Ok(ApiResponse<IEnumerable<BirthdayDto>>.Ok(data));
        }

        /// <summary>GET /api/dashboard/todays-anniversaries</summary>
        [HttpGet("todays-anniversaries")]
        public async Task<IActionResult> TodaysAnniversaries()
        {
            var data = await _service.GetTodaysAnniversariesAsync();
            return Ok(ApiResponse<IEnumerable<AnniversaryDto>>.Ok(data));
        }

        /// <summary>GET /api/dashboard/upcoming-anniversaries?days=7</summary>
        [HttpGet("upcoming-anniversaries")]
        public async Task<IActionResult> UpcomingAnniversaries([FromQuery] int days = 7)
        {
            if (days < 1 || days > 30)
                return BadRequest(ApiResponse<object>.Fail("Days must be between 1 and 30."));

            var data = await _service.GetUpcomingAnniversariesAsync(days);
            return Ok(ApiResponse<IEnumerable<AnniversaryDto>>.Ok(data));
        }
    }
}