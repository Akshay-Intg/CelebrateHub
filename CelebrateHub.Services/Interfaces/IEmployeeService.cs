using CelebrateHub.Services.DTOs;

namespace CelebrateHub.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllAsync();
        Task<EmployeeDto?> GetByIdAsync(int id);
        Task<IEnumerable<EmployeeDto>> SearchAsync(string term);
        Task<EmployeeDto> UpdateAsync(int id, UpdateEmployeeDto dto);
        Task DeleteAsync(int id);
    }
}