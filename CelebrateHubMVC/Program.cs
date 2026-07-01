using CelebrateHubMVC.Filters;
using CelebrateHubMVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    // Catches the "valid cookie, missing session JWT" stale-login bug
    options.Filters.Add<RequireSessionTokenFilter>();
});

// ── HttpClient for calling the API ────────────────────────────────────────────
builder.Services.AddHttpClient("API", client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7160/api/";
    if (!apiBaseUrl.EndsWith("/")) apiBaseUrl += "/";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ── HttpContextAccessor (required by ApiClientService) ─────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Cookie Authentication (MVC session) ───────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "BirthdayPortal.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// ── Session (stores the JWT to forward to the API) ─────────────────────────────
// NOTE: AddDistributedMemoryCache is IN-MEMORY ONLY.
// It is cleared on every app restart / IIS recycle, while the auth COOKIE
// can persist for up to 8 hours independently. That mismatch is exactly
// what caused the "looks logged in but no data" bug — RequireSessionTokenFilter
// (registered above) now detects and fixes this automatically by forcing
// re-login whenever the session token is missing.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── MVC-side API client service ───────────────────────────────────────────────
builder.Services.AddScoped<ApiClientService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANT: Session must be added BEFORE Authentication so the filter
// can read Session inside the auth pipeline correctly.
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();