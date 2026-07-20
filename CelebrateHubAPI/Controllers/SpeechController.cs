using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CelebrateHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SpeechController : ControllerBase
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<SpeechController> _logger;
        private readonly IConfiguration _configuration;

        

        public SpeechController(
    IHttpClientFactory http,
    ILogger<SpeechController> logger,
    IConfiguration configuration)
        {
            _http = http;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// GET /api/speech/generate?type=birthday&name=Akshay&age=32&dept=Engineering
        /// GET /api/speech/generate?type=anniversary&name=Priya&years=3&dept=HR
        ///
        /// Calls FreeTTS, gets file_id, downloads MP3, streams it back.
        /// </summary>
        [HttpGet("generate")]
        public async Task<IActionResult> Generate(
            [FromQuery] string type,
            [FromQuery] string name,
            [FromQuery] int age = 0,
            [FromQuery] int years = 0,
            [FromQuery] string? dept = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            if (type != "birthday" && type != "anniversary")
                return BadRequest("type must be 'birthday' or 'anniversary'.");

            // ── Build the message text ────────────────────────────────────────
            var text = type == "birthday"
                ? BuildBirthdayMessage(name, age, dept)
                : BuildAnniversaryMessage(name, years, dept);

            try
            {
                var apiKey = _configuration["ElevenLabs:ApiKey"];
                var voiceId = _configuration["ElevenLabs:VoiceId"];

                var client = _http.CreateClient("ElevenLabs");

                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Add("xi-api-key", apiKey);

                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("audio/mpeg"));

                var body = new
                {
                    text = text,
                    model_id = "eleven_flash_v2_5",
                    output_format = "mp3_44100_128",
                    voice_settings = new
                    {
                        stability = 0.45,
                        similarity_boost = 0.80,
                        style = 0.30,
                        use_speaker_boost = true
                    }
                };

                var response = await client.PostAsJsonAsync(
                    $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}",
                    body);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    _logger.LogError(
                        "ElevenLabs Error {Status}: {Error}",
                        response.StatusCode,
                        error);

                    return StatusCode((int)response.StatusCode, error);
                }

                var audio = await response.Content.ReadAsByteArrayAsync();

                return File(audio, "audio/mpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Speech generation failed.");

                return StatusCode(500);
            }
        }

        // ── Message builders ──────────────────────────────────────────────────

        private static string BuildBirthdayMessage(
            string name, int age, string? dept)
        {
            var firstName = name.Split(' ')[0];
            var agePart = age > 0
                ? $"You are turning {age} today. "
                : string.Empty;
            var deptPart = !string.IsNullOrEmpty(dept)
                ? $"Everyone in the {dept} team"
                : "Your entire team";

            // Keep under 1000 chars (free tier limit)
            return
                $"Happy Birthday, {firstName}! " +
                $"{agePart}" +
                $"{deptPart} wishes you a wonderful, " +
                $"joyful, and truly memorable day. " +
                $"May this year bring you great success, happiness, " +
                $"and unforgettable moments. " +
                $"You make our workplace a better place every single day. " +
                $"Enjoy your special day, and don't forget to " +
                $"treat the team to something sweet!";
        }

        private static string BuildAnniversaryMessage(
            string name, int years, string? dept)
        {
            var firstName = name.Split(' ')[0];
            var yearsPart = years > 0
                ? $"{years} incredible year{(years != 1 ? "s" : "")}"
                : "another wonderful year";
            var deptPart = !string.IsNullOrEmpty(dept)
                ? $"Everyone in the {dept} team"
                : "Your entire team";

            return
                $"Happy Marriage Anniversary, {firstName}! " +
                $"Today marks {yearsPart} of your beautiful journey together, " +
                $"and what a wonderful journey it has been. " +
                $"May your love, trust, and companionship continue to grow stronger with each passing year. " +
                $"Wishing you both a lifetime of happiness, good health, and cherished memories. " +
                $"Here's to many more years of love and togetherness. " +
                $"Congratulations, and have a wonderful celebration together!";
        }
    }
}