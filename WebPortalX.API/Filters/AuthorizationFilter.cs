using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace WebPortalX.API.Filters
{
    public class AuthorizationFilter : IAuthorizationFilter
    {
        private readonly ILogger<AuthorizationFilter> _logger;

        public AuthorizationFilter(ILogger<AuthorizationFilter> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
            
            if (allowAnonymous)
            {
                return;
            }

            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Tentative d'accès non autorisé");
                context.Result = new UnauthorizedResult();
                return;
            }
        }
    }
} 