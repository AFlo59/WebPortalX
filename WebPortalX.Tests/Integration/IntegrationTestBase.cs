using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using WebPortalX.Infrastructure.Data;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using WebPortalX.Core.Models.Responses;
using WebPortalX.Core.Models;

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
            var response = await Client.PostAsJsonAsync("/api/users/login", new
            {
                Email = "test@example.com",
                Password = "Test123!"
            });

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return loginResponse.Token;
        }
    }
} 