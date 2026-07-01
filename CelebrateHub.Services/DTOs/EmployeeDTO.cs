using System;
using System.Collections.Generic;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime? AnniversaryDate { get; set; }
        public string? Department { get; set; }
        public string Role { get; set; } = "User";
        public DateTime CreatedDate { get; set; }

        /// <summary>How old the employee turns this year.</summary>
        public int Age => CalculateAge(DateOfBirth);

        /// <summary>How many years since joining (if anniversary set).</summary>
        public int? YearsOfService => AnniversaryDate.HasValue
            ? CalculateAge(AnniversaryDate.Value)
            : null;

        private static int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            int age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
