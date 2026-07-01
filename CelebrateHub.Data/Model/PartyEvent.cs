// ============================================================
// BirthdayPortal.Data/Models/PartyEvent.cs
// Place in: BirthdayPortal.Data → Models folder
// ============================================================
// Represents a single party event (Birthday or Anniversary)
// that team members can vote on.
// ============================================================

using CelebrateHub.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelebrateHub.Data.Models
{
    public class PartyEvent
    {
        [Key]
        public int PartyEventId { get; set; }

        // ── Foreign key to the employee whose birthday/anniversary this is ──
        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee Employee { get; set; } = null!;

        /// <summary>"Birthday" or "Anniversary"</summary>
        [Required, MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>The actual date of this year's birthday or anniversary.</summary>
        [Required]
        public DateTime EventDate { get; set; }

        /// <summary>"Pending" until DonePercentage >= 75, then "Completed".</summary>
        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        /// <summary>Recalculated on every vote. 0-100.</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal DonePercentage { get; set; } = 0;

        public int TotalVotes { get; set; } = 0;
        public int DoneVotes { get; set; } = 0;
        public int PendingVotes { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>False = soft-deleted/archived by admin.</summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<PartyVote> Votes { get; set; } = new List<PartyVote>();
    }
}