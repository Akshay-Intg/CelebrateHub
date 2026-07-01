using CelebrateHub.Data.Models;
using CelebrateHub.Data.Repositories.Interfaces;
using CelebrateHub.Services.DTOs;
using CelebrateHub.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CelebrateHub.Services
{
    /// <summary>
    /// Handles registration, login (BCrypt verify + JWT issue), and password changes.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IEmployeeRepository _repo;
        private readonly IConfiguration _config;

        public AuthService(IEmployeeRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Prevent duplicate email
            if (await _repo.EmailExistsAsync(dto.Email))
                return new AuthResponseDto { Success = false, Message = "Email already registered." };

            var employee = new Employee
            {
                Name = dto.Name,
                Email = dto.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                DateOfBirth = dto.DateOfBirth,
                AnniversaryDate = dto.AnniversaryDate,
                Department = dto.Department,
                Role = "User",
                CreatedDate = DateTime.UtcNow
            };

            var created = await _repo.AddAsync(employee);
            var token = GenerateJwtToken(created);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful.",
                Token = token,
                Employee = MapToDto(created)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var employee = await _repo.GetByEmailAsync(dto.Email.ToLower());

            if (employee == null || !BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash))
                return new AuthResponseDto { Success = false, Message = "Invalid email or password." };

            var token = GenerateJwtToken(employee);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                Employee = MapToDto(employee)
            };
        }

        public async Task<bool> ChangePasswordAsync(int employeeId, ChangePasswordDto dto)
        {
            var employee = await _repo.GetByIdAsync(employeeId);
            if (employee == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, employee.PasswordHash))
                return false;

            employee.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _repo.UpdateAsync(employee);
            return true;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private string GenerateJwtToken(Employee employee)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new Claim(ClaimTypes.Name, employee.Name),
                new Claim(ClaimTypes.Email, employee.Email),
                new Claim(ClaimTypes.Role, employee.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static EmployeeDto MapToDto(Employee e) => new()
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