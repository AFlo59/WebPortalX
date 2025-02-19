using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using WebPortalX.Infrastructure.Data;
using WebPortalX.Infrastructure.Services;
using System.IO;
using Microsoft.OpenApi.Models;
using WebPortalX.Core.Interfaces;
using WebPortalX.Core.Models.Settings;
using System.Runtime.CompilerServices;

// Rendre les types internes visibles pour le projet de test
[assembly: InternalsVisibleTo("WebPortalX.Tests")]

// Rendre la classe Program publique
public class Program 
{ 
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configurer l'URL d'écoute
        builder.WebHost.UseUrls("http://localhost:5165");

        // Configuration manuelle des variables d'environnement
        const string envFile = ".env";
        if (File.Exists(envFile))
        {
            foreach (var line in File.ReadAllLines(envFile))
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0], parts[1]);
                }
            }
        }

        // ✅ Ajouter la documentation Swagger
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebPortalX API", Version = "v1" });
        });

        Console.WriteLine($"📂 Chemin actuel : {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"🔍 JWT_SECRET_KEY (env) : {Environment.GetEnvironmentVariable("JWT_SECRET_KEY")}");

        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

        // ✅ Vérifier si la clé existe DÉJÀ dans `.env`
        if (string.IsNullOrEmpty(jwtSecret))
        {
            // ✅ Générer une nouvelle clé sécurisée (256 bits)
            jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            // ✅ Vérifier si `.env` contient déjà une clé JWT, sinon l'ajouter
            File.AppendAllText(envFile, $"\nJWT_SECRET_KEY={jwtSecret}");
            Console.WriteLine("🔑 Nouvelle clé JWT générée et sauvegardée dans .env !");
        }
        else
        {
            // ✅ Afficher un message si la clé JWT existe déjà
            Console.WriteLine("🔑 Clé JWT existante trouvée, pas besoin de générer une nouvelle.");
        }

        // ✅ Récupérer les variables d'environnement ou utiliser des valeurs par défaut
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "WebPortalX";
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "WebPortalXUsers";
        var jwtExpiry = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "60");
        var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "Data Source=webportalx.db";

        // Configuration du JWT avec la clé secrète depuis .env
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"))
                    )
                };
            });

        // Configuration de l'email avec les credentials depuis .env
        builder.Services.Configure<EmailSettings>(options =>
        {
            options.SmtpHost = builder.Configuration["Email:SmtpHost"];
            options.SmtpPort = int.Parse(builder.Configuration["Email:SmtpPort"]);
            options.Username = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
            options.Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
            options.From = builder.Configuration["Email:From"];
        });

        // Configuration de la base de données avec la chaîne de connexion depuis .env
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")));

        // ✅ Ajouter les services de l'API
        builder.Services.AddControllers();
        // ✅ Ajouter les services CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                builder => builder
                    .WithOrigins("http://localhost:5075")  // Port du frontend
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        // Add services to the container
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IEmailService, EmailService>();

        // ✅ Créer l'application
        var app = builder.Build();

        // Initialiser la base de données
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                await DbInitializer.Initialize(services);
                Console.WriteLine("✅ Base de données initialisée avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de l'initialisation de la base de données : {ex.Message}");
            }
        }

        // ✅ Activer la documentation Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebPortalX API V1");
            });
        }
        // ✅ Activation du CORS
        app.UseCors("AllowFrontend");
        // ✅ Activer les points de terminaison de l'API
        app.UseRouting();
        // ✅ Activer l'authentification
        app.UseAuthentication();
        // ✅ Activer l'authentification
        app.UseAuthorization();
        // ✅ Activer les points de terminaison de l'API
        app.MapControllers();
        // ✅ Créer la base de données
        app.Run();
    }
}
