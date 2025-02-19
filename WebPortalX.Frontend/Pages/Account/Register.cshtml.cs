using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class RegisterModel : PageModel
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

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [StringLength(100, ErrorMessage = "Le mot de passe doit faire au moins {2} caractères", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    [BindProperty]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le mot de passe")]
    [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
    [BindProperty]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "La date de naissance est requise")]
    [DataType(DataType.Date)]
    [Display(Name = "Date de naissance")]
    [BindProperty]
    public DateTime DateOfBirth { get; set; }

    [TempData]
    public string Message { get; set; }

    private readonly IConfiguration _configuration;
    private readonly string _apiBaseUrl;

    public RegisterModel(IConfiguration configuration)
    {
        _configuration = configuration;
        _apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5165";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            using var client = new HttpClient();
            var response = await client.PostAsync($"{_apiBaseUrl}/api/users/register",
                new StringContent(JsonSerializer.Serialize(new
                {
                    UserName,
                    FirstName,
                    LastName,
                    Email,
                    Password,
                    DateOfBirth
                }), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Account/Login");
            }

            var error = await response.Content.ReadAsStringAsync();
            Message = $"Erreur lors de l'inscription : {error}";
            return Page();
        }
        catch (Exception ex)
        {
            Message = $"Une erreur est survenue : {ex.Message}";
            return Page();
        }
    }
}