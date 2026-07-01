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
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService service, ILogger<EmployeesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/employees  — Returns all employees (optionally filtered by ?search=).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            IEnumerable<EmployeeDto> employees = string.IsNullOrWhiteSpace(search)
                ? await _service.GetAllAsync()
                : await _service.SearchAsync(search);

            return Ok(ApiResponse<IEnumerable<EmployeeDto>>.Ok(employees));
        }

        /// <summary>GET /api/employees/{id}</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            // Users can only view their own profile; admins can view anyone
            if (!IsAdminOrSelf(id))
                return Forbid();

            var emp = await _service.GetByIdAsync(id);
            if (emp == null) return NotFound(ApiResponse<EmployeeDto>.Fail("Employee not found."));

            return Ok(ApiResponse<EmployeeDto>.Ok(emp));
        }

        /// <summary>PUT /api/employees/{id} — Update employee details.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Validation failed."));

            // Users can only update themselves; admins can update anyone
            if (!IsAdminOrSelf(id))
                return Forbid();

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(ApiResponse<EmployeeDto>.Ok(updated, "Profile updated successfully."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<EmployeeDto>.Fail("Employee not found."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<EmployeeDto>.Fail(ex.Message));
            }
        }

        /// <summary>DELETE /api/employees/{id} — Admin only (soft delete).</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                _logger.LogInformation("Employee {Id} soft-deleted by admin {Admin}",
                    id, User.FindFirst(ClaimTypes.Email)?.Value);
                return Ok(ApiResponse<object>.Ok(new { }, "Employee deleted."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<object>.Fail("Employee not found."));
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Returns true if caller is an Admin, or the employee themselves.</summary>
        private bool IsAdminOrSelf(int targetId)
        {
            if (User.IsInRole("Admin")) return true;
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int callerId) && callerId == targetId;
        }
    }
}