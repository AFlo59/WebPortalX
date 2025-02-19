using System.ComponentModel.DataAnnotations;

namespace WebPortalX.Core.Models.Requests
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Le prénom est requis")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La date de naissance est requise")]
        public DateTime DateOfBirth { get; set; }

        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
} 