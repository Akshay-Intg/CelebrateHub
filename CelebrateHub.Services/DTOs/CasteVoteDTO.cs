using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class CastVoteDto
    {
        [Required]
        public int PartyEventId { get; set; }

        /// <summary>"Done" or "Pending"</summary>
        [Required]
        public string VoteType { get; set; } = string.Empty;
    }
}
