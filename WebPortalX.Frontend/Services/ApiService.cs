using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace WebPortalX.Frontend.Services
{
    public interface IApiService
    {
        Task<HttpResponseMessage> GetAsync(string endpoint);
        Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, string token = null);
        Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, string token = null);
        Task<HttpResponseMessage> DeleteAsync(string endpoint, string token = null);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://localhost:5165";
        private readonly ILogger<ApiService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(IHttpClientFactory clientFactory, ILogger<ApiService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = clientFactory.CreateClient("API");
            _logger = logger;
            _clientFactory = clientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            try
            {
                var client = _clientFactory.CreateClient("API");
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["WebPortalX.Auth"];

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                _logger.LogInformation($"Envoi de requête GET à {endpoint}");
                var response = await client.GetAsync(endpoint);
                _logger.LogInformation($"Réponse reçue : {response.StatusCode}");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'appel GET à {endpoint}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, string token = null)
        {
            try
            {
                SetAuthorizationHeader(token);
                var content = new StringContent(
                    JsonSerializer.Serialize(data), 
                    Encoding.UTF8, 
                    "application/json");
                
                var url = $"{API_BASE_URL}{endpoint}";
                _logger.LogInformation($"Envoi de requête POST à {url}");
                
                var response = await _httpClient.PostAsync(url, content);
                _logger.LogInformation($"Réponse reçue : {response.StatusCode}");
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la requête POST");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, string token = null)
        {
            SetAuthorizationHeader(token);
            var content = new StringContent(
                JsonSerializer.Serialize(data), 
                Encoding.UTF8, 
                "application/json");
            return await _httpClient.PutAsync($"{API_BASE_URL}{endpoint}", content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint, string token = null)
        {
            SetAuthorizationHeader(token);
            return await _httpClient.DeleteAsync($"{API_BASE_URL}{endpoint}");
        }

        private void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token) 
                ? null 
                : new AuthenticationHeaderValue("Bearer", token);
        }
    }
} 