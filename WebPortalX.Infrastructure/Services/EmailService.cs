using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using WebPortalX.Core.Interfaces;

namespace WebPortalX.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpClient = new SmtpClient
            {
                Host = _configuration["Email:SmtpHost"],
                Port = int.Parse(_configuration["Email:SmtpPort"]),
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                )
            };
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_configuration["Email:From"]),
                Subject = "Bienvenue sur WebPortalX",
                Body = $"Bonjour {userName},\n\nBienvenue sur WebPortalX !"
            };
            message.To.Add(email);

            await _smtpClient.SendMailAsync(message);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var resetUrl = $"{_configuration["Application:BaseUrl"]}/Account/ResetPassword?token={resetToken}&email={email}";
            
            var message = new MailMessage
            {
                From = new MailAddress(_configuration["Email:From"]),
                Subject = "Réinitialisation de mot de passe",
                Body = $"Pour réinitialiser votre mot de passe, cliquez sur ce lien : {resetUrl}"
            };
            message.To.Add(email);

            await _smtpClient.SendMailAsync(message);
        }

        public async Task SendEmailVerificationAsync(string email, string verificationToken)
        {
            var verificationUrl = $"{_configuration["Application:BaseUrl"]}/Account/VerifyEmail?token={verificationToken}";
            
            var message = new MailMessage
            {
                From = new MailAddress(_configuration["Email:From"]),
                Subject = "Vérification de votre email",
                Body = $"Pour vérifier votre email, cliquez sur ce lien : {verificationUrl}"
            };
            message.To.Add(email);

            await _smtpClient.SendMailAsync(message);
        }
    }
} 