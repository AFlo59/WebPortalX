using System.Net;
using System.Net.Http.Json;
using WebPortalX.Core.Models.Requests;
using Xunit;
using Microsoft.EntityFrameworkCore;
using WebPortalX.Core.Models;
using WebPortalX.Core.Models.Responses;

namespace WebPortalX.Tests.Integration
{
    public class ProfileTests : IntegrationTestBase
    {
        public ProfileTests(TestWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task UpdateProfile_WithValidData_ShouldSucceed()
        {
            // Arrange
            await AuthenticateAsync();
            var request = new UpdateUserRequest
            {
                UserName = "updateduser",
                FirstName = "Updated",
                LastName = "User",
                Email = "updated@example.com",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/users/profile", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = await Context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            Assert.NotNull(user);
            Assert.Equal(request.UserName, user.UserName);
            Assert.Equal(request.FirstName, user.FirstName);
        }

        [Fact]
        public async Task UpdateProfile_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            await AuthenticateAsync();
            var request = new UpdateUserRequest
            {
                UserName = "", // Invalid: empty username
                FirstName = "Updated",
                LastName = "User",
                Email = "invalid-email", // Invalid email format
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/users/profile", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
} 