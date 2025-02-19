using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebPortalX.Frontend.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _publicPaths = new[] 
        { 
            "/Account/Login", 
            "/Account/Register", 
            "/Account/ForgotPassword",
            "/Account/ResetPassword",
            "/Account/VerifyEmail",
            "/swagger",
            "/health"
        };

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            var protectedPaths = new[] { "/account/profile", "/account/editprofile" };

            // Permettre l'accès aux ressources statiques et aux chemins publics
            if (path.StartsWith("/lib/") || 
                path.StartsWith("/css/") || 
                path.StartsWith("/js/") || 
                _publicPaths.Any(p => path.StartsWith(p.ToLower())))
            {
                await _next(context);
                return;
            }

            if (protectedPaths.Any(p => path.StartsWith(p)) && !context.Request.Cookies.ContainsKey("AuthToken"))
            {
                context.Response.Redirect("/Account/Login");
                return;
            }

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