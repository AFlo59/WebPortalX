using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using WebPortalX.Frontend.Services;
using System.Net.Http.Json;
using WebPortalX.Frontend.Interfaces;

namespace WebPortalX.Frontend.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ProfileModel> _logger;

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

        public UserProfileResponse UserProfile { get; set; }

        public ProfileModel(IApiService apiService, ILogger<ProfileModel> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<UserProfileResponse>("/api/users/profile");
                
                if (response.IsSuccess && response.Data != null)
                {
                    UserProfile = response.Data;
                    return Page();
                }

                if (!response.IsSuccess)
                {
                    return RedirectToPage("/Account/Login");
                }

                throw new Exception($"Erreur lors de la récupération du profil : {response.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du profil");
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la récupération de votre profil";
                return RedirectToPage("/Index");
            }
        }

        public IActionResult OnPostLogout()
        {
            Response.Cookies.Delete("WebPortalX.Auth");
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
}