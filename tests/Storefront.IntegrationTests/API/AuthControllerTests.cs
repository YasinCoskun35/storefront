using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Storefront.IntegrationTests.Infrastructure;

namespace Storefront.IntegrationTests.API;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_Should_Return_Success_With_Tokens()
    {
        // Arrange
        var loginDto = new
        {
            Email = "admin@storefront.com",
            Password = "AdminPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse.RefreshToken.Should().NotBeNullOrEmpty();
        loginResponse.User.Should().NotBeNull();
        loginResponse.User.Email.Should().Be(loginDto.Email);
        loginResponse.User.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Should_Return_Unauthorized()
    {
        // Arrange
        var loginDto = new
        {
            Email = "admin@storefront.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_Should_Return_BadRequest()
    {
        // Arrange
        var loginDto = new
        {
            Email = "not-an-email",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_Should_Return_BadRequest()
    {
        // Arrange
        var loginDto = new
        {
            Email = "admin@storefront.com",
            Password = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_Should_Return_New_Tokens()
    {
        // Arrange
        var loginDto = new
        {
            Email = "admin@storefront.com",
            Password = "AdminPassword123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/identity/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var refreshDto = new
        {
            RefreshToken = loginResult!.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity/auth/refresh", refreshDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResult = await response.Content.ReadFromJsonAsync<LoginResponse>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        refreshResult.Should().NotBeNull();
        refreshResult!.AccessToken.Should().NotBeNullOrEmpty();
        refreshResult.RefreshToken.Should().NotBeNullOrEmpty();
        refreshResult.AccessToken.Should().NotBe(loginResult.AccessToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_Should_Return_Unauthorized()
    {
        // Arrange
        var refreshDto = new
        {
            RefreshToken = "invalid-token-12345"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity/auth/refresh", refreshDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }

    private class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}

