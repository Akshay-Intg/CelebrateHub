using CelebrateHub.Services.DTOs;
using CelebrateHubMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CelebrateHubMVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApiClientService _api;

        public DashboardController(ApiClientService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var dashboard = await _api.GetDashboardAsync() ?? new DashboardDto();
            return View(dashboard);
        }
    }
}