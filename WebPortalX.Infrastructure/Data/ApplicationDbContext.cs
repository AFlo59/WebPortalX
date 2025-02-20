using Microsoft.EntityFrameworkCore;
using WebPortalX.Core.Models;

namespace WebPortalX.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserManager> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la relation User-Role
            modelBuilder.Entity<UserManager>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration de la table Users
            modelBuilder.Entity<UserManager>()
                .ToTable("Users");

            // Configuration de la table Roles
            modelBuilder.Entity<Role>()
                .ToTable("Roles");
        }
    }
}
