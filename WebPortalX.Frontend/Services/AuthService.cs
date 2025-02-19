using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WebPortalX.Core.Models.Responses;

public interface IAuthService
{
    Task<bool> ValidateTokenAsync(string token);
    Task<string> RefreshTokenAsync(string token);
    void StoreToken(string token, bool rememberMe);
    void RemoveToken();
    string GetToken();
}

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IHttpClientFactory clientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _clientFactory = clientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var client = _clientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("/api/users/validate-token");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la validation du token");
            return false;
        }
    }

    public void StoreToken(string token, bool rememberMe)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(1)
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append("WebPortalX.Auth", token, cookieOptions);
    }

    public void RemoveToken()
    {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("WebPortalX.Auth");
    }

    public string GetToken()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies["WebPortalX.Auth"] ?? string.Empty;
    }

    public async Task<string> RefreshTokenAsync(string token)
    {
        try
        {
            var client = _clientFactory.CreateClient("API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsync("/api/users/refresh-token", null);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var refreshResponse = JsonSerializer.Deserialize<RefreshTokenResponse>(content);
                return refreshResponse?.Token ?? string.Empty;
            }
            
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
            return string.Empty;
        }
    }
} 