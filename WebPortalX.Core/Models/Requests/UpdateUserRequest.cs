using System.ComponentModel.DataAnnotations;

namespace WebPortalX.Core.Models.Requests
{
    public class UpdateUserRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string CurrentPassword { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Le mot de passe doit faire au moins {2} caractères", MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
} 