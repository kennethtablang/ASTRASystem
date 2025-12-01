using ASTRASystem.Interfaces;
using ASTRASystem.Models;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;

namespace ASTRASystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _configuration["Smtp:FromName"] ?? "ASTRA System",
                    _configuration["Smtp:FromAddress"] ?? "noreply@astra.local"));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    bodyBuilder.HtmlBody = body;
                }
                else
                {
                    bodyBuilder.TextBody = body;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // Connect to SMTP server
                await client.ConnectAsync(
                    _configuration["Smtp:Host"] ?? "localhost",
                    int.Parse(_configuration["Smtp:Port"] ?? "587"),
                    SecureSocketOptions.StartTls);

                // Authenticate if credentials are provided
                var username = _configuration["Smtp:UserName"];
                var password = _configuration["Smtp:Password"];

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    await client.AuthenticateAsync(username, password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                // Don't throw - email failures shouldn't break the application flow
            }
        }

        public async Task SendConfirmationEmailAsync(ApplicationUser user, string confirmationLink)
        {
            var subject = "Confirm Your Email - ASTRA System";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to ASTRA System, {user.FirstName}!</h2>
                    <p>Please confirm your email address by clicking the link below:</p>
                    <p><a href='{confirmationLink}'>Confirm Email Address</a></p>
                    <p>If you didn't create this account, please ignore this email.</p>
                    <br/>
                    <p>Thank you,<br/>ASTRA System Team</p>
                </body>
                </html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(ApplicationUser user, string resetLink)
        {
            var subject = "Reset Your Password - ASTRA System";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hello {user.FirstName},</p>
                    <p>We received a request to reset your password. Click the link below to proceed:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you didn't request a password reset, please ignore this email.</p>
                    <br/>
                    <p>Thank you,<br/>ASTRA System Team</p>
                </body>
                </html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendTwoFactorCodeAsync(ApplicationUser user, string code)
        {
            var subject = "Your Two-Factor Code - ASTRA System";
            var body = $@"
                <html>
                <body>
                    <h2>Two-Factor Authentication</h2>
                    <p>Hello {user.FirstName},</p>
                    <p>Your two-factor authentication code is:</p>
                    <h1 style='color: #007bff; letter-spacing: 5px;'>{code}</h1>
                    <p>This code will expire in 10 minutes.</p>
                    <p>If you didn't request this code, please contact support immediately.</p>
                    <br/>
                    <p>Thank you,<br/>ASTRA System Team</p>
                </body>
                </html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(ApplicationUser user)
        {
            var subject = "Welcome to ASTRA System!";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome {user.FirstName}!</h2>
                    <p>Your account has been successfully created.</p>
                    <p>You can now log in and start using the ASTRA System.</p>
                    <br/>
                    <p>Thank you,<br/>ASTRA System Team</p>
                </body>
                </html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendAccountApprovalEmailAsync(ApplicationUser user, bool approved, string? message)
        {
            var subject = approved ? "Account Approved - ASTRA System" : "Account Status - ASTRA System";
            var body = $@"
                <html>
                <body>
                    <h2>Account Status Update</h2>
                    <p>Hello {user.FirstName},</p>
                    <p>Your account has been {(approved ? "approved" : "reviewed")}.</p>
                    {(!string.IsNullOrEmpty(message) ? $"<p><strong>Message:</strong> {message}</p>" : "")}
                    {(approved ? "<p>You can now log in and access the system.</p>" : "<p>Please contact support for more information.</p>")}
                    <br/>
                    <p>Thank you,<br/>ASTRA System Team</p>
                </body>
                </html>";

            await SendEmailAsync(user.Email, subject, body);
        }
    }
}
