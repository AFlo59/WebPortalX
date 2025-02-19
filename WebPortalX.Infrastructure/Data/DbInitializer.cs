using WebPortalX.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebPortalX.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // S'assurer que la base de données est créée
            await context.Database.EnsureCreatedAsync();

            // Vérifier si des rôles existent déjà
            if (!await context.Roles.AnyAsync())
            {
                // Ajouter les rôles par défaut
                var roles = new[]
                {
                    new Role { Name = "FreeUser" },
                    new Role { Name = "PremiumUser" },
                    new Role { Name = "Admin" }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }
        }
    }
} 