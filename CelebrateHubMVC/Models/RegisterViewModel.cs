using System.ComponentModel.DataAnnotations;

namespace CelebrateHubMVC.Models
{
    public class RegisterViewModel
    {
        [Required, MaxLength(40), Display(Name = "Full Name")]
        [RegularExpression(@"^[A-Za-z\s]+$",
        ErrorMessage = "Name can contain only letters and spaces.")]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(50), Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), DataType(DataType.Password),MaxLength(16)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,16}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password"), DataType(DataType.Password),MaxLength(16)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Date), Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [DataType(DataType.Date), Display(Name = "Anniversary Date")]
        public DateTime? AnniversaryDate { get; set; }

        [MaxLength(30)]
        [RegularExpression(@"^[A-Za-z\s&]+$")]
        public string? Department { get; set; }
    }
}
