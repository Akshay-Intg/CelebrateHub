using System;
using System.Collections.Generic;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class AnniversaryDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Department { get; set; }
        public DateTime AnniversaryDate { get; set; }
        public int DaysUntil { get; set; }
        public int YearsCompleting { get; set; }
    }
}
