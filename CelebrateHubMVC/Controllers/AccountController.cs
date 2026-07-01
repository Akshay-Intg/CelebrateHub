using CelebrateHub.Services.DTOs;
using CelebrateHubMVC.Models;
using CelebrateHubMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CelebrateHubMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClientService _api;

        public AccountController(ApiClientService api) => _api = api;
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _api.LoginAsync(new LoginDto
            {
                Email = model.Email,
                Password = model.Password
            });

            if (result == null || !result.Success || result.Token == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Store JWT in session (forwarded to API on every call)
            HttpContext.Session.SetString("JWT", result.Token);

            // Sign in with cookie auth so [Authorize] works on MVC actions
            await SignInWithCookie(result.Token, result.Employee!);

            return RedirectToLocal(model.ReturnUrl);
        }

        // ── Register ───────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _api.RegisterAsync(new RegisterDto
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password,
                DateOfBirth = model.DateOfBirth,
                AnniversaryDate = model.AnniversaryDate,
                Department = model.Department
            });

            if (result == null || !result.Success || result.Token == null)
            {
                ModelState.AddModelError("", "Registration failed. Email may already be registered.");
                return View(model);
            }

            HttpContext.Session.SetString("JWT", result.Token);
            await SignInWithCookie(result.Token, result.Employee!);

            return RedirectToAction("Index", "Dashboard");
        }

        // ── Logout ─────────────────────────────────────────────────────────────

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ── Change Password ────────────────────────────────────────────────────

        [Authorize, HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var success = await _api.ChangePasswordAsync(new ChangePasswordDto
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword
            });

            if (!success)
            {
                ModelState.AddModelError("", "Current password is incorrect.");
                return View(model);
            }

            TempData["Success"] = "Password changed successfully. Please log in again.";
            await Logout();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();

        // ── Helpers ────────────────────────────────────────────────────────────

        private async Task SignInWithCookie(string token, EmployeeDto employee)
        {
            // Parse JWT to extract expiry
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                new(ClaimTypes.Name, employee.Name),
                new(ClaimTypes.Email, employee.Email),
                new(ClaimTypes.Role, employee.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = jwt.ValidTo
                });
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard");
        }
    }
}