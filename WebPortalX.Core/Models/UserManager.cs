using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BCrypt.Net;

namespace WebPortalX.Core.Models
{
    [Table("UserManager")]
    public class UserManager : AbstractEntity, IAbstractTimestamp
    {
        [Column("UserName"), Required, MaxLength(255), Display(Name = "User Name"), 
        RegularExpression(@"^[a-zA-Z''-'\s]{1,20}$", ErrorMessage = "Invalid user name format.")]
        public string UserName { get; set; } = string.Empty;

        [Column("FirstName"), Required]
        public string FirstName { get; set; } = string.Empty;

        [Column("LastName"), Required, MaxLength(255), Display(Name = "Last Name"), 
        RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Invalid last name format.")]
        public string LastName { get; set; } = string.Empty;

        [Column("Email"), MaxLength(255), DataType(DataType.EmailAddress), Display(Name = "Email")]
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format de l'email invalide.")]
        [RegularExpression(@"^([\w\.-]+)@([\w-]+)((\.(\w){2,3})+)$", ErrorMessage = "Format de l'email invalide.")]
        public string Email { get; set; } = string.Empty;

        [Column("DateOfBirth"), Required, Display(Name = "Date of Birth"), DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        private string passwordHash;

        [Column("PasswordHash"), Required, MaxLength(255), Display(Name = "Password Hash")]
        public string PasswordHash
        {
            get => passwordHash;
            set => passwordHash = value;
        }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt"), Required, Display(Name = "Created At"), DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt"), Required, Display(Name = "Updated At"), DataType(DataType.DateTime)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("RefreshToken"), MaxLength(255)]
        public string RefreshToken { get; set; } = string.Empty;

        [Column("RefreshTokenExpiryTime"), DataType(DataType.DateTime)]
        public DateTime RefreshTokenExpiryTime { get; set; }

        [Column("EmailVerified")]
        public bool EmailVerified { get; set; }

        [Column("RoleId"), Required]
        public int RoleId { get; set; }
        
        public virtual Role Role { get; set; } = null!;

        // ✅ Méthode sécurisée pour mettre à jour un utilisateur
        public void UpdateUser(string userName, string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            UserName = userName ?? UserName;
            FirstName = firstName ?? FirstName;
            LastName = lastName ?? LastName;
            Email = email ?? Email;
            DateOfBirth = dateOfBirth;

            UpdateTimestamp();
        }

        public void SetPassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string passwordToCheck)
        {
            return BCrypt.Net.BCrypt.Verify(passwordToCheck, PasswordHash);
        }

        public void SetActiveStatus(bool status)
        {
            IsActive = status;
            UpdateTimestamp();
        }

        public void ChangeRole(Role newRole)
        {
            Role = newRole;
            RoleId = newRole.Id;
            UpdateTimestamp();
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}