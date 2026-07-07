// ============================================================
// CelebrateHub.Services/EmailService.cs
// Uses MailKit for SMTP — Microsoft's recommended SmtpClient
// replacement. Builds beautiful HTML emails from templates.
// ============================================================

using CelebrateHub.Services.Interfaces;
using CelebrateHub.Services.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace CelebrateHub.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings,
            ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public async Task SendBirthdayWishAsync(string toEmail,
            string employeeName, int turningAge, string? department)
        {
            var subject = $"🎂 Happy Birthday, {employeeName.Split(' ')[0]}! " +
                          $"Wishing you an amazing day! 🎉";

            var html = BuildBirthdayTemplate(employeeName, turningAge, department);

            await SendEmailAsync(toEmail, employeeName, subject, html);
        }

        public async Task SendAnniversaryWishAsync(string toEmail,
            string employeeName, int yearsCompleted, string? department)
        {
            var subject = $"💐 Happy Marriage Anniversary, {employeeName.Split(' ')[0]}!" +
                          $" {yearsCompleted} amazing year{(yearsCompleted != 1 ? "s" : "")}! 🌟";

            var html = BuildAnniversaryTemplate(employeeName, yearsCompleted, department);

            await SendEmailAsync(toEmail, employeeName, subject, html);
        }

        public async Task SendEmailAsync(string toEmail, string toName,
            string subject, string htmlBody)
        {
            // Master switch — skip silently in dev/test when disabled
            if (!_settings.IsEnabled)
            {
                _logger.LogInformation(
                    "Email disabled. Would have sent '{Subject}' to {Email}",
                    subject, toEmail);
                return;
            }

            try
            {
                var message = new MimeMessage();

                // From
                message.From.Add(new MailboxAddress(
                    _settings.SenderName, _settings.SenderEmail));

                // To
                message.To.Add(new MailboxAddress(toName, toEmail));

                // Subject
                message.Subject = subject;

                // Body — multipart so clients that can't render HTML
                // still get a plain-text fallback
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody,
                    TextBody = StripHtml(htmlBody)
                };
                message.Body = bodyBuilder.ToMessageBody();

                // Send via MailKit SMTP client
                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    _settings.SmtpHost,
                    _settings.SmtpPort,
                    SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(
                    _settings.SenderEmail,
                    _settings.SenderPassword);

                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(quit: true);

                _logger.LogInformation(
                    "Email sent successfully to {Email} | Subject: {Subject}",
                    toEmail, subject);
            }
            catch (Exception ex)
            {
                // Log but don't crash the app — email failure should
                // never take down the whole web server
                _logger.LogError(ex,
                    "Failed to send email to {Email} | Subject: {Subject}",
                    toEmail, subject);
            }
        }

        // ── HTML Email Templates ──────────────────────────────────────────────

        private string BuildBirthdayTemplate(string name,
            int turningAge, string? department)
        {
            var firstName = name.Split(' ')[0];
            var dept = string.IsNullOrEmpty(department)
                           ? "the team"
                           : $"the {department} team";
            var ordinal = GetOrdinal(turningAge);

            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8""/>
<meta name=""viewport"" content=""width=device-width,initial-scale=1.0""/>
<title>Happy Birthday!</title>
</head>
<body style=""margin:0;padding:0;background:#f5f6fb;
             font-family:'Segoe UI',Arial,sans-serif;"">

  <div style=""max-width:600px;margin:40px auto;border-radius:20px;
               overflow:hidden;box-shadow:0 8px 40px rgba(0,0,0,0.12);"">

    <!-- Header -->
    <div style=""background:linear-gradient(135deg,#4338ca 0%,#7c3aed 100%);
                 padding:48px 40px;text-align:center;"">
      <div style=""font-size:64px;margin-bottom:12px;"">🎂</div>
      <h1 style=""color:white;margin:0;font-size:32px;
                  font-weight:800;letter-spacing:-0.5px;"">
        Happy Birthday!
      </h1>
      <p style=""color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:16px;"">
        Today is your special day, {firstName}! 🎉
      </p>
    </div>

    <!-- Confetti strip -->
    <div style=""background:linear-gradient(90deg,#fb7185,#f59e0b,
                 #34d399,#6366f1,#a78bfa);height:6px;""></div>

    <!-- Body -->
    <div style=""background:white;padding:48px 40px;"">

      <p style=""font-size:18px;color:#374151;margin:0 0 16px;
                 font-weight:600;"">
        Dear {name},
      </p>

      <p style=""font-size:15px;color:#6b7280;line-height:1.8;margin:0 0 24px;"">
        On behalf of everyone at <strong>CelebrateHub</strong>, we want to wish
        you a very <strong style=""color:#4f46e5;"">Happy {turningAge}{ordinal} Birthday!</strong>
        🎊 Today is all about you — we hope it's filled with joy, laughter,
        and everything that makes you smile.
      </p>

      <!-- Birthday card box -->
      <div style=""background:linear-gradient(135deg,#eef2ff,#f5f3ff);
                   border-radius:16px;padding:28px;margin:24px 0;
                   border-left:5px solid #6366f1;text-align:center;"">
        <div style=""font-size:36px;margin-bottom:8px;"">🥳</div>
        <p style=""font-size:20px;font-weight:800;color:#4338ca;margin:0 0 4px;"">
          Turning {turningAge} never looked so good!
        </p>
        <p style=""font-size:14px;color:#6b7280;margin:0;"">
          {dept} is so lucky to have you.
        </p>
      </div>

      <p style=""font-size:15px;color:#6b7280;line-height:1.8;margin:0 0 24px;"">
        Your energy, dedication, and personality make our workplace a better
        place every single day. Thank you for being an incredible part of
        our team. Here's to another year of great achievements and
        unforgettable memories! 🌟
      </p>

      <!-- Party nudge section -->
      <div style=""background:#fffbeb;border:1px solid #fde68a;
                   border-radius:12px;padding:20px 24px;margin:24px 0;"">
        <p style=""margin:0 0 8px;font-size:15px;font-weight:700;color:#92400e;"">
          🎉 Party Time?
        </p>
        <p style=""margin:0;font-size:14px;color:#92400e;line-height:1.7;"">
          We'd love to celebrate with you! If you haven't already,
          consider treating {dept} to something sweet today —
          even a small gesture goes a long way in making memories.
          The team is rooting for you! 😊
        </p>
      </div>

      <p style=""font-size:15px;color:#6b7280;line-height:1.8;margin:0 0 8px;"">
        With warm wishes and lots of cake,
      </p>
      <p style=""font-size:16px;font-weight:700;color:#4f46e5;margin:0;"">
        The CelebrateHub Team 🎂
      </p>

    </div>

    <!-- Footer -->
    <div style=""background:#f8f9fc;padding:24px 40px;text-align:center;
                 border-top:1px solid #e5e7eb;"">
      <p style=""margin:0;font-size:12px;color:#9ca3af;"">
        This message was sent by CelebrateHub — your team's celebration portal.<br/>
        Making every birthday and anniversary count. 🎊
      </p>
    </div>

  </div>
</body>
</html>";
        }

        private string BuildAnniversaryTemplate(string name,
            int years, string? department)
        {
            var firstName = name.Split(' ')[0];
            var dept = string.IsNullOrEmpty(department)
                          ? "the team"
                          : $"the {department} team";
            var ordinal = GetOrdinal(years);
            var milestone = GetMilestone(years);

            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8""/>
<meta name=""viewport"" content=""width=device-width,initial-scale=1.0""/>
<title>Happy Marriage Anniversary!</title>
</head>
<body style=""margin:0;padding:0;background:#f5f6fb;
             font-family:'Segoe UI',Arial,sans-serif;"">

  <div style=""max-width:600px;margin:40px auto;border-radius:20px;
               overflow:hidden;box-shadow:0 8px 40px rgba(0,0,0,0.12);"">

    <!-- Header -->
    <div style=""background:linear-gradient(135deg,#059669 0%,#0891b2 100%);
                 padding:48px 40px;text-align:center;"">
      <div style=""font-size:64px;margin-bottom:12px;"">💍</div>
      <h1 style=""color:white;margin:0;font-size:32px;
                  font-weight:800;letter-spacing:-0.5px;"">
        Marriage Anniversary!
      </h1>
      <p style=""color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:16px;"">
        Celebrating {years} wonderful year{(years != 1 ? "s" : "")} together, {firstName}! ❤️
      </p>
    </div>

    <!-- Colour strip -->
    <div style=""background:linear-gradient(90deg,#34d399,#0891b2,
                 #6366f1,#a78bfa,#fb7185);height:6px;""></div>

    <!-- Body -->
    <div style=""background:white;padding:48px 40px;"">

      <p style=""font-size:18px;color:#374151;margin:0 0 16px;font-weight:600;"">
        Dear {name},
      </p>

      <p style=""font-size:15px;color:#6b7280;line-height:1.8;margin:0 0 24px;"">
        Wishing you a very <strong style=""color:#059669;"">
        Happy {years}{ordinal} Marriage Anniversary</strong> 💐
        May this special day bring back cherished memories and fill your hearts
        with happiness, love, and gratitude. On behalf of everyone at<strong>CelebrateHub</strong>, we hope your bond continues to grow stronger
        with every passing year.
      </p>

      <!-- Anniversary milestone card -->
      <div style=""background:linear-gradient(135deg,#ecfdf5,#ecfeff);
                   border-radius:16px;padding:28px;margin:24px 0;
                   border-left:5px solid #059669;text-align:center;"">
        <div style=""font-size:36px;margin-bottom:8px;"">💖</div>
        <p style=""font-size:22px;font-weight:800;color:#065f46;margin:0 0 4px;"">
          Cheers to {years} Beautiful Year{(years != 1 ? "s" : "")} Together!
        </p>
        {milestone}
      </div>

      <p style=""font-size:15px;color:#6b7280;line-height:1.8;margin:0 0 24px;"">
        Your journey together is a wonderful reminder that love, trust, and
        companionship grow stronger with time. May the years ahead be filled
        with good health, endless happiness, and countless beautiful moments
        together. 💕
      </p>

      <!-- Party nudge section -->
      <div style=""background:#fffbeb;border:1px solid #fde68a;
                   border-radius:12px;padding:20px 24px;margin:24px 0;"">
        <p style=""margin:0 0 8px;font-size:15px;font-weight:700;color:#92400e;"">
          🎉 Celebrate Your Special Day!
        </p>
        <p style=""margin:0;font-size:14px;color:#92400e;line-height:1.7;"">
          Marriage anniversaries are precious milestones worth celebrating.
          Take some time to enjoy this special occasion with your loved ones,
          create new memories, and cherish the beautiful journey you've shared
          together. Wishing you many more happy years ahead! 🥂
        </p>
      </div>

      <p style=""font-size:15px;color:#6b7280;line-height:1.8;margin:0 0 8px;"">
        With heartfelt congratulations and best wishes,
      </p>
      <p style=""font-size:16px;font-weight:700;color:#059669;margin:0;"">
        The CelebrateHub Team 💐
      </p>

    </div>

    <!-- Footer -->
    <div style=""background:#f8f9fc;padding:24px 40px;text-align:center;
                 border-top:1px solid #e5e7eb;"">
      <p style=""margin:0;font-size:12px;color:#9ca3af;"">
        This message was sent by CelebrateHub — your team's celebration portal.<br/>
        Celebrating life's beautiful milestones together. 🎊
      </p>
    </div>

  </div>
</body>
</html>";
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static string GetOrdinal(int n) => (n % 100) switch
        {
            11 or 12 or 13 => "th",
            _ => (n % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            }
        };

        private static string GetMilestone(int years) => years switch
        {
            1 => "<p style=\"font-size:14px;color:#065f46;margin:0;\">Your first year — what a start! 🌱</p>",
            3 => "<p style=\"font-size:14px;color:#065f46;margin:0;\">3 years — you're truly part of the family! 👨‍👩‍👧‍👦</p>",
            5 => "<p style=\"font-size:14px;color:#065f46;margin:0;\">Half a decade — you're a cornerstone of this team! 💎</p>",
            10 => "<p style=\"font-size:14px;color:#065f46;margin:0;\">A full decade — a true legend in our midst! 🏅</p>",
            _ => $"<p style=\"font-size:14px;color:#065f46;margin:0;\">{years} years of dedication — simply remarkable! ⭐</p>"
        };

        /// <summary>
        /// Very basic HTML stripper for the plain-text fallback.
        /// Not perfect but good enough for email clients.
        /// </summary>
        private static string StripHtml(string html)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(html, "<[^>]*>", " ")
                .Replace("  ", " ")
                .Trim();
        }
    }
}