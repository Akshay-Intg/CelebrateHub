// ============================================================
// BirthdayPortal.MVC/Services/ApiClientService.Party.cs
// Place in: BirthdayPortal.MVC → Services folder
// ============================================================
// Partial extension of ApiClientService adding all party-related
// HTTP calls. In a real project you can merge this into the
// existing ApiClientService.cs if you prefer one file.
// ============================================================

using CelebrateHub.Services.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CelebrateHubMVC.Services
{
    /// <summary>
    /// Party-specific HTTP client methods.
    /// These live in a separate file for clarity but belong to the
    /// same ApiClientService class (mark the existing class partial).
    /// </summary>
    public partial class ApiClientService
    {
        // ── Party Events ──────────────────────────────────────────────────────

        public Task<IEnumerable<PartyEventDto>?> GetPartyEventsAsync(string? eventType = null)
            => GetAsync<IEnumerable<PartyEventDto>>(
                $"party/events{(eventType != null ? $"?eventType={eventType}" : "")}");

        public Task<PartyEventDto?> GetPartyEventAsync(int id)
            => GetAsync<PartyEventDto>($"party/events/{id}");

        // ── Voting ────────────────────────────────────────────────────────────

        public Task<VoteResultDto?> CastVoteAsync(CastVoteDto dto)
            => PostAsync<CastVoteDto, VoteResultDto>("party/vote", dto);

        public Task<VoteResultDto?> ChangeVoteAsync(ChangeVoteDto dto)
            => PutAsync<ChangeVoteDto, VoteResultDto>("party/change-vote", dto);

        // ── Reminders ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns pending party events where the logged-in user is the subject.
        /// Empty list = no popup needed.
        /// </summary>
        public Task<IEnumerable<PartyReminderDto>?> GetMyRemindersAsync()
            => GetAsync<IEnumerable<PartyReminderDto>>("party/my-reminders");

        // ── Status ────────────────────────────────────────────────────────────

        public Task<IEnumerable<PartyEventDto>?> GetPartyStatusByEmployeeAsync(int employeeId)
            => GetAsync<IEnumerable<PartyEventDto>>($"party/status/{employeeId}");

        // ── Sync (triggers auto-event creation) ───────────────────────────────

        public async Task SyncPartyEventsAsync()
        {
            var client = CreateClient();
            await client.PostAsync("party/sync", null);
        }

        // ── Admin ─────────────────────────────────────────────────────────────

        public Task<PartyAnalyticsDto?> GetPartyAnalyticsAsync()
            => GetAsync<PartyAnalyticsDto>("party/analytics");

        public async Task<bool> OverridePartyStatusAsync(int id, string newStatus)
        {
            var client = CreateClient();
            // Send as a proper JSON object, not a bare string
            var payload = new { NewStatus = newStatus };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");
            var resp = await client.PutAsync($"party/override/{id}", content);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePartyEventAsync(int id)
        {
            var client = CreateClient();
            var resp = await client.DeleteAsync($"party/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}