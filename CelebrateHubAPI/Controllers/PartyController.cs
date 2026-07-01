// ============================================================
// BirthdayPortal.API/Controllers/PartyController.cs
// Place in: BirthdayPortal.API → Controllers folder
// ============================================================
// Exposes all party-voting endpoints:
//   POST   /api/party/vote
//   PUT    /api/party/change-vote
//   GET    /api/party/events
//   GET    /api/party/events/{id}
//   GET    /api/party/my-reminders
//   GET    /api/party/status/{employeeId}
//   POST   /api/party/sync           (triggers auto-event creation)
//   PUT    /api/party/override/{id}   (Admin only)
//   DELETE /api/party/{id}            (Admin only)
//   GET    /api/party/analytics       (Admin only)
// ============================================================

using CelebrateHub.Services;
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
    public class PartyController : ControllerBase
    {
        private readonly IPartyService _party;
        private readonly ILogger<PartyController> _logger;

        public PartyController(IPartyService party, ILogger<PartyController> logger)
        {
            _party = party;
            _logger = logger;
        }

        // ── Vote ──────────────────────────────────────────────────────────────

        /// <summary>POST /api/party/vote — Cast a new vote.</summary>
        [HttpPost("vote")]
        public async Task<IActionResult> Vote([FromBody] CastVoteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Validation failed."));

            var result = await _party.VoteAsync(GetCallerId(), dto);
            return result.Success
                ? Ok(ApiResponse<VoteResultDto>.Ok(result, result.Message))
                : BadRequest(ApiResponse<VoteResultDto>.Fail(result.Message));
        }

        /// <summary>PUT /api/party/change-vote — Update an existing vote.</summary>
        [HttpPut("change-vote")]
        public async Task<IActionResult> ChangeVote([FromBody] ChangeVoteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Validation failed."));

            var result = await _party.UpdateVoteAsync(GetCallerId(), dto);
            return result.Success
                ? Ok(ApiResponse<VoteResultDto>.Ok(result, result.Message))
                : BadRequest(ApiResponse<VoteResultDto>.Fail(result.Message));
        }

        // ── Events ────────────────────────────────────────────────────────────

        /// <summary>GET /api/party/events?eventType=Birthday — List all events.</summary>
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents([FromQuery] string? eventType)
        {
            var events = await _party.GetAllEventsAsync(GetCallerId(), eventType);
            return Ok(ApiResponse<IEnumerable<PartyEventDto>>.Ok(events));
        }

        /// <summary>GET /api/party/events/{id}</summary>
        [HttpGet("events/{id:int}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _party.GetEventByIdAsync(id, GetCallerId());
            return ev == null
                ? NotFound(ApiResponse<PartyEventDto>.Fail("Event not found."))
                : Ok(ApiResponse<PartyEventDto>.Ok(ev));
        }

        /// <summary>
        /// GET /api/party/my-reminders
        /// Returns events where the CALLER is the subject and party is still pending.
        /// The MVC dashboard calls this on every load to decide whether to show popup.
        /// </summary>
        [HttpGet("my-reminders")]
        public async Task<IActionResult> MyReminders()
        {
            var reminders = await _party.GetMyRemindersAsync(GetCallerId());
            return Ok(ApiResponse<IEnumerable<PartyReminderDto>>.Ok(reminders));
        }

        /// <summary>GET /api/party/status/{employeeId} — All events for a specific employee.</summary>
        [HttpGet("status/{employeeId:int}")]
        public async Task<IActionResult> GetStatus(int employeeId)
        {
            var events = await _party.GetStatusByEmployeeAsync(employeeId, GetCallerId());
            return Ok(ApiResponse<IEnumerable<PartyEventDto>>.Ok(events));
        }

        /// <summary>
        /// POST /api/party/sync — Idempotent trigger to auto-create events.
        /// Called by dashboard on load; safe to call multiple times.
        /// </summary>
        [HttpPost("sync")]
        public async Task<IActionResult> Sync()
        {
            await _party.SyncEventsForTodayAsync();
            return Ok(ApiResponse<object>.Ok(new { }, "Events synced successfully."));
        }

        // ── Admin endpoints ───────────────────────────────────────────────────

        /// <summary>PUT /api/party/override/{id} — Admin: force a status value.</summary>
        [HttpPut("override/{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Override(int id, [FromBody] OverrideStatusDto dto)
        {
            if (dto.NewStatus != "Pending" && dto.NewStatus != "Completed")
                return BadRequest(ApiResponse<object>.Fail(
                    "Status must be 'Pending' or 'Completed'."));

            var result = await _party.OverrideStatusAsync(id, dto.NewStatus);
            return result == null
                ? NotFound(ApiResponse<object>.Fail("Event not found."))
                : Ok(ApiResponse<PartyEventDto>.Ok(result, "Status overridden."));
        }

        /// <summary>DELETE /api/party/{id} — Admin: soft-delete an event.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            await _party.DeactivateEventAsync(id);
            _logger.LogInformation("Party event {Id} deactivated by {User}", id,
                User.FindFirst(ClaimTypes.Email)?.Value);
            return Ok(ApiResponse<object>.Ok(new { }, "Event deactivated."));
        }

        /// <summary>GET /api/party/analytics — Admin: aggregate stats.</summary>
        [HttpGet("analytics")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Analytics()
        {
            var stats = await _party.GetAnalyticsAsync();
            return Ok(ApiResponse<PartyAnalyticsDto>.Ok(stats));
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private int GetCallerId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}