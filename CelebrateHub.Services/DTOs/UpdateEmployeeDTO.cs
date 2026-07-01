using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CelebrateHub.Services.DTOs
{
    public class UpdateEmployeeDto
    {
        [Required]
        [StringLength(40, MinimumLength = 2)]
        [RegularExpression(@"^[A-Za-z\s]+$",
        ErrorMessage = "Name can contain only letters and spaces.")]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        public DateTime? AnniversaryDate { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }
    }
}
