using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ForgotPasswordModel : PageModel
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    [Display(Name = "Email")]
    [BindProperty]
    public string Email { get; set; }

    [TempData]
    public string Message { get; set; }

    public bool IsSuccess { get; set; }

    private readonly IConfiguration _configuration;
    private readonly string _apiBaseUrl;

    public ForgotPasswordModel(IConfiguration configuration)
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
            var response = await client.PostAsync($"{_apiBaseUrl}/api/users/forgot-password",
                new StringContent(JsonSerializer.Serialize(new { Email }), 
                Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                IsSuccess = true;
                Message = "Un email de réinitialisation a été envoyé à votre adresse email.";
                return Page();
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