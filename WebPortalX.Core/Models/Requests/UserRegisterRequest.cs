using System;
using System.ComponentModel.DataAnnotations;

namespace WebPortalX.Core.Models.Requests
{
    public class UserRegisterRequest
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,20}$", ErrorMessage = "Format du nom d'utilisateur invalide.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom de famille est obligatoire.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Format du nom de famille invalide.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [RegularExpression(@"^([\\w\\.-]+)@([\\w-]+)((\\.(\\w){2,3})+)$", ErrorMessage = "Format de l'email invalide.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$", ErrorMessage = "Le mot de passe doit contenir au moins une lettre majuscule, une lettre minuscule et un chiffre.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de naissance est obligatoire.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(UserRegisterRequest), nameof(ValidateDateOfBirth))]
        public DateTime DateOfBirth { get; set; }

        public static ValidationResult ValidateDateOfBirth(DateTime date, ValidationContext context)
        {
            int age = DateTime.Today.Year - date.Year;
            if (date > DateTime.Today.AddYears(-age)) age--;

            return age < 18 ? new ValidationResult("Vous devez avoir au moins 18 ans.") : ValidationResult.Success;
        }
    }
}