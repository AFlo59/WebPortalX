using Microsoft.EntityFrameworkCore;
using WebPortalX.Core.Interfaces;
using WebPortalX.Core.Models;
using WebPortalX.Core.Models.Requests;
using WebPortalX.Core.Common;
using WebPortalX.Infrastructure.Data;
using BC = BCrypt.Net.BCrypt;
using System.Security.Claims;
using WebPortalX.Core.Models.Responses;
using Microsoft.Extensions.Logging;

namespace WebPortalX.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, ITokenService tokenService, IEmailService emailService, ILogger<UserService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ServiceResult<UserManager>> GetUserByIdAsync(long id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null 
                ? ServiceResult<UserManager>.Error("Utilisateur non trouvé") 
                : ServiceResult<UserManager>.Ok(user);
        }

        public async Task<ServiceResult<UserManager>> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            return user == null 
                ? ServiceResult<UserManager>.Error("Utilisateur non trouvé") 
                : ServiceResult<UserManager>.Ok(user);
        }

        public async Task<ServiceResult<UserManager>> RegisterUserAsync(UserManager user)
        {
            try
            {
                // Vérifier si l'email existe déjà
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    return ServiceResult<UserManager>.Error("Cet email est déjà utilisé.");
                }

                // Ajouter l'utilisateur
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return ServiceResult<UserManager>.Ok(user, "Inscription réussie !");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserManager>.Error($"Erreur lors de l'inscription : {ex.Message}");
            }
        }

        public async Task<ServiceResult<UserManager>> AuthenticateAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation($"Tentative d'authentification pour {email}");

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    _logger.LogWarning($"Utilisateur non trouvé : {email}");
                    return ServiceResult<UserManager>.Error("Email ou mot de passe incorrect");
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning($"Mot de passe incorrect pour {email}");
                    return ServiceResult<UserManager>.Error("Email ou mot de passe incorrect");
                }

                _logger.LogInformation($"Authentification réussie pour {email}");
                return ServiceResult<UserManager>.Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'authentification de {email}");
                throw;
            }
        }

        public async Task<ServiceResult<UserManager>> UpdateUserAsync(long userId, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return ServiceResult<UserManager>.Error("User not found");
            }

            if (!string.IsNullOrEmpty(request.CurrentPassword))
            {
                if (!BC.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    return ServiceResult<UserManager>.Error("Current password is incorrect");
                }
                user.PasswordHash = BC.HashPassword(request.NewPassword);
            }

            user.UserName = request.UserName;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;

            await _context.SaveChangesAsync();
            return ServiceResult<UserManager>.Ok(user);
        }

        public async Task<ServiceResult> InitiatePasswordResetAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return ServiceResult.Error("User not found");
            }

            var token = _tokenService.GenerateToken(user);
            await _emailService.SendPasswordResetEmailAsync(email, token);

            return ServiceResult.Ok("Password reset email sent");
        }

        public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (!_tokenService.ValidateToken(request.Token))
            {
                return new ServiceResult(false, "Invalid or expired token");
            }

            var principal = _tokenService.GetPrincipalFromToken(request.Token);
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new ServiceResult(false, "User not found");
            }

            user.PasswordHash = BC.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return new ServiceResult(true, "Password reset successfully");
        }

        public async Task<ServiceResult> VerifyEmailAsync(string token)
        {
            if (!_tokenService.ValidateToken(token))
            {
                return new ServiceResult(false, "Invalid or expired token");
            }

            var principal = _tokenService.GetPrincipalFromToken(token);
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new ServiceResult(false, "User not found");
            }

            user.EmailVerified = true;
            await _context.SaveChangesAsync();

            return new ServiceResult(true, "Email verified successfully");
        }
    }
} 