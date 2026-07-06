using CelebrateHub.Data.Repositories.Interfaces;
using CelebrateHub.Services.DTOs;
using CelebrateHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CelebrateHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _email;
        private readonly IEmployeeRepository _empRepo;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IEmailService email,
            IEmployeeRepository empRepo,
            ILogger<EmailController> logger)
        {
            _email = email;
            _empRepo = empRepo;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/email/send-todays-wishes
        /// Manually triggers birthday and anniversary emails for today.
        /// Admin only.
        /// </summary>
        [HttpPost("send-todays-wishes")]
        public async Task<IActionResult> SendTodaysWishes()
        {
            var today = DateTime.Today;
            var employees = await _empRepo.GetAllAsync();
            var empList = employees.ToList();

            int birthdayCount = 0;
            int anniversaryCount = 0;
            var errors = new List<string>();

            foreach (var emp in empList)
            {
                try
                {
                    if (emp.DateOfBirth.Month == today.Month &&
                        emp.DateOfBirth.Day == today.Day)
                    {
                        var age = today.Year - emp.DateOfBirth.Year;
                        await _email.SendBirthdayWishAsync(
                            emp.Email, emp.Name, age, emp.Department);
                        birthdayCount++;
                    }

                    if (emp.AnniversaryDate.HasValue &&
                        emp.AnniversaryDate.Value.Month == today.Month &&
                        emp.AnniversaryDate.Value.Day == today.Day)
                    {
                        var years = today.Year - emp.AnniversaryDate.Value.Year;
                        if (years >= 1)
                        {
                            await _email.SendAnniversaryWishAsync(
                                emp.Email, emp.Name, years, emp.Department);
                            anniversaryCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{emp.Name}: {ex.Message}");
                    _logger.LogError(ex,
                        "Failed to send email to {Name}", emp.Name);
                }
            }

            return Ok(ApiResponse<object>.Ok(new
            {
                BirthdayEmailsSent = birthdayCount,
                AnniversaryEmailsSent = anniversaryCount,
                Errors = errors
            }, $"Sent {birthdayCount} birthday and " +
               $"{anniversaryCount} anniversary emails."));
        }

        /// <summary>
        /// POST /api/email/test
        /// Sends a test email to a specific address to verify SMTP config.
        /// </summary>
        [HttpPost("test")]
        public async Task<IActionResult> SendTestEmail([FromBody] string testEmail)
        {
            await _email.SendEmailAsync(
                testEmail,
                "Test Recipient",
                "✅ CelebrateHub Email Test",
                "<h2>It works!</h2><p>Your SMTP configuration is correct.</p>");

            return Ok(ApiResponse<object>.Ok(new { },
                $"Test email sent to {testEmail}"));
        }
    }
}