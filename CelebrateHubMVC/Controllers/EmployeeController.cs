using CelebrateHub.Services.DTOs;
using CelebrateHubMVC.Models;
using CelebrateHubMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BirthdayPortal.MVC.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly ApiClientService _api;

        public EmployeeController(ApiClientService api) => _api = api;

        private int CurrentUserId =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // ── Directory ──────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? search)
        {
            var employees = await _api.GetEmployeesAsync(search) ?? Enumerable.Empty<EmployeeDto>();
            ViewBag.Search = search;
            return View(employees);
        }

        // ── Profile ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Profile(int? id)
        {
            var currentId = CurrentUserId;
            var targetId = id ?? currentId;

            // Non-admins can only view their own profile — redirect nicely
            if (!User.IsInRole("Admin") && targetId != currentId)
                return RedirectToAction("AccessDenied", "Account");

            var emp = await _api.GetEmployeeAsync(targetId);
            if (emp == null) return NotFound();
            return View(emp);
        }

        // ── Edit ───────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentId = CurrentUserId;
            if (!User.IsInRole("Admin") && id != currentId)
                return RedirectToAction("AccessDenied", "Account");

            var emp = await _api.GetEmployeeAsync(id);
            if (emp == null) return NotFound();

            return View(new EditEmployeeViewModel
            {
                EmployeeId = emp.EmployeeId,
                Name = emp.Name,
                Email = emp.Email,
                DateOfBirth = emp.DateOfBirth,
                AnniversaryDate = emp.AnniversaryDate,
                Department = emp.Department
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditEmployeeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var currentId = CurrentUserId;
            if (!User.IsInRole("Admin") && model.EmployeeId != currentId)
                return RedirectToAction("AccessDenied", "Account");

            var updated = await _api.UpdateEmployeeAsync(model.EmployeeId, new UpdateEmployeeDto
            {
                Name = model.Name,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth,
                AnniversaryDate = model.AnniversaryDate,
                Department = model.Department
            });

            if (updated == null)
            {
                ModelState.AddModelError("", "Update failed. Email may already be in use.");
                return View(model);
            }

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile", new { id = model.EmployeeId });
        }

        // ── Delete (Admin only) ────────────────────────────────────────────────

        [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _api.DeleteEmployeeAsync(id);
            TempData[success ? "Success" : "Error"] = success
                ? "Employee deleted successfully."
                : "Failed to delete employee.";
            return RedirectToAction("Index");
        }
    }
}