using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ResetPasswordModel : PageModel
{
    [BindProperty]
    public string Token { get; set; }

    [BindProperty]
    public string Email { get; set; }

    [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
    [StringLength(100, ErrorMessage = "Le mot de passe doit faire au moins {2} caractères", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Nouveau mot de passe")]
    [BindProperty]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le mot de passe")]
    [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
    [BindProperty]
    public string ConfirmPassword { get; set; }

    [TempData]
    public string Message { get; set; }

    public bool IsSuccess { get; set; }

    private readonly IConfiguration _configuration;
    private readonly string _apiBaseUrl;

    public ResetPasswordModel(IConfiguration configuration)
    {
        _configuration = configuration;
        _apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5165";
    }

    public IActionResult OnGet(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            return RedirectToPage("/Account/Login");
        }

        Token = token;
        Email = email;
        return Page();
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
            var response = await client.PostAsync($"{_apiBaseUrl}/api/users/reset-password",
                new StringContent(JsonSerializer.Serialize(new 
                { 
                    Token, 
                    Email, 
                    NewPassword 
                }), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                IsSuccess = true;
                Message = "Votre mot de passe a été réinitialisé avec succès.";
                return RedirectToPage("/Account/Login");
            }

            var error = await response.Content.ReadAsStringAsync();
            Message = $"Erreur : {error}";
            return Page();
        }
        catch (Exception ex)
        {
            Message = $"Une erreur est survenue : {ex.Message}";
            return Page();
        }
    }
} 