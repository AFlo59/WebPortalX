using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebPortalX.Frontend.Services;
using WebPortalX.Core.Models.Requests;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class LoginModel : PageModel
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    [Display(Name = "Email")]
    [BindProperty]
    public string Email { get; set; }

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    [BindProperty]
    public string Password { get; set; }

    [Display(Name = "Se souvenir de moi")]
    [BindProperty]
    public bool RememberMe { get; set; }

    [TempData]
    public string Message { get; set; }

    private readonly IConfiguration _configuration;
    private readonly string _apiBaseUrl;
    private readonly IApiService _apiService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(IConfiguration configuration, IApiService apiService, ILogger<LoginModel> logger)
    {
        _configuration = configuration;
        _apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5165";
        _apiService = apiService;
        _logger = logger;
    }

    [BindProperty]
    public LoginRequest LoginRequest { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            LoginRequest = new LoginRequest
            {
                Email = Email,
                Password = Password
            };

            _logger.LogInformation($"Tentative de connexion pour {LoginRequest.Email}");
            
            var response = await _apiService.PostAsync("/api/users/login", LoginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Réponse reçue : {content}");

                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse?.Token == null)
                {
                    _logger.LogError("Token manquant dans la réponse");
                    ModelState.AddModelError(string.Empty, "Erreur lors de la connexion : token manquant");
                    return Page();
                }

                // Stocker le token dans un cookie sécurisé
                Response.Cookies.Append("AuthToken", loginResponse.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(1)
                });

                _logger.LogInformation("Token stocké dans le cookie, redirection vers l'accueil");
                TempData["SuccessMessage"] = "Connexion réussie !";
                return RedirectToPage("/Index");
            }
            
            _logger.LogWarning($"Échec de connexion : {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning($"Contenu de l'erreur : {errorContent}");
            
            ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion");
            ModelState.AddModelError(string.Empty, $"Une erreur est survenue : {ex.Message}");
            return Page();
        }
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}