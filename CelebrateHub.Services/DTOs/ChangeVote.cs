using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class ChangeVoteDto
    {
        [Required]
        public int PartyEventId { get; set; }

        [Required]
        public string NewVoteType { get; set; } = string.Empty;
    }
}
