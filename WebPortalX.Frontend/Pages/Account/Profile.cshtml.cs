using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class ProfileModel : PageModel
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

    [TempData]
    public string Message { get; set; }

    private readonly IConfiguration _configuration;
    private readonly string _apiBaseUrl;

    public ProfileModel(IConfiguration configuration)
    {
        _configuration = configuration;
        _apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5165";
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{_apiBaseUrl}/api/users/me");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var userProfile = JsonSerializer.Deserialize<UserProfileResponse>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (userProfile != null)
                {
                    UserName = userProfile.UserName;
                    FirstName = userProfile.FirstName;
                    LastName = userProfile.LastName;
                    Email = userProfile.Email;
                    DateOfBirth = userProfile.DateOfBirth;
                    Role = userProfile.Role;
                    IsActive = userProfile.IsActive;
                    CreatedAt = userProfile.CreatedAt;
                    UpdatedAt = userProfile.UpdatedAt;
                    return Page();
                }
            }

            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            Message = $"Une erreur est survenue : {ex.Message}";
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