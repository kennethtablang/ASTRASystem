using ASTRASystem.Models;

namespace ASTRASystem.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task SendConfirmationEmailAsync(ApplicationUser user, string confirmationLink);
        Task SendPasswordResetEmailAsync(ApplicationUser user, string resetLink);
        Task SendTwoFactorCodeAsync(ApplicationUser user, string code);
        Task SendWelcomeEmailAsync(ApplicationUser user);
        Task SendAccountApprovalEmailAsync(ApplicationUser user, bool approved, string? message);
    }
}
