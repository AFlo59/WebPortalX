using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPortalX.Core.Models
{
    [Table("RoleManager")]
    public class RoleManager : AbstractEntity, IAbstractTimestamp
    {
        [Column("RoleName"), Required, MaxLength(50)]
        public string RoleName { get; private set; }

        [Column("CreatedAt"), Required]
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        [Column("UpdatedAt"), Required]
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        [Column("Description"), MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public RoleManager(string roleName)
        {
            RoleName = roleName;
        }

        public void UpdateRoleName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Role name cannot be empty.");

            RoleName = newName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
