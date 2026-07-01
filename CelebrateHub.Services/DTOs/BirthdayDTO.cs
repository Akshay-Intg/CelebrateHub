using System;
using System.Collections.Generic;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class BirthdayDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Department { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int DaysUntil { get; set; }
        public int TurningAge { get; set; }
    }
}
