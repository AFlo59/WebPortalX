using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WebPortalX.Frontend.Services
{
    public interface IApiService
    {
        Task<HttpResponseMessage> GetAsync(string endpoint, string token = null);
        Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, string token = null);
        Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, string token = null);
        Task<HttpResponseMessage> DeleteAsync(string endpoint, string token = null);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private const string API_BASE_URL = "http://localhost:5165";
        private readonly ILogger<ApiService> _logger;

        public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _logger = logger;
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint, string token = null)
        {
            SetAuthorizationHeader(token);
            return await _httpClient.GetAsync($"{API_BASE_URL}{endpoint}");
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