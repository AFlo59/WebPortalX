using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebPortalX.Infrastructure.Data;
using WebPortalX.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using IdentityModel;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using WebPortalX.Core.Models.Requests;
using WebPortalX.Core.Models.Responses;
using WebPortalX.Core.Interfaces;
using WebPortalX.Core.Common;
using Microsoft.Extensions.Logging;

namespace WebPortalX.API.Controllers
{
    [Route("api/users")]
    [ApiController]  // ✅ Active la validation automatique des modèles
    public class UserManagerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserManagerController> _logger;

        public UserManagerController(ApplicationDbContext context, IConfiguration config, IUserService userService, ITokenService tokenService, ILogger<UserManagerController> logger)
        {
            _context = context;
            _config = config;
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserManager>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id:long}")]
        [Authorize]
        public async Task<ActionResult<UserManager>> GetUser(long id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success || result.Data == null)
            {
                return NotFound(result.Message);
            }

            return Ok(new
            {
                UserName = result.Data.UserName,
                FirstName = result.Data.FirstName,
                LastName = result.Data.LastName,
                Email = result.Data.Email,
                DateOfBirth = result.Data.DateOfBirth,
                Role = result.Data.Role?.Name ?? "User",
                IsActive = result.Data.IsActive,
                CreatedAt = result.Data.CreatedAt,
                UpdatedAt = result.Data.UpdatedAt
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileResponse>> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _userService.GetUserByIdAsync(int.Parse(userId));
                if (!result.Success || result.Data == null)
                {
                    return NotFound();
                }

                return Ok(new UserProfileResponse
                {
                    UserName = result.Data.UserName,
                    FirstName = result.Data.FirstName,
                    LastName = result.Data.LastName,
                    Email = result.Data.Email,
                    DateOfBirth = result.Data.DateOfBirth,
                    Role = result.Data.Role?.Name ?? "User"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du profil");
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation($"Tentative d'inscription avec email: {request.Email}");
            
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = new UserManager
                {
                    UserName = request.UserName,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    DateOfBirth = request.DateOfBirth,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EmailVerified = false,
                    Role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "FreeUser")
                };

                user.SetPassword(request.Password);

                var result = await _userService.RegisterUserAsync(user);
                
                if (result.Success)
                {
                    return Ok(new { Message = "Inscription réussie !" });
                }
                
                return BadRequest(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription");
                return StatusCode(500, new { Message = "Une erreur est survenue lors de l'inscription." });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Tentative de connexion pour {request.Email}");
                
                var result = await _userService.AuthenticateAsync(request.Email, request.Password);
                
                if (!result.Success)
                {
                    _logger.LogWarning($"Échec de connexion pour {request.Email}: {result.Message}");
                    return Unauthorized(result.Message);
                }

                var token = _tokenService.GenerateToken(result.Data);
                
                _logger.LogInformation($"Connexion réussie pour {request.Email}");
                
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la connexion pour {request.Email}");
                return StatusCode(500, "Une erreur est survenue lors de la connexion");
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userService.UpdateUserAsync(long.Parse(userId), request);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { Message = "Profil mis à jour avec succès" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.InitiatePasswordResetAsync(request.Email);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { Message = "Email de réinitialisation envoyé" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ResetPasswordAsync(request);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new { Message = "Mot de passe réinitialisé avec succès" });
        }

        [HttpGet("validate-token")]
        [Authorize]
        public IActionResult ValidateToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Token invalide : UserId non trouvé");
                    return Unauthorized();
                }

                _logger.LogInformation($"Token validé pour l'utilisateur {userId}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du token");
                return StatusCode(500);
            }
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var userResult = await _userService.GetUserByIdAsync(long.Parse(userId));
                if (!userResult.Success || userResult.Data == null)
                {
                    return NotFound();
                }

                var newToken = _tokenService.GenerateToken(userResult.Data);
                return Ok(new { Token = newToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
                return StatusCode(500);
            }
        }

        [HttpGet("debug-users")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugUsers()
        {
            var users = await _context.Users.Select(u => new { u.Email, u.PasswordHash }).ToListAsync();
            return Ok(users);
        }
    }

    public class UserRegisterRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
    }

    public class UserLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}