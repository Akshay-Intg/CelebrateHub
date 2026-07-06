namespace CelebrateHub.Services.Interfaces
{
    public interface IEmailService
    {
        /// <summary>Send a birthday wish email to one employee.</summary>
        Task SendBirthdayWishAsync(string toEmail, string employeeName,
            int turningAge, string? department);

        /// <summary>Send an anniversary wish email to one employee.</summary>
        Task SendAnniversaryWishAsync(string toEmail, string employeeName,
            int yearsCompleted, string? department);

        /// <summary>Generic send — used internally and for testing.</summary>
        Task SendEmailAsync(string toEmail, string toName,
            string subject, string htmlBody);
    }
}