using CelebrateHub.Data.Models;
using CelebrateHub.Data.Repositories.Interfaces;
using CelebrateHub.Services.DTOs;
using CelebrateHub.Services.Interfaces;

namespace CelebrateHub.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IEmployeeRepository _repo;

        public DashboardService(IEmployeeRepository repo) => _repo = repo;

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var all = await _repo.GetAllAsync();

            return new DashboardDto
            {
                TodaysBirthdays = await GetTodaysBirthdaysAsync(),
                UpcomingBirthdays = await GetUpcomingBirthdaysAsync(),
                TodaysAnniversaries = await GetTodaysAnniversariesAsync(),
                UpcomingAnniversaries = await GetUpcomingAnniversariesAsync(),
                TotalEmployees = all.Count()
            };
        }

        public async Task<IEnumerable<BirthdayDto>> GetTodaysBirthdaysAsync()
        {
            var employees = await _repo.GetTodaysBirthdaysAsync();
            return employees.Select(e => ToBirthdayDto(e, 0));
        }

        public async Task<IEnumerable<BirthdayDto>> GetUpcomingBirthdaysAsync(int days = 7)
        {
            var employees = await _repo.GetUpcomingBirthdaysAsync(days);
            return employees.Select(e => ToBirthdayDto(e, DaysUntilNextOccurrence(e.DateOfBirth)));
        }

        public async Task<IEnumerable<AnniversaryDto>> GetTodaysAnniversariesAsync()
        {
            var employees = await _repo.GetTodaysAnniversariesAsync();
            return employees.Select(e => ToAnniversaryDto(e, 0));
        }

        public async Task<IEnumerable<AnniversaryDto>> GetUpcomingAnniversariesAsync(int days = 7)
        {
            var employees = await _repo.GetUpcomingAnniversariesAsync(days);
            return employees.Select(e => ToAnniversaryDto(e, DaysUntilNextOccurrence(e.AnniversaryDate!.Value)));
        }

        // ── Mapping helpers ────────────────────────────────────────────────────
        
        private static BirthdayDto ToBirthdayDto(Employee e, int daysUntil)
        {
            var today = DateTime.Today;
            int turningAge = today.Year - e.DateOfBirth.Year + (daysUntil > 0 ? 0 : 0);
            // Calculate what age they'll be on their next birthday
            var nextBirthday = new DateTime(today.Year, e.DateOfBirth.Month, e.DateOfBirth.Day);
            if (nextBirthday < today) nextBirthday = nextBirthday.AddYears(1);
            turningAge = nextBirthday.Year - e.DateOfBirth.Year;

            return new BirthdayDto
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                Department = e.Department,
                DateOfBirth = e.DateOfBirth,
                DaysUntil = daysUntil,
                TurningAge = turningAge
            };
        }

        private static AnniversaryDto ToAnniversaryDto(Employee e, int daysUntil)
        {
            var today = DateTime.Today;
            var ann = e.AnniversaryDate!.Value;
            var nextAnn = new DateTime(today.Year, ann.Month, ann.Day);
            if (nextAnn < today) nextAnn = nextAnn.AddYears(1);
            int years = nextAnn.Year - ann.Year;

            return new AnniversaryDto
            {
                EmployeeId = e.EmployeeId,
                Name = e.Name,
                Department = e.Department,
                AnniversaryDate = ann,
                DaysUntil = daysUntil,
                YearsCompleting = years
            };
        }

        private static int DaysUntilNextOccurrence(DateTime date)
        {
            var today = DateTime.Today;
            var next = new DateTime(today.Year, date.Month, date.Day);
            if (next <= today) next = next.AddYears(1);
            return (next - today).Days;
        }
    }
}