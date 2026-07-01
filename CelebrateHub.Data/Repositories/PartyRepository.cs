// ============================================================
// BirthdayPortal.Data/Repositories/PartyRepository.cs
// Place in: BirthdayPortal.Data → Repositories folder
// ============================================================

using CelebrateHub.Data;
using CelebrateHub.Data.Models;
using CelebrateHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CelebrateHub.Data.Repositories
{
    public class PartyRepository : IPartyRepository
    {
        private readonly AppDbContext _db;
        public PartyRepository(AppDbContext db) => _db = db;

        // ── Base query: always include the Employee nav-prop ──────────────────
        private IQueryable<PartyEvent> BaseQuery()
            => _db.PartyEvents
                  .Include(pe => pe.Employee)
                  .Include(pe => pe.Votes);

        public async Task<IEnumerable<PartyEvent>> GetAllActiveAsync()
            => await BaseQuery()
                     .Where(pe => pe.IsActive)
                     .OrderByDescending(pe => pe.EventDate)
                     .ToListAsync();

        public async Task<PartyEvent?> GetByIdAsync(int partyEventId)
            => await BaseQuery()
                     .FirstOrDefaultAsync(pe => pe.PartyEventId == partyEventId && pe.IsActive);

        public async Task<IEnumerable<PartyEvent>> GetEligibleForReminderAsync()
        {
            var today = DateTime.Today;
            return await BaseQuery()
                         .Where(pe => pe.IsActive
                                      && pe.Status == "Pending"
                                      && pe.EventDate <= today)
                         .OrderByDescending(pe => pe.EventDate)
                         .ToListAsync();
        }

        public async Task<IEnumerable<PartyEvent>> GetPendingEventsForEmployeeAsync(int employeeId)
        {
            var today = DateTime.Today;
            return await BaseQuery()
                         .Where(pe => pe.IsActive
                                      && pe.EmployeeId == employeeId
                                      && pe.Status == "Pending"
                                      && pe.EventDate <= today)
                         .OrderByDescending(pe => pe.EventDate)
                         .ToListAsync();
        }

        public async Task<IEnumerable<PartyEvent>> GetFilteredAsync(string? eventType)
        {
            var q = BaseQuery().Where(pe => pe.IsActive);
            if (!string.IsNullOrWhiteSpace(eventType))
                q = q.Where(pe => pe.EventType == eventType);
            return await q.OrderByDescending(pe => pe.EventDate).ToListAsync();
        }

        public async Task<PartyEvent?> GetExistingEventAsync(
     int employeeId, string eventType, int year)
        {
            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);

            return await _db.PartyEvents
                            .FirstOrDefaultAsync(pe =>
                                pe.EmployeeId == employeeId &&
                                pe.EventType == eventType &&
                                pe.EventDate >= yearStart &&
                                pe.EventDate <= yearEnd);
        }

        public async Task<PartyEvent> CreateAsync(PartyEvent partyEvent)
        {
            _db.PartyEvents.Add(partyEvent);
            await _db.SaveChangesAsync();
            return partyEvent;
        }

        public async Task UpdateAsync(PartyEvent partyEvent)
        {
            _db.PartyEvents.Update(partyEvent);
            await _db.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int partyEventId)
        {
            var ev = await _db.PartyEvents.FindAsync(partyEventId);
            if (ev != null)
            {
                ev.IsActive = false;
                await _db.SaveChangesAsync();
            }
        }
    }
}