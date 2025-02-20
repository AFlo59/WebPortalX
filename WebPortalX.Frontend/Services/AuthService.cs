using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WebPortalX.Core.Models.Responses;
using WebPortalX.Frontend.Interfaces;

public interface IAuthService
{
    Task<bool> ValidateTokenAsync(string token);
    Task<string> RefreshTokenAsync(string token);
    Task StoreTokenAsync(string token);
    void RemoveToken();
    string GetToken();
    Task<bool> LoginAsync(string email, string password);
    Task<bool> IsAuthenticatedAsync();
}

namespace WebPortalX.Frontend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;
        private readonly IApiService _apiService;

        public AuthService(
            IHttpClientFactory clientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger,
            IApiService apiService)
        {
            _clientFactory = clientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _apiService = apiService;
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

        public async Task StoreTokenAsync(string token)
        {
            _logger.LogInformation("Stockage du token");
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,  // Mettre à false pour le développement local
                SameSite = SameSiteMode.Lax,  // Changer à Lax pour le développement
                Expires = DateTime.UtcNow.AddHours(1)
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("WebPortalX.Auth", token, cookieOptions);
            _logger.LogInformation($"Token stocké avec succès : {token.Substring(0, 10)}...");
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

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _apiService.PostAsync<LoginResponse>("api/users/login", 
                    new { email, password });

                if (response.IsSuccess && !string.IsNullOrEmpty(response.Data.Token))
                {
                    // Le token est déjà stocké dans un cookie HTTP-only par l'API
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la connexion : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["WebPortalX.Auth"];
            var isAuthenticated = !string.IsNullOrEmpty(token);
            _logger.LogInformation($"Vérification de l'authentification - Token présent : {isAuthenticated}");
            if (isAuthenticated)
            {
                _logger.LogInformation($"Token trouvé : {token.Substring(0, 10)}...");
            }
            return isAuthenticated;
        }
    }
} 