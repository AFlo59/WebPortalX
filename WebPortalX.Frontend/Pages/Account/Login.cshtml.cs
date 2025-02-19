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
    private readonly IAuthService _authService;

    public LoginModel(IConfiguration configuration, IApiService apiService, ILogger<LoginModel> logger, IAuthService authService)
    {
        _configuration = configuration;
        _apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5165";
        _apiService = apiService;
        _logger = logger;
        _authService = authService;
    }

    [BindProperty]
    public LoginRequest LoginRequest { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            _logger.LogInformation($"Tentative de connexion pour {Email}");
            
            var response = await _apiService.PostAsync("/api/users/login", new LoginRequest
            {
                Email = Email,
                Password = Password
            });

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Réponse de l'API : {content}");

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse?.Token == null)
                {
                    ModelState.AddModelError(string.Empty, "Erreur lors de la connexion");
                    return Page();
                }

                _authService.StoreToken(loginResponse.Token, RememberMe);
                
                TempData["SuccessMessage"] = "Connexion réussie !";
                return RedirectToPage("/Index");
            }

            _logger.LogWarning($"Échec de connexion : {response.StatusCode} - {content}");
            ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion");
            ModelState.AddModelError(string.Empty, "Une erreur est survenue");
            return Page();
        }
    }

    public IActionResult OnPostLogout()
    {
        _authService.RemoveToken();
        TempData["SuccessMessage"] = "Vous avez été déconnecté avec succès";
        return RedirectToPage("/Index");
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}