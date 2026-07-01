using CelebrateHub.Services.DTOs;

namespace CelebrateHub.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
        Task<IEnumerable<BirthdayDto>> GetTodaysBirthdaysAsync();
        Task<IEnumerable<BirthdayDto>> GetUpcomingBirthdaysAsync(int days = 7);
        Task<IEnumerable<AnniversaryDto>> GetTodaysAnniversariesAsync();
        Task<IEnumerable<AnniversaryDto>> GetUpcomingAnniversariesAsync(int days = 7);
    }
}