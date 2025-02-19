using System.ComponentModel.DataAnnotations;

namespace WebPortalX.Core.Models.Requests
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; }
    }
} 