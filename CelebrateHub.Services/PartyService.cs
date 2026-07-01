// ============================================================
// BirthdayPortal.Services/PartyService.cs
// Place in: BirthdayPortal.Services project (root)
// ============================================================
// Implements all party-voting business logic:
//   - Auto-event creation (idempotent)
//   - Vote casting & updating with recalculation
//   - 75% threshold → "Completed" status
//   - Reminder queries for the popup system
// ============================================================

using CelebrateHub.Data.Models;
using CelebrateHub.Data.Repositories.Interfaces;
using CelebrateHub.Services.DTOs;
using CelebrateHub.Services.Interfaces;

namespace CelebrateHub.Services
{
    public class PartyService : IPartyService
    {
        private const decimal CompletionThreshold = 75m;

        private readonly IPartyRepository _partyRepo;
        private readonly IPartyVoteRepository _voteRepo;
        private readonly IEmployeeRepository _empRepo;

        public PartyService(
            IPartyRepository partyRepo,
            IPartyVoteRepository voteRepo,
            IEmployeeRepository empRepo)
        {
            _partyRepo = partyRepo;
            _voteRepo = voteRepo;
            _empRepo = empRepo;
        }

        // ── Event creation ────────────────────────────────────────────────────

        public async Task<PartyEventDto?> CreatePartyEventAsync(CreatePartyEventDto dto)
        {
            // Idempotent: skip if event already exists for this year
            var existing = await _partyRepo.GetExistingEventAsync(
                dto.EmployeeId, dto.EventType, dto.EventDate.Year);

            if (existing != null) return await MapToDtoAsync(existing, callerId: 0);

            var ev = new PartyEvent
            {
                EmployeeId = dto.EmployeeId,
                EventType = dto.EventType,
                EventDate = dto.EventDate,
                Status = "Pending",
                DonePercentage = 0,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _partyRepo.CreateAsync(ev);
            return await MapToDtoAsync(created, callerId: 0);
        }

        /// <summary>
        /// Called on every dashboard load. Scans all employees and creates
        /// events for birthdays/anniversaries that are today or earlier this year.
        /// Completely safe to call multiple times — existing events are skipped.
        /// </summary>
        public async Task SyncEventsForTodayAsync()
        {
            var today = DateTime.Today;
            var employees = await _empRepo.GetAllAsync();

            foreach (var emp in employees)
            {
                // ── Birthday ──────────────────────────────────────────────────
                var bdayThisYear = new DateTime(today.Year, emp.DateOfBirth.Month, emp.DateOfBirth.Day);
                if (bdayThisYear <= today)
                {
                    var existing = await _partyRepo.GetExistingEventAsync(emp.EmployeeId, "Birthday", today.Year);
                    if (existing == null)
                    {
                        try
                        {
                            await _partyRepo.CreateAsync(new PartyEvent
                            {
                                EmployeeId = emp.EmployeeId,
                                EventType = "Birthday",
                                EventDate = bdayThisYear,
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                IsActive = true
                            });
                        }
                        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
                        {
                            
                        }
                    }
                }

                // ── Anniversary ───────────────────────────────────────────────
                if (emp.AnniversaryDate.HasValue)
                {
                    var annThisYear = new DateTime(today.Year,
                        emp.AnniversaryDate.Value.Month,
                        emp.AnniversaryDate.Value.Day);

                    if (annThisYear <= today)
                    {
                        var existing = await _partyRepo.GetExistingEventAsync(emp.EmployeeId, "Anniversary", today.Year);
                        if (existing == null)
                            await _partyRepo.CreateAsync(new PartyEvent
                            {
                                EmployeeId = emp.EmployeeId,
                                EventType = "Anniversary",
                                EventDate = annThisYear,
                                Status = "Pending",
                                CreatedDate = DateTime.UtcNow,
                                IsActive = true
                            });
                    }
                }
            }
        }

        // ── Voting ────────────────────────────────────────────────────────────

        public async Task<VoteResultDto> VoteAsync(int voterEmployeeId, CastVoteDto dto)
        {
            // Validate vote type
            if (dto.VoteType != "Done" && dto.VoteType != "Pending")
                return Fail("VoteType must be 'Done' or 'Pending'.");

            var partyEvent = await _partyRepo.GetByIdAsync(dto.PartyEventId);
            if (partyEvent == null)
                return Fail("Party event not found.");

            // Employees cannot vote on their own event
            if (partyEvent.EmployeeId == voterEmployeeId)
                return Fail("You cannot vote on your own birthday or anniversary event.");

            // One vote per person
            var existingVote = await _voteRepo.GetVoteAsync(dto.PartyEventId, voterEmployeeId);
            if (existingVote != null)
                return Fail("You have already voted on this event. Use change-vote to update.");

            // Cast vote
            await _voteRepo.CastVoteAsync(new PartyVote
            {
                PartyEventId = dto.PartyEventId,
                VoterEmployeeId = voterEmployeeId,
                VoteType = dto.VoteType,
                VotedOn = DateTime.UtcNow
            });

            // Recalculate and persist
            await RecalculateAndSaveAsync(partyEvent);

            var updated = await _partyRepo.GetByIdAsync(dto.PartyEventId);
            return new VoteResultDto
            {
                Success = true,
                Message = "Vote cast successfully.",
                UpdatedEvent = await MapToDtoAsync(updated!, voterEmployeeId)
            };
        }

        public async Task<VoteResultDto> UpdateVoteAsync(int voterEmployeeId, ChangeVoteDto dto)
        {
            if (dto.NewVoteType != "Done" && dto.NewVoteType != "Pending")
                return Fail("NewVoteType must be 'Done' or 'Pending'.");

            var existingVote = await _voteRepo.GetVoteAsync(dto.PartyEventId, voterEmployeeId);
            if (existingVote == null)
                return Fail("No existing vote found. Please cast a new vote first.");

            if (existingVote.VoteType == dto.NewVoteType)
                return Fail($"Your vote is already '{dto.NewVoteType}'. No change needed.");

            existingVote.VoteType = dto.NewVoteType;
            existingVote.UpdatedOn = DateTime.UtcNow;
            await _voteRepo.UpdateVoteAsync(existingVote);

            var partyEvent = await _partyRepo.GetByIdAsync(dto.PartyEventId);
            if (partyEvent == null) return Fail("Party event not found.");

            await RecalculateAndSaveAsync(partyEvent);

            var updated = await _partyRepo.GetByIdAsync(dto.PartyEventId);
            return new VoteResultDto
            {
                Success = true,
                Message = "Vote updated successfully.",
                UpdatedEvent = await MapToDtoAsync(updated!, voterEmployeeId)
            };
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public async Task<IEnumerable<PartyEventDto>> GetAllEventsAsync(int callerEmployeeId, string? eventType = null)
        {
            var events = await _partyRepo.GetFilteredAsync(eventType);
            var dtos = new List<PartyEventDto>();
            foreach (var ev in events)
                dtos.Add(await MapToDtoAsync(ev, callerEmployeeId));
            return dtos;
        }

        public async Task<PartyEventDto?> GetEventByIdAsync(int partyEventId, int callerEmployeeId)
        {
            var ev = await _partyRepo.GetByIdAsync(partyEventId);
            return ev == null ? null : await MapToDtoAsync(ev, callerEmployeeId);
        }

        public async Task<IEnumerable<PartyReminderDto>> GetMyRemindersAsync(int employeeId)
        {
            var events = await _partyRepo.GetPendingEventsForEmployeeAsync(employeeId);
            return events.Select(ev => new PartyReminderDto
            {
                PartyEventId = ev.PartyEventId,
                EmployeeName = ev.Employee?.Name ?? string.Empty,
                EventType = ev.EventType,
                EventDate = ev.EventDate,
                DonePercentage = ev.DonePercentage,
                Status = ev.Status
            });
        }

        public async Task<IEnumerable<PartyEventDto>> GetStatusByEmployeeAsync(int employeeId, int callerEmployeeId)
        {
            var all = await _partyRepo.GetAllActiveAsync();
            var events = all.Where(e => e.EmployeeId == employeeId);
            var dtos = new List<PartyEventDto>();
            foreach (var ev in events)
                dtos.Add(await MapToDtoAsync(ev, callerEmployeeId));
            return dtos;
        }

        // ── Admin ─────────────────────────────────────────────────────────────

        public async Task<PartyEventDto?> OverrideStatusAsync(int partyEventId, string newStatus)
        {
            var ev = await _partyRepo.GetByIdAsync(partyEventId);
            if (ev == null) return null;

            ev.Status = newStatus;
            await _partyRepo.UpdateAsync(ev);
            return await MapToDtoAsync(ev, callerId: 0);
        }

        public async Task DeactivateEventAsync(int partyEventId)
            => await _partyRepo.DeactivateAsync(partyEventId);

        public async Task<PartyAnalyticsDto> GetAnalyticsAsync()
        {
            var events = await _partyRepo.GetAllActiveAsync();
            var list = events.ToList();

            return new PartyAnalyticsDto
            {
                TotalEvents = list.Count,
                CompletedEvents = list.Count(e => e.Status == "Completed"),
                PendingEvents = list.Count(e => e.Status == "Pending"),
                TotalVotesCast = list.Sum(e => e.TotalVotes),
                AverageDonePercentage = list.Any()
                    ? list.Average(e => e.DonePercentage)
                    : 0
            };
        }

        // ── Core calculation ──────────────────────────────────────────────────

        public decimal CalculatePercentage(int doneVotes, int totalVotes)
            => totalVotes == 0 ? 0 : Math.Round((decimal)doneVotes / totalVotes * 100, 2);

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Reloads vote totals from DB, recalculates percentage,
        /// flips status to Completed if threshold reached, then saves.
        /// </summary>
        private async Task RecalculateAndSaveAsync(PartyEvent partyEvent)
        {
            var votes = await _voteRepo.GetVotesByEventAsync(partyEvent.PartyEventId);
            var voteList = votes.ToList();

            partyEvent.TotalVotes = voteList.Count;
            partyEvent.DoneVotes = voteList.Count(v => v.VoteType == "Done");
            partyEvent.PendingVotes = voteList.Count(v => v.VoteType == "Pending");
            partyEvent.DonePercentage = CalculatePercentage(partyEvent.DoneVotes, partyEvent.TotalVotes);

            // Auto-transition to Completed when threshold is hit
            partyEvent.Status = partyEvent.DonePercentage >= CompletionThreshold
                ? "Completed"
                : "Pending";

            await _partyRepo.UpdateAsync(partyEvent);
        }

        private async Task<PartyEventDto> MapToDtoAsync(PartyEvent ev, int callerId)
        {
            string? myVote = null;
            if (callerId > 0)
            {
                var vote = await _voteRepo.GetVoteAsync(ev.PartyEventId, callerId);
                myVote = vote?.VoteType;
            }

            return new PartyEventDto
            {
                PartyEventId = ev.PartyEventId,
                EmployeeId = ev.EmployeeId,
                EmployeeName = ev.Employee?.Name ?? string.Empty,
                Department = ev.Employee?.Department,
                EventType = ev.EventType,
                EventDate = ev.EventDate,
                Status = ev.Status,
                DonePercentage = ev.DonePercentage,
                TotalVotes = ev.TotalVotes,
                DoneVotes = ev.DoneVotes,
                PendingVotes = ev.PendingVotes,
                MyVote = myVote
            };
        }

        private static VoteResultDto Fail(string msg)
            => new() { Success = false, Message = msg };
    }
}