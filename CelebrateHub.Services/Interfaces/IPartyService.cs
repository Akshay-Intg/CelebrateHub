// ============================================================
// BirthdayPortal.Services/Interfaces/IPartyService.cs
// Place in: BirthdayPortal.Services → Interfaces folder
// ============================================================

using CelebrateHub.Services.DTOs;

namespace CelebrateHub.Services.Interfaces
{
    /// <summary>
    /// Business logic contract for the Party Status Voting System.
    /// </summary>
    public interface IPartyService
    {
        // ── Event lifecycle ───────────────────────────────────────────────────

        /// <summary>
        /// Creates a party event if one does not already exist for this
        /// employee + type + current year. Called by a background job or
        /// on dashboard load.
        /// </summary>
        Task<PartyEventDto?> CreatePartyEventAsync(CreatePartyEventDto dto);

        /// <summary>
        /// Scans all employees and auto-creates events for any birthday or
        /// anniversary that is today or earlier in the current year.
        /// Safe to call on every dashboard load — idempotent.
        /// </summary>
        Task SyncEventsForTodayAsync();

        // ── Voting ────────────────────────────────────────────────────────────

        /// <summary>Cast a new vote. Fails if the user has already voted.</summary>
        Task<VoteResultDto> VoteAsync(int voterEmployeeId, CastVoteDto dto);

        /// <summary>
        /// Update an existing vote. Recalculates percentages and status.
        /// </summary>
        Task<VoteResultDto> UpdateVoteAsync(int voterEmployeeId, ChangeVoteDto dto);

        // ── Queries ───────────────────────────────────────────────────────────

        Task<IEnumerable<PartyEventDto>> GetAllEventsAsync(int callerEmployeeId, string? eventType = null);
        Task<PartyEventDto?> GetEventByIdAsync(int partyEventId, int callerEmployeeId);

        /// <summary>
        /// Returns events where the CALLER is the subject AND the party is
        /// still pending — used to trigger the reminder popup.
        /// </summary>
        Task<IEnumerable<PartyReminderDto>> GetMyRemindersAsync(int employeeId);

        /// <summary>All pending events for an employee (admin view).</summary>
        Task<IEnumerable<PartyEventDto>> GetStatusByEmployeeAsync(int employeeId, int callerEmployeeId);

        // ── Admin ─────────────────────────────────────────────────────────────

        Task<PartyEventDto?> OverrideStatusAsync(int partyEventId, string newStatus);
        Task DeactivateEventAsync(int partyEventId);
        Task<PartyAnalyticsDto> GetAnalyticsAsync();

        // ── Internal helpers (also public for reuse) ──────────────────────────
        decimal CalculatePercentage(int doneVotes, int totalVotes);
    }
}