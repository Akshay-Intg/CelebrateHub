using System;
using System.Collections.Generic;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class VoteResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PartyEventDto? UpdatedEvent { get; set; }
    }
}
