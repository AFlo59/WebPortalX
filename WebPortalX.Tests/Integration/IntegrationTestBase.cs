using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using WebPortalX.Infrastructure.Data;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using WebPortalX.Core.Models.Responses;
using WebPortalX.Core.Models;
using System.Text.Json;
using WebPortalX.Core.Models.Requests;

namespace WebPortalX.Tests.Integration
{
    public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>
    {
        protected readonly TestWebApplicationFactory Factory;
        protected readonly HttpClient Client;
        protected readonly ApplicationDbContext Context;

        protected IntegrationTestBase(TestWebApplicationFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
            Context = Factory.Services.CreateScope()
                .ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        protected async Task AuthenticateAsync()
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer", await GetJwtAsync());
        }

        private async Task<string> GetJwtAsync()
        {
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Test123!"
            };

            var response = await Client.PostAsJsonAsync("/api/users/login", loginRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login failed: {response.StatusCode}, {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            try 
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return loginResponse?.Token ?? throw new Exception("Token not received");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse response: {content}", ex);
            }
        }
    }
} 