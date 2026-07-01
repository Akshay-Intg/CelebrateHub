using System;
using System.Collections.Generic;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class PartyEventDto
    {
        public int PartyEventId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal DonePercentage { get; set; }
        public decimal PendingPercentage => 100 - DonePercentage;
        public int TotalVotes { get; set; }
        public int DoneVotes { get; set; }
        public int PendingVotes { get; set; }
        public bool IsPartyGiven => DonePercentage >= 75;

        /// <summary>The calling user's current vote for this event, null if not voted.</summary>
        public string? MyVote { get; set; }

        /// <summary>Progress bar CSS class based on percentage.</summary>
        public string ProgressBarClass => DonePercentage >= 75 ? "bg-success"
                                        : DonePercentage >= 50 ? "bg-warning"
                                        : "bg-danger";

        public string StatusBadgeClass => IsPartyGiven ? "badge-party-given" : "badge-party-pending";
        public string StatusLabel => IsPartyGiven ? "Party Given ✅" : "Party Pending ❌";
    }
}
