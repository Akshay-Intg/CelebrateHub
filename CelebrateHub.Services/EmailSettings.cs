namespace CelebrateHub.Services.Models
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// Master switch — set to false in development to prevent
        /// accidental emails to real employees during testing.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}