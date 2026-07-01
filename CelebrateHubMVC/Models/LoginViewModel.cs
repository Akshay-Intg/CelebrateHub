using System.ComponentModel.DataAnnotations;

namespace CelebrateHubMVC.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress, Display(Name = "Email Address"),MaxLength(30)]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password),MaxLength(16)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
