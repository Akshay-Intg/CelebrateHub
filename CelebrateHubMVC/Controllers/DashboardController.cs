// CelebrateHubMVC/Controllers/DashboardController.cs
// Replace existing Index action

using CelebrateHub.Services.DTOs;
using CelebrateHubMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BirthdayPortal.MVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApiClientService _api;

        public DashboardController(ApiClientService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var dashboard = await _api.GetDashboardAsync()
                            ?? new DashboardDto();

            // Check if today is the logged-in user's birthday/anniversary
            var myToday = await _api.GetMyTodayAsync();
            System.Diagnostics.Debug.WriteLine(
    $"Birthday={myToday?.IsBirthday}, Anniversary={myToday?.IsAnniversary}");

            if (myToday != null)
            {
                ViewBag.IsBirthdayToday = myToday.IsBirthday;
                ViewBag.IsAnniversaryToday = myToday.IsAnniversary;
                ViewBag.TurningAge = myToday.TurningAge;
                ViewBag.YearsOfService = myToday.YearsOfService;
                ViewBag.Department = myToday.Department;
            }
            else
            {
                ViewBag.IsBirthdayToday = false;
                ViewBag.IsAnniversaryToday = false;
                ViewBag.TurningAge = 0;
                ViewBag.YearsOfService = 0;
                ViewBag.Department = "";
            }

            return View(dashboard);
        }
    }
}