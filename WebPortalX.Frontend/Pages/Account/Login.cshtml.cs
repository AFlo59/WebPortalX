using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebPortalX.Frontend.Services;
using WebPortalX.Core.Models.Requests;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebPortalX.Frontend.Interfaces;

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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _logger.LogInformation($"Tentative de connexion pour {Email}");

            var loginRequest = new LoginRequest { Email = Email, Password = Password };
            var response = await _apiService.PostAsync<LoginResponse>("/api/users/authenticate", loginRequest);

            if (response.IsSuccess && response.Data?.Token != null)
            {
                _logger.LogInformation("Token reçu, stockage...");
                await _authService.StoreTokenAsync(response.Data.Token);
                _logger.LogInformation("Redirection vers l'accueil...");
                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(string.Empty, response.Message ?? "Email ou mot de passe incorrect");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion");
            ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la connexion");
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