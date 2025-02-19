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
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(new
            {
                result.Data.UserName,
                result.Data.FirstName,
                result.Data.LastName,
                result.Data.Email,
                result.Data.DateOfBirth,
                result.Data.Role,
                result.Data.IsActive,
                result.Data.CreatedAt,
                result.Data.UpdatedAt
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userService.GetUserByIdAsync(long.Parse(userId));
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(new UserProfileResponse
            {
                UserName = result.Data.UserName,
                FirstName = result.Data.FirstName,
                LastName = result.Data.LastName,
                Email = result.Data.Email,
                DateOfBirth = result.Data.DateOfBirth,
                Role = result.Data.Role.ToString(),
                IsActive = result.Data.IsActive,
                CreatedAt = result.Data.CreatedAt,
                UpdatedAt = result.Data.UpdatedAt
            });
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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation($"Tentative de connexion pour l'email: {request.Email}");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modèle invalide");
                return BadRequest(ModelState);
            }

            var result = await _userService.AuthenticateAsync(request.Email, request.Password);
            if (!result.Success)
            {
                _logger.LogWarning($"Échec de l'authentification: {result.Message}");
                return Unauthorized(result.Message);
            }

            var token = _tokenService.GenerateToken(result.Data);
            _logger.LogInformation("Connexion réussie");
            
            return Ok(new LoginResponse 
            { 
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
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