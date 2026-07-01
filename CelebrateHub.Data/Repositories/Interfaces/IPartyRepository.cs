// ============================================================
using CelebrateHub.Data.Models;

namespace CelebrateHub.Data.Repositories.Interfaces
{
    /// <summary>
    /// Data-access contract for PartyEvent entities.
    /// Services call this — never EF directly.
    /// </summary>
    public interface IPartyRepository
    {
        // ── Read ──────────────────────────────────────────────────────────────
        Task<IEnumerable<PartyEvent>> GetAllActiveAsync();
        Task<PartyEvent?> GetByIdAsync(int partyEventId);

        /// <summary>Events whose EventDate <= today AND status still Pending.</summary>
        Task<IEnumerable<PartyEvent>> GetEligibleForReminderAsync();

        /// <summary>
        /// Events where the given employee is the SUBJECT (EmployeeId == employeeId)
        /// AND the status is Pending — these trigger the reminder popup.
        /// </summary>
        Task<IEnumerable<PartyEvent>> GetPendingEventsForEmployeeAsync(int employeeId);

        /// <summary>All active events optionally filtered by EventType.</summary>
        Task<IEnumerable<PartyEvent>> GetFilteredAsync(string? eventType);

        /// <summary>
        /// Checks whether an active event already exists for this employee/type/year.
        /// </summary>
        Task<PartyEvent?> GetExistingEventAsync(int employeeId, string eventType, int year);

        // ── Write ─────────────────────────────────────────────────────────────
        Task<PartyEvent> CreateAsync(PartyEvent partyEvent);

        /// <summary>Persists recalculated vote counts and status.</summary>
        Task UpdateAsync(PartyEvent partyEvent);

        Task DeactivateAsync(int partyEventId);
    }
}