using System.ComponentModel.DataAnnotations;

namespace WebPortalX.Core.Models.Requests
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est requis")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de naissance est requise")]
        public DateTime DateOfBirth { get; set; }

        public string CurrentPassword { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Le mot de passe doit faire au moins {2} caractères", MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
} 