using CelebrateHub.Data.Models;
using CelebrateHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CelebrateHub.Data.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _db;

        public EmployeeRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Employee>> GetAllAsync()
            => await _db.Employees
                        .Where(e => e.IsActive)
                        .OrderBy(e => e.Name)
                        .ToListAsync();

        public async Task<Employee?> GetByIdAsync(int id)
            => await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id && e.IsActive);

        public async Task<Employee?> GetByEmailAsync(string email)
            => await _db.Employees
                        .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower() && e.IsActive);

        public async Task<IEnumerable<Employee>> SearchAsync(string term)
        {
            term = term.ToLower().Trim();
            return await _db.Employees
                .Where(e => e.IsActive &&
                            (e.Name.ToLower().Contains(term) ||
                             e.Email.ToLower().Contains(term) ||
                             (e.Department != null && e.Department.ToLower().Contains(term))))
                .OrderBy(e => e.Name)
                .ToListAsync();
        }
        public async Task<IEnumerable<Employee>> GetTodaysBirthdaysAsync()
        {
            var today = DateTime.Today;
            return await _db.Employees
                .Where(e => e.IsActive
                            && e.DateOfBirth.Month == today.Month
                            && e.DateOfBirth.Day == today.Day)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetUpcomingBirthdaysAsync(int days = 7)
        {
            var today = DateTime.Today;
            var results = new List<Employee>();

            for (int i = 1; i <= days; i++)
            {
                var date = today.AddDays(i);
                var employees = await _db.Employees
                    .Where(e => e.IsActive
                                && e.DateOfBirth.Month == date.Month
                                && e.DateOfBirth.Day == date.Day)
                    .ToListAsync();
                results.AddRange(employees);
            }

            return results.OrderBy(e => GetNextOccurrence(e.DateOfBirth));
        }
        public async Task<IEnumerable<Employee>> GetTodaysAnniversariesAsync()
        {
            var today = DateTime.Today;
            return await _db.Employees
                .Where(e => e.IsActive
                            && e.AnniversaryDate.HasValue
                            && e.AnniversaryDate.Value.Month == today.Month
                            && e.AnniversaryDate.Value.Day == today.Day)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetUpcomingAnniversariesAsync(int days = 7)
        {
            var today = DateTime.Today;
            var results = new List<Employee>();

            for (int i = 1; i <= days; i++)
            {
                var date = today.AddDays(i);
                var employees = await _db.Employees
                    .Where(e => e.IsActive
                                && e.AnniversaryDate.HasValue
                                && e.AnniversaryDate.Value.Month == date.Month
                                && e.AnniversaryDate.Value.Day == date.Day)
                    .ToListAsync();
                results.AddRange(employees);
            }

            return results.OrderBy(e => GetNextOccurrence(e.AnniversaryDate!.Value));
        }
        public async Task<Employee> AddAsync(Employee employee)
        {
            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee> UpdateAsync(Employee employee)
        {
            _db.Employees.Update(employee);
            await _db.SaveChangesAsync();
            return employee;
        }

        public async Task DeleteAsync(int id)
        {
            var emp = await GetByIdAsync(id);
            if (emp != null)
            {
                emp.IsActive = false;  // Soft delete
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
            => await _db.Employees.AnyAsync(e => e.EmployeeId == id && e.IsActive);

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
            => await _db.Employees.AnyAsync(e =>
                e.Email.ToLower() == email.ToLower() &&
                e.IsActive &&
                (excludeId == null || e.EmployeeId != excludeId));

        // ── Private Helpers ────────────────────────────────────────────────────

        /// <summary>Returns the next calendar date matching month+day of the source date.</summary>
        private static DateTime GetNextOccurrence(DateTime source)
        {
            var today = DateTime.Today;
            var next = new DateTime(today.Year, source.Month, source.Day);
            if (next <= today) next = next.AddYears(1);
            return next;
        }
    }
}