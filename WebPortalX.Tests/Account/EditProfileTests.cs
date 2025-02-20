using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using WebPortalX.Core.Models.Requests;
using System.Text.Json;

namespace WebPortalX.Tests.Account;

public class EditProfileTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public EditProfileTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task UpdateProfile_ReturnsSuccess()
    {
        // Arrange
        // 1. Register an admin user first
        var registerRequest = new RegisterRequest
        {
            UserName = "adminuser",
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@webportalx.com",
            Password = "Admin123!",
            DateOfBirth = new DateTime(1990, 1, 1),
            Role = "Admin"  // Spécifier le rôle Admin
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/users/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // 2. Login as admin to get token
        var loginRequest = new LoginRequest 
        { 
            Email = "admin@webportalx.com",
            Password = "Admin123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/users/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(loginContent, _jsonOptions);
        Assert.NotNull(tokenResponse?.Token);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

        // 3. Update profile
        var updateRequest = new UpdateUserRequest
        {
            UserName = "adminuser_updated",
            FirstName = "Admin Updated",
            LastName = "User Updated",
            Email = "admin_updated@webportalx.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/users/profile", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify the update
        var profileResponse = await _client.GetAsync("/api/users/profile");
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileResponse>(_jsonOptions);
        Assert.NotNull(profile);
        Assert.Equal(updateRequest.UserName, profile.UserName);
        Assert.Equal(updateRequest.FirstName, profile.FirstName);
        Assert.Equal(updateRequest.LastName, profile.LastName);
        Assert.Equal(updateRequest.Email, profile.Email);
        Assert.Equal("Admin", profile.Role);
    }
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
}

public class UserProfileResponse
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Role { get; set; } = string.Empty;
} 