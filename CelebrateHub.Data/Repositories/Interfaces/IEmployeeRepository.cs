using CelebrateHub.Data.Models;

namespace CelebrateHub.Data.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetByEmailAsync(string email);
        Task<IEnumerable<Employee>> SearchAsync(string term);

        // Birthday helpers — compare month+day only (ignores year)
        Task<IEnumerable<Employee>> GetTodaysBirthdaysAsync();
        Task<IEnumerable<Employee>> GetUpcomingBirthdaysAsync(int days = 7);

        // Anniversary helpers
        Task<IEnumerable<Employee>> GetTodaysAnniversariesAsync();
        Task<IEnumerable<Employee>> GetUpcomingAnniversariesAsync(int days = 7);

        Task<Employee> AddAsync(Employee employee);
        Task<Employee> UpdateAsync(Employee employee);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    }
}