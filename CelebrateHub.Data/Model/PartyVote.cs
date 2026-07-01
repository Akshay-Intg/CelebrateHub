// ============================================================
// BirthdayPortal.Data/Models/PartyVote.cs
// Place in: BirthdayPortal.Data → Models folder
// ============================================================
// Stores one vote per employee per party event.
// A unique index on (PartyEventId, VoterEmployeeId) enforces
// the "one vote per person per event" rule at the DB level.
// ============================================================

using CelebrateHub.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CelebrateHub.Data.Models
{
    public class PartyVote
    {
        [Key]
        public int PartyVoteId { get; set; }

        [Required]
        public int PartyEventId { get; set; }

        [ForeignKey(nameof(PartyEventId))]
        public virtual PartyEvent PartyEvent { get; set; } = null!;

        /// <summary>The employee who cast this vote.</summary>
        [Required]
        public int VoterEmployeeId { get; set; }

        [ForeignKey(nameof(VoterEmployeeId))]
        public virtual Employee VoterEmployee { get; set; } = null!;

        /// <summary>"Done" or "Pending"</summary>
        [Required, MaxLength(10)]
        public string VoteType { get; set; } = string.Empty;

        public DateTime VotedOn { get; set; } = DateTime.UtcNow;

        /// <summary>Updated when the voter changes their mind.</summary>
        public DateTime? UpdatedOn { get; set; }
    }
}