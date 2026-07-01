using System;
using System.Collections.Generic;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class PartyReminderDto
    {
        public int PartyEventId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public decimal DonePercentage { get; set; }
        public string Status { get; set; } = string.Empty;

        /// <summary>Pre-formatted message for the popup.</summary>
        public string ReminderMessage =>
            $"Hi {EmployeeName}, your {EventType} celebration on {EventDate:MMM dd} is still " +
            $"marked as Pending. Current team approval: {DonePercentage:F0}% Done. " +
            $"Please arrange a team party — this reminder continues until 75% of the team confirms.";
    }
}
