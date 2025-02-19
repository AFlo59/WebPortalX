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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string[] _publicPaths = new[] 
        { 
            "/",
            "/index",
            "/account/login", 
            "/account/register", 
            "/account/forgotpassword",
            "/account/resetpassword",
            "/account/verifyemail",
            "/privacy",
            "/lib",
            "/css",
            "/js",
            "/images",
            "/favicon.ico"
        };

        public AuthenticationMiddleware(
            RequestDelegate next,
            ILogger<AuthenticationMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            _logger.LogInformation($"Requête vers : {path}");

            if (_publicPaths.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            }

            // Créer un scope pour résoudre le service
            using (var scope = _scopeFactory.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                var token = authService.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning($"Accès non autorisé à {path} - Token manquant");
                    context.Response.Redirect("/Account/Login");
                    return;
                }

                var isValid = await authService.ValidateTokenAsync(token);
                if (!isValid)
                {
                    _logger.LogWarning("Token invalide, tentative de rafraîchissement");
                    var newToken = await authService.RefreshTokenAsync(token);
                    if (string.IsNullOrEmpty(newToken))
                    {
                        authService.RemoveToken();
                        context.Response.Redirect("/Account/Login");
                        return;
                    }
                    authService.StoreToken(newToken, true);
                    token = newToken;
                }

                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers["Authorization"] = $"Bearer {token}";
                }

                await _next(context);
            }
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