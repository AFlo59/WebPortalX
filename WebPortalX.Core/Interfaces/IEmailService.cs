namespace WebPortalX.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email, string userName);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendEmailVerificationAsync(string email, string verificationToken);
    }
} 