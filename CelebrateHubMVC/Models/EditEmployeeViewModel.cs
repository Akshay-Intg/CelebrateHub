using System.ComponentModel.DataAnnotations;

namespace CelebrateHubMVC.Models
{
    public class EditEmployeeViewModel
    {
        public int EmployeeId { get; set; }

        [Required, MaxLength(40),MinLength(2), Display(Name = "Full Name")]
        [RegularExpression(@"^[A-Za-z\s]+$",
        ErrorMessage = "Name can contain only letters and spaces.")]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, Display(Name = "Email Address"),MaxLength(50)]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Date), Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [DataType(DataType.Date), Display(Name = "Anniversary")]
        public DateTime? AnniversaryDate { get; set; }

        [MaxLength(40)]
        [RegularExpression(@"^[A-Za-z\s&]+$")]
        public string? Department { get; set; }
    }
}