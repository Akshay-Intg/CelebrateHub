using CelebrateHub.Data.Repositories.Interfaces;
using CelebrateHub.Services.Interfaces;

namespace CelebrateHubAPI.BackgroundServices
{
    public class BirthdayEmailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<BirthdayEmailBackgroundService> _logger;

        // Send emails at 9:00 AM every day
        private static readonly TimeSpan SendTime = new(9, 0, 0);

        public BirthdayEmailBackgroundService(
            IServiceProvider services,
            ILogger<BirthdayEmailBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Birthday Email Service started. " +
                "Emails will be sent daily at {Time}.", SendTime);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Calculate delay until next 9 AM
                var delay = CalculateDelay();

                _logger.LogInformation(
                    "Next birthday emails scheduled in {Hours}h {Minutes}m.",
                    (int)delay.TotalHours, delay.Minutes);

                // Wait until send time
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                // Send all emails for today
                await SendTodaysEmailsAsync();
            }
        }

        private async Task SendTodaysEmailsAsync()
        {
            _logger.LogInformation(
                "Running daily birthday/anniversary email job at {Time}",
                DateTime.Now);

            // Use a new DI scope for each execution — background services
            // are singletons, but DbContext/repositories are scoped,
            // so we must create a fresh scope each time
            using var scope = _services.CreateScope();

            var empRepo = scope.ServiceProvider
                .GetRequiredService<IEmployeeRepository>();
            var emailService = scope.ServiceProvider
                .GetRequiredService<IEmailService>();

            var today = DateTime.Today;
            var employees = await empRepo.GetAllAsync();
            var empList = employees.ToList();

            int birthdayCount = 0;
            int anniversaryCount = 0;

            foreach (var emp in empList)
            {
                // ── Birthday check ────────────────────────────────────────────
                if (emp.DateOfBirth.Month == today.Month &&
                    emp.DateOfBirth.Day == today.Day)
                {
                    var age = today.Year - emp.DateOfBirth.Year;

                    _logger.LogInformation(
                        "Sending birthday email to {Name} ({Email}) — turning {Age}",
                        emp.Name, emp.Email, age);

                    await emailService.SendBirthdayWishAsync(
                        emp.Email, emp.Name, age, emp.Department);

                    birthdayCount++;
                }

                // ── Anniversary check ─────────────────────────────────────────
                if (emp.AnniversaryDate.HasValue &&
                    emp.AnniversaryDate.Value.Month == today.Month &&
                    emp.AnniversaryDate.Value.Day == today.Day)
                {
                    var years = today.Year - emp.AnniversaryDate.Value.Year;

                    // Only send if at least 1 year (avoid day-0 emails)
                    if (years >= 1)
                    {
                        _logger.LogInformation(
                            "Sending anniversary email to {Name} ({Email}) — {Years} years",
                            emp.Name, emp.Email, years);

                        await emailService.SendAnniversaryWishAsync(
                            emp.Email, emp.Name, years, emp.Department);

                        anniversaryCount++;
                    }
                }
            }

            _logger.LogInformation(
                "Email job complete. Sent {B} birthday and {A} anniversary emails.",
                birthdayCount, anniversaryCount);
        }

        /// <summary>
        /// Calculates how long to wait until the next 9:00 AM.
        /// If it's currently before 9 AM → wait until 9 AM today.
        /// If it's currently after 9 AM → wait until 9 AM tomorrow.
        /// </summary>
        private static TimeSpan CalculateDelay()
        {
            var now = DateTime.Now;
            var nextRun = DateTime.Today.Add(SendTime);

            // If 9 AM today has already passed, schedule for tomorrow
            if (now > nextRun)
                nextRun = nextRun.AddDays(1);

            return nextRun - now;
        }
    }
}