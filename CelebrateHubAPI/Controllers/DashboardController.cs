using CelebrateHub.Data.Repositories.Interfaces;
using CelebrateHub.Services.DTOs;
using CelebrateHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CelebrateHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        private readonly IEmployeeRepository _empRepo;  // ← add

        public DashboardController(IDashboardService service,
            IEmployeeRepository empRepo)                // ← add
        {
            _service = service;
            _empRepo = empRepo;
        }

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



        // Add to CelebrateHubAPI/Controllers/DashboardController.cs

        [HttpGet("my-today")]
        public async Task<IActionResult> MyToday()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int employeeId))
                return Unauthorized();

            var emp = await _empRepo.GetByIdAsync(employeeId);
            if (emp == null)
                return NotFound();

            var today = DateTime.Today;

            bool isBirthday =
                emp.DateOfBirth.Month == today.Month &&
                emp.DateOfBirth.Day == today.Day;

            bool isAnniversary =
                emp.AnniversaryDate.HasValue &&
                emp.AnniversaryDate.Value.Month == today.Month &&
                emp.AnniversaryDate.Value.Day == today.Day;

            var dto = new MyTodayDto
            {
                IsBirthday = isBirthday,
                IsAnniversary = isAnniversary,
                TurningAge = isBirthday ? today.Year - emp.DateOfBirth.Year : 0,
                YearsOfService = isAnniversary
                    ? today.Year - emp.AnniversaryDate!.Value.Year
                    : 0,
                Department = emp.Department ?? ""
            };

            return Ok(ApiResponse<MyTodayDto>.Ok(dto));
        }
    }
}