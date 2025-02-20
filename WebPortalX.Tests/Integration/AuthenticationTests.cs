using System.Net;
using System.Net.Http.Json;
using WebPortalX.Core.Models.Requests;
using Xunit;
using Microsoft.EntityFrameworkCore;
using WebPortalX.Core.Models;
using WebPortalX.Core.Models.Responses;

namespace WebPortalX.Tests.Integration
{
    public class AuthenticationTests : IntegrationTestBase
    {
        public AuthenticationTests(TestWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Register_WithValidData_ShouldSucceed()
        {
            // Arrange
            var request = new RegisterRequest
            {
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "Test123!",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/users/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = await Context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            Assert.NotNull(user);
            Assert.Equal(request.UserName, user.UserName);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Test123!"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/users/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginResponse?.Token);
        }

        [Fact]
        public async Task GetProfile_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await Client.GetAsync("/api/users/profile");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProfile_WithAuth_ShouldReturnUserProfile()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await Client.GetAsync("/api/users/profile");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
            Assert.NotNull(profile);
            Assert.Equal("test@example.com", profile.Email);
        }
    }
} 