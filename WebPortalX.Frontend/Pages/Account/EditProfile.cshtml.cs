using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class EditProfileModel : PageModel
{
    [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
    [Display(Name = "Nom d'utilisateur")]
    [BindProperty]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Le prénom est requis")]
    [Display(Name = "Prénom")]
    [BindProperty]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Le nom est requis")]
    [Display(Name = "Nom")]
    [BindProperty]
    public string LastName { get; set; }

    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    [Display(Name = "Email")]
    [BindProperty]
    public string Email { get; set; }

    [Required(ErrorMessage = "La date de naissance est requise")]
    [DataType(DataType.Date)]
    [Display(Name = "Date de naissance")]
    [BindProperty]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Mot de passe actuel")]
    [BindProperty]
    public string CurrentPassword { get; set; }

    [StringLength(100, ErrorMessage = "Le mot de passe doit faire au moins {2} caractères", MinimumLength = 6)]
    [Display(Name = "Nouveau mot de passe")]
    [BindProperty]
    public string NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
    [Display(Name = "Confirmer le nouveau mot de passe")]
    [BindProperty]
    public string ConfirmNewPassword { get; set; }

    [TempData]
    public string Message { get; set; }

    public bool IsSuccess { get; set; }

    private readonly IConfiguration _configuration;
    private readonly string _apiBaseUrl;

    public EditProfileModel(IConfiguration configuration)
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = Request.Cookies["AuthToken"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new
            {
                UserName,
                FirstName,
                LastName,
                Email,
                DateOfBirth,
                CurrentPassword,
                NewPassword
            };

            var response = await client.PutAsync($"{_apiBaseUrl}/api/users/update",
                new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                IsSuccess = true;
                Message = "Profil mis à jour avec succès";
                return RedirectToPage("/Account/Profile");
            }

            var error = await response.Content.ReadAsStringAsync();
            Message = $"Erreur lors de la mise à jour : {error}";
            return Page();
        }
        catch (Exception ex)
        {
            Message = $"Une erreur est survenue : {ex.Message}";
            return Page();
        }
    }
} 