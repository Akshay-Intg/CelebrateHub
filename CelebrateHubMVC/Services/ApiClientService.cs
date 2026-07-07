using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CelebrateHub.Services.DTOs;

namespace CelebrateHubMVC.Services
{
    public partial class ApiClientService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _http;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClientService(IHttpClientFactory factory, IHttpContextAccessor http)
        {
            _factory = factory;
            _http = http;
        }


        public Task<AuthResponseDto?> LoginAsync(LoginDto dto)
            => PostAsync<LoginDto, AuthResponseDto>("account/login", dto, addToken: false);

        public Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
            => PostAsync<RegisterDto, AuthResponseDto>("account/register", dto, addToken: false);

        public Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
            => PostVoidAsync("account/change-password", dto);


        public Task<DashboardDto?> GetDashboardAsync()
            => GetAsync<DashboardDto>("dashboard");

        public Task<IEnumerable<BirthdayDto>?> GetTodaysBirthdaysAsync()
            => GetAsync<IEnumerable<BirthdayDto>>("dashboard/todays-birthdays");

        public Task<IEnumerable<BirthdayDto>?> GetUpcomingBirthdaysAsync(int days = 7)
            => GetAsync<IEnumerable<BirthdayDto>>($"dashboard/upcoming-birthdays?days={days}");

        public Task<IEnumerable<AnniversaryDto>?> GetTodaysAnniversariesAsync()
            => GetAsync<IEnumerable<AnniversaryDto>>("dashboard/todays-anniversaries");

        public Task<IEnumerable<AnniversaryDto>?> GetUpcomingAnniversariesAsync(int days = 7)
            => GetAsync<IEnumerable<AnniversaryDto>>($"dashboard/upcoming-anniversaries?days={days}");

        // ── Employees ──────────────────────────────────────────────────────────

        public Task<IEnumerable<EmployeeDto>?> GetEmployeesAsync(string? search = null)
            => GetAsync<IEnumerable<EmployeeDto>>($"employees{(search != null ? $"?search={Uri.EscapeDataString(search)}" : "")}");

        public Task<EmployeeDto?> GetEmployeeAsync(int id)
            => GetAsync<EmployeeDto>($"employees/{id}");

        public Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
            => PutAsync<UpdateEmployeeDto, EmployeeDto>($"employees/{id}", dto);

        public Task<bool> DeleteEmployeeAsync(int id)
            => DeleteAsync($"employees/{id}");

        // ── HTTP primitives ────────────────────────────────────────────────────

        private async Task<T?> GetAsync<T>(string url)
        {
            var client = CreateClient();
            var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return default;
            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponse<T>>(json, _json);
            return wrapper != null ? wrapper.Data : default;
        }

        private async Task<TOut?> PostAsync<TIn, TOut>(string url, TIn body, bool addToken = true)
        {
            var client = CreateClient(addToken);
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(url, content);
            if (!resp.IsSuccessStatusCode) return default;
            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponse<TOut>>(json, _json);
            return wrapper != null ? wrapper.Data : default;
        }

        private async Task<bool> PostVoidAsync<TIn>(string url, TIn body)
        {
            var client = CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(url, content);
            return resp.IsSuccessStatusCode;
        }

        private async Task<TOut?> PutAsync<TIn, TOut>(string url, TIn body)
        {
            var client = CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var resp = await client.PutAsync(url, content);
            if (!resp.IsSuccessStatusCode) return default;
            var json = await resp.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponse<TOut>>(json, _json);
            return wrapper != null ? wrapper.Data : default;
        }

        private async Task<bool> DeleteAsync(string url)
        {
            var client = CreateClient();
            var resp = await client.DeleteAsync(url);
            return resp.IsSuccessStatusCode;
        }
        public async Task<bool> PostVoidAsync(string url)
        {
            var client = CreateClient();
            var resp = await client.PostAsync(url, null);
            return resp.IsSuccessStatusCode;
        }

        /// <summary>Creates an HttpClient and optionally attaches the JWT from session.</summary>
        private HttpClient CreateClient(bool addToken = true)
        {
            var client = _factory.CreateClient("API");
            if (addToken)
            {
                var token = _http.HttpContext?.Session.GetString("JWT");
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }
    }

    // ── Local response wrapper so we can deserialize ApiResponse<T> ────────────
    internal class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}