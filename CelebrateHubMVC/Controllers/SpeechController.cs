// ============================================================
// CelebrateHubMVC/Controllers/SpeechController.cs
// ============================================================
// Thin proxy — the browser calls this MVC endpoint,
// which attaches the JWT and forwards to the API.
// Returns the raw MP3 bytes directly to the browser.
// ============================================================

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CelebrateHubMVC.Controllers
{
    [Authorize]
    public class SpeechController : Controller
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _http;
        private readonly IConfiguration _config;

        public SpeechController(IHttpClientFactory factory,
            IHttpContextAccessor http,
            IConfiguration config)
        {
            _factory = factory;
            _http = http;
            _config = config;
        }

        /// <summary>
        /// GET /Speech/Generate?type=birthday&name=Akshay&age=32&dept=IT
        ///
        /// Forwards to API /api/speech/generate with JWT,
        /// streams the MP3 back to the browser.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Generate(
            string type, string name,
            int age = 0, int years = 0,
            string? dept = null)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var client = _factory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Build the query string for the API call
            var query = $"speech/generate?type={Uri.EscapeDataString(type)}" +
                        $"&name={Uri.EscapeDataString(name)}" +
                        $"&age={age}" +
                        $"&years={years}" +
                        $"&dept={Uri.EscapeDataString(dept ?? "")}";

            var resp = await client.GetAsync(query);
                       if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode,
                    "Speech generation failed.");

            var bytes = await resp.Content.ReadAsByteArrayAsync();

            // Return MP3 inline — browser's Audio element plays it directly
            return File(bytes, "audio/mpeg");
        }
    }
}