using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur non gérée s'est produite");
            
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Requête AJAX
                await context.Response.WriteAsJsonAsync(new { error = "Une erreur est survenue" });
            }
            else
            {
                // Requête normale
                context.Response.Redirect("/Error");
            }
        }
    }
}

// Extension method
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
} 