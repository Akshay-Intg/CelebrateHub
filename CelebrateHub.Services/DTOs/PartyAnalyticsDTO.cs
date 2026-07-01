// ============================================================
// BirthdayPortal.Services/DTOs/PartyDTOs.cs
// Place in: BirthdayPortal.Services → DTOs folder
// ============================================================
// All request and response shapes for the Party Voting feature.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace CelebrateHub.Services.DTOs
{
    public class PartyAnalyticsDto
    {
        public int TotalEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int PendingEvents { get; set; }
        public int TotalVotesCast { get; set; }
        public decimal AverageDonePercentage { get; set; }
    }
}