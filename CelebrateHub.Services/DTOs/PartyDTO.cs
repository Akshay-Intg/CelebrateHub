using System.ComponentModel.DataAnnotations;

namespace CelebrateHub.Services
{
    public class OverrideStatusDto
    {
        [Required]
        public string NewStatus { get; set; } = string.Empty;
    }
}
