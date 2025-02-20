using WebPortalX.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace WebPortalX.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            Console.WriteLine("🔄 Initialisation de la base de données...");
            
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Vérifier si la base de données existe
            if (context.Database.EnsureCreated())
            {
                Console.WriteLine("✅ Base de données créée");
            }

            // Vérifier si des rôles existent déjà
            if (!context.Roles.Any())
            {
                Console.WriteLine("➕ Création des rôles...");
                var roles = new[]
                {
                    new Role { Name = "Admin", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsActive = true },
                    new Role { Name = "FreeUser", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsActive = true },
                    new Role { Name = "PremiumUser", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now, IsActive = true }
                };

                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Rôles créés");
            }

            // Créer l'utilisateur admin s'il n'existe pas
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            Console.WriteLine($"🔍 Role Admin trouvé : {adminRole?.Name}");

            if (adminRole != null)
            {
                Console.WriteLine("➕ Création de l'utilisateur admin...");
                var adminUser = new UserManager
                {
                    UserName = Environment.GetEnvironmentVariable("ADMIN_USERNAME"),
                    FirstName = Environment.GetEnvironmentVariable("ADMIN_FIRSTNAME"),
                    LastName = Environment.GetEnvironmentVariable("ADMIN_LASTNAME"),
                    Email = Environment.GetEnvironmentVariable("ADMIN_EMAIL"),
                    DateOfBirth = DateTime.Parse(Environment.GetEnvironmentVariable("ADMIN_BIRTHDATE")),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Role = adminRole,
                    EmailVerified = true
                };

                // Définir le mot de passe admin
                adminUser.SetPassword(Environment.GetEnvironmentVariable("ADMIN_PASSWORD"));

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }

            // Créer l'utilisateur de test s'il n'existe pas
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "FreeUser");
            if (userRole != null)
            {
                var testUser = new UserManager
                {
                    UserName = Environment.GetEnvironmentVariable("TEST_USERNAME"),
                    FirstName = Environment.GetEnvironmentVariable("TEST_FIRSTNAME"),
                    LastName = Environment.GetEnvironmentVariable("TEST_LASTNAME"),
                    Email = Environment.GetEnvironmentVariable("TEST_EMAIL"),
                    DateOfBirth = DateTime.Parse(Environment.GetEnvironmentVariable("TEST_BIRTHDATE")),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Role = userRole,
                    EmailVerified = true
                };

                // Définir le mot de passe de test
                testUser.SetPassword(Environment.GetEnvironmentVariable("TEST_PASSWORD"));

                context.Users.Add(testUser);
                await context.SaveChangesAsync();
            }
        }
    }
} 