using CelebrateHub.Services.DTOs;
using CelebrateHubMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CelebrateHubMVC.Controllers
{
    [Authorize]
    public class PartyController : Controller
    {
        private readonly ApiClientService _api;

        public PartyController(ApiClientService api) => _api = api;

        // ── Main voting board ─────────────────────────────────────────────────

        /// <summary>
        /// GET /Party — The party voting board with all active events.
        /// Also triggers the event sync on every load.
        /// </summary>
        public async Task<IActionResult> Index(string? filter)
        {
            // Sync events every time (idempotent — no harm if already done)
            await _api.SyncPartyEventsAsync();

            var events = await _api.GetPartyEventsAsync(filter)
                         ?? Enumerable.Empty<PartyEventDto>();

            ViewBag.Filter = filter;
            return View(events);
        }

        // ── AJAX: Cast vote ───────────────────────────────────────────────────

        /// <summary>POST /Party/Vote — Called via jQuery AJAX from the voting board.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int partyEventId, string voteType)
        {
            var result = await _api.CastVoteAsync(new CastVoteDto
            {
                PartyEventId = partyEventId,
                VoteType = voteType
            });

            if (result == null) return Json(new { success = false, message = "API call failed." });

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                updatedEvent = result.UpdatedEvent
            });
        }

        // ── AJAX: Change vote ─────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeVote(int partyEventId, string newVoteType)
        {
            var result = await _api.ChangeVoteAsync(new ChangeVoteDto
            {
                PartyEventId = partyEventId,
                NewVoteType = newVoteType
            });

            if (result == null) return Json(new { success = false, message = "API call failed." });

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                updatedEvent = result.UpdatedEvent
            });
        }

        // ── AJAX: Get reminder data for popup ─────────────────────────────────

        /// <summary>
        /// GET /Party/MyReminders — Called on every dashboard load via AJAX.
        /// Returns JSON reminder list; empty = no popup.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyReminders()
        {
            var reminders = await _api.GetMyRemindersAsync()
                            ?? Enumerable.Empty<PartyReminderDto>();
            return Json(reminders);
        }

        // ── Admin: Analytics ──────────────────────────────────────────────────

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Analytics()
        {
            var stats = await _api.GetPartyAnalyticsAsync() ?? new PartyAnalyticsDto();
            var events = await _api.GetPartyEventsAsync() ?? Enumerable.Empty<PartyEventDto>();
            ViewBag.Events = events;
            return View(stats);
        }

        // ── Admin: Override status ────────────────────────────────────────────

        [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Override(int id, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                TempData["Error"] = "Invalid status value.";
                return RedirectToAction("Analytics");
            }

            var ok = await _api.OverridePartyStatusAsync(id, newStatus);
            TempData[ok ? "Success" : "Error"] = ok
                ? $"Status successfully changed to '{newStatus}'."
                : "Override failed. Please try again.";
            return RedirectToAction("Analytics");
        }

        // ── Admin: Delete event ───────────────────────────────────────────────

        [HttpPost, Authorize(Policy = "AdminOnly"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeletePartyEventAsync(id);
            TempData["Success"] = "Event deactivated.";
            return RedirectToAction("Analytics"); 
        }
    }
}