using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class ProfileModel : PageModel
{
    private readonly ILogger<ProfileModel> _logger;
    private readonly IHttpClientFactory _clientFactory;

    [BindProperty]
    public string UserName { get; set; } = string.Empty;
    [BindProperty]
    public string FirstName { get; set; } = string.Empty;
    [BindProperty]
    public string LastName { get; set; } = string.Empty;
    [BindProperty]
    public string Email { get; set; } = string.Empty;
    [BindProperty]
    public DateTime DateOfBirth { get; set; }
    [BindProperty]
    public string Role { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public ProfileModel(ILogger<ProfileModel> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var client = _clientFactory.CreateClient("API");
            var token = Request.Cookies["AuthToken"];
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token manquant lors de l'accès au profil");
                return RedirectToPage("/Account/Login");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            // Log pour déboguer
            _logger.LogInformation($"Tentative d'accès au profil avec le token : {token.Substring(0, 10)}...");
            
            // Utiliser l'endpoint correct de l'API
            var response = await client.GetAsync("/api/users/profile");
            
            // Log de la réponse
            _logger.LogInformation($"Statut de la réponse API : {response.StatusCode}");
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Contenu de la réponse : {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var userProfile = JsonSerializer.Deserialize<UserProfileResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (userProfile != null)
                    {
                        UserName = userProfile.UserName;
                        FirstName = userProfile.FirstName;
                        LastName = userProfile.LastName;
                        Email = userProfile.Email;
                        DateOfBirth = userProfile.DateOfBirth;
                        Role = userProfile.Role;
                        return Page();
                    }
                    else
                    {
                        _logger.LogError("La désérialisation du profil a retourné null");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Erreur lors de la désérialisation du profil");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Token invalide ou expiré");
                Response.Cookies.Delete("AuthToken");
                return RedirectToPage("/Account/Login");
            }

            _logger.LogError($"Erreur lors de la récupération du profil : {response.StatusCode}");
            // Ne pas rediriger vers l'index en cas d'erreur
            Message = "Une erreur est survenue lors de la récupération de votre profil";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception lors de la récupération du profil");
            Message = "Une erreur inattendue est survenue";
            return Page();
        }
    }

    public IActionResult OnPostLogout()
    {
        Response.Cookies.Delete("AuthToken");
        return RedirectToPage("/Account/Login");
    }
}

public class UserProfileResponse
{
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}