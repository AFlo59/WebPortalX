using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using WebPortalX.Frontend.Services;
using WebPortalX.Frontend.Pages.Account;
using System.Net.Http.Json;
using WebPortalX.Core.Models.Requests;
using WebPortalX.Frontend.Interfaces;

namespace WebPortalX.Frontend.Pages.Account
{
    [Authorize]
    public class EditProfileModel : PageModel
    {
        private readonly IApiService _apiService;
        private readonly ILogger<EditProfileModel> _logger;

        [BindProperty]
        public string UserName { get; set; } = string.Empty;
        
        [BindProperty]
        public string FirstName { get; set; } = string.Empty;
        
        [BindProperty]
        public string LastName { get; set; } = string.Empty;
        
        [BindProperty]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [BindProperty]
        public DateTime DateOfBirth { get; set; }

        [BindProperty]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;
        
        [BindProperty]
        public string ConfirmNewPassword { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public EditProfileModel(IApiService apiService, ILogger<EditProfileModel> logger)
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
                    var userProfile = response.Data;
                    // Remplir les champs avec les données du profil
                    UserName = userProfile.UserName;
                    FirstName = userProfile.FirstName;
                    LastName = userProfile.LastName;
                    Email = userProfile.Email;
                    DateOfBirth = userProfile.DateOfBirth;
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

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var updateRequest = new UpdateUserRequest
                {
                    UserName = UserName,
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    DateOfBirth = DateOfBirth,
                    CurrentPassword = CurrentPassword,
                    NewPassword = NewPassword
                };

                var response = await _apiService.PutAsync<UserProfileResponse>("/api/users/profile", updateRequest);

                if (response.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Profil mis à jour avec succès";
                    return RedirectToPage("/Account/Profile");
                }

                ModelState.AddModelError(string.Empty, response.Message ?? "Erreur lors de la mise à jour du profil");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du profil");
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la mise à jour du profil");
                return Page();
            }
        }
    }
} 