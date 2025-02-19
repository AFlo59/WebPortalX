using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;

public class AuthenticationStateMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationStateMiddleware> _logger;

    public AuthenticationStateMiddleware(RequestDelegate next, ILogger<AuthenticationStateMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Cookies["WebPortalX.Auth"];
        _logger.LogInformation($"Token présent : {!string.IsNullOrEmpty(token)}");

        if (!string.IsNullOrEmpty(token))
        {
            context.Items["IsAuthenticated"] = true;
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }

        await _next(context);
    }
}

// Extension method
public static class AuthenticationStateMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationState(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationStateMiddleware>();
    }
} 