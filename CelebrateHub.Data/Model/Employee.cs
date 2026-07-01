using System.ComponentModel.DataAnnotations;

namespace CelebrateHub.Data.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        public DateTime? AnniversaryDate { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }
        [MaxLength(100)]
        public string? Project { get; set; }

        [MaxLength(20)]
        public string Role { get; set; } = "User";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}