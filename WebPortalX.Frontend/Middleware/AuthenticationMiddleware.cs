using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace WebPortalX.Frontend.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        private readonly string[] _publicPaths = new[] 
        { 
            "/",
            "/index",
            "/account/login", 
            "/account/register",
            "/account/forgotpassword",
            "/images",
            "/css",
            "/js",
            "/lib"
        };

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            _logger.LogInformation($"Requête vers : {path}");

            var token = context.Request.Cookies["WebPortalX.Auth"];
            
            // Ajouter l'état d'authentification au contexte
            context.Items["IsAuthenticated"] = !string.IsNullOrEmpty(token);

            // Si c'est un chemin public, on continue sans vérification
            if (_publicPaths.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            }

            // Vérifier l'authentification pour les chemins protégés
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning($"Accès non autorisé à {path} - Token manquant");
                context.Response.Redirect("/Account/Login");
                return;
            }

            // Token présent, on l'ajoute aux headers
            context.Request.Headers["Authorization"] = $"Bearer {token}";
            await _next(context);
        }
    }

    // Extension method pour faciliter l'enregistrement du middleware
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomAuthentication(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
} 