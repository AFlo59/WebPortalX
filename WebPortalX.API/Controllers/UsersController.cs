using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebPortalX.Core.Interfaces;           // Pour IUserService
using WebPortalX.Core.Models.Requests;      // Pour LoginRequest
using WebPortalX.Core.Models.Responses;     // Pour les réponses
using Microsoft.AspNetCore.Authorization;  // Ajout de ce using pour AllowAnonymous

namespace WebPortalX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;

        public UsersController(ILogger<UsersController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Tentative de connexion pour {request.Email}");
                var result = await _userService.AuthenticateAsync(request.Email, request.Password);
                
                if (result.Success)
                {
                    _logger.LogInformation($"Connexion réussie pour {request.Email}");
                    return Ok(new { token = result.Token });
                }

                _logger.LogWarning($"Échec de connexion pour {request.Email}");
                return Unauthorized(new { message = "Email ou mot de passe incorrect" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la connexion : {ex.Message}");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la connexion" });
            }
        }
    }
} 