using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebPortalX.Infrastructure.Data;
using WebPortalX.Core.Models;

namespace WebPortalX.Tests.Integration
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Charger les variables d'environnement de test
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", "clé_secrète_de_test");
            Environment.SetEnvironmentVariable("EMAIL_USERNAME", "test@example.com");
            Environment.SetEnvironmentVariable("EMAIL_PASSWORD", "password_test");
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", "DataSource=:memory:");

            builder.ConfigureServices(services =>
            {
                // Remplacer la base de données par une base en mémoire pour les tests
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Créer et seeder la base de données
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                db.Database.EnsureCreated();
                SeedTestData(db);
            });
        }

        private void SeedTestData(ApplicationDbContext context)
        {
            // Ajouter les rôles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "User" }
                );
                context.SaveChanges();
            }

            // Ajouter un utilisateur de test
            if (!context.Users.Any())
            {
                var user = new UserManager
                {
                    UserName = "testuser",
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    IsActive = true,
                    EmailVerified = true,
                    Role = context.Roles.First(r => r.Name == "Admin")
                };
                user.SetPassword("Test123!");
                context.Users.Add(user);
                context.SaveChanges();
            }
        }
    }
} 