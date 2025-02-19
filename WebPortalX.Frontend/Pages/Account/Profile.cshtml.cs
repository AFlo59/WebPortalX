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
            var response = await client.GetAsync("/api/users/profile");

            if (response.IsSuccessStatusCode)
            {
                var userProfile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
                if (userProfile != null)
                {
                    UserName = userProfile.UserName;
                    FirstName = userProfile.FirstName;
                    LastName = userProfile.LastName;
                    Email = userProfile.Email;
                    DateOfBirth = userProfile.DateOfBirth;
                    Role = userProfile.Role;
                }
                return Page();
            }

            _logger.LogWarning("Erreur lors de la récupération du profil");
            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du profil");
            return RedirectToPage("/Account/Login");
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