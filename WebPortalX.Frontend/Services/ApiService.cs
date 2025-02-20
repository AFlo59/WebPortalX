using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WebPortalX.Core.Models.Responses;
using WebPortalX.Frontend.Interfaces;

namespace WebPortalX.Frontend.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
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

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
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

                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'appel GET à {endpoint}");
                throw;
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                SetAuthorizationHeader();
                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                
                _logger.LogInformation($"Envoi de requête POST à {endpoint}");
                var response = await _httpClient.PostAsync(endpoint, content);
                _logger.LogInformation($"Réponse reçue : {response.StatusCode}");
                
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la requête POST");
                throw;
            }
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
        {
            SetAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            return await HandleResponse<T>(response);
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync(endpoint);
            return await HandleResponse<T>(response);
        }

        private void SetAuthorizationHeader(string token = null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token) 
                ? null 
                : new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(content);
                return new ApiResponse<T> { IsSuccess = true, Data = data };
            }

            return new ApiResponse<T> 
            { 
                IsSuccess = false, 
                Message = content 
            };
        }
    }
} 