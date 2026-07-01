using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class CreatePartyEventDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty;   // "Birthday" | "Anniversary"

        [Required]
        public DateTime EventDate { get; set; }
    }
}
