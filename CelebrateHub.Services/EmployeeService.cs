using CelebrateHub.Services.Interfaces;
using CelebrateHub.Data.Repositories.Interfaces;
using CelebrateHub.Services.DTOs;

namespace CelebrateHub.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo) => _repo = repo;

        public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
        {
            var employees = await _repo.GetAllAsync();
            return employees.Select(Map);
        }

        public async Task<EmployeeDto?> GetByIdAsync(int id)
        {
            var emp = await _repo.GetByIdAsync(id);
            return emp == null ? null : Map(emp);
        }

        public async Task<IEnumerable<EmployeeDto>> SearchAsync(string term)
        {
            var employees = await _repo.SearchAsync(term);
            return employees.Select(Map);
        }

        public async Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var emp = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Employee {id} not found.");

            // Check email uniqueness (exclude self)
            if (await _repo.EmailExistsAsync(dto.Email, id))
                throw new InvalidOperationException("Email is already in use by another employee.");

            emp.Name = dto.Name;
            emp.Email = dto.Email.ToLower();
            emp.DateOfBirth = dto.DateOfBirth;
            emp.AnniversaryDate = dto.AnniversaryDate;
            emp.Department = dto.Department;

            var updated = await _repo.UpdateAsync(emp);
            return Map(updated);
        }

        public async Task DeleteAsync(int id)
        {
            if (!await _repo.ExistsAsync(id))
                throw new KeyNotFoundException($"Employee {id} not found.");
            await _repo.DeleteAsync(id);
        }

        // ── Mapper ─────────────────────────────────────────────────────────────

        private static EmployeeDto Map(Data.Models.Employee e) => new()
        {
            EmployeeId = e.EmployeeId,
            Name = e.Name,
            Email = e.Email,
            DateOfBirth = e.DateOfBirth,
            AnniversaryDate = e.AnniversaryDate,
            Department = e.Department,
            Role = e.Role,
            CreatedDate = e.CreatedDate
        };
    }
}