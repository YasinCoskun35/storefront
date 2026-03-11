using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Storefront.IntegrationTests.Infrastructure;

namespace Storefront.IntegrationTests.API;

public class PartnerAuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public PartnerAuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PartnerLogin_WithValidCredentials_Should_Return_Tokens_And_DiscountRate()
    {
        // Arrange
        var (_, partnerEmail, partnerPassword) = await CreateAndApprovePartnerAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity/partners/auth/login", new
        {
            Email = partnerEmail,
            Password = partnerPassword
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PartnerLoginResponse>(_json);

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(partnerEmail);
        result.User.DiscountRate.Should().Be(0); // Default discount rate
        result.User.Company.Should().NotBeNull();
    }

    [Fact]
    public async Task PartnerLogin_WithInvalidPassword_Should_Return_BadRequest()
    {
        // Arrange
        var (_, partnerEmail, _) = await CreateAndApprovePartnerAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity/partners/auth/login", new
        {
            Email = partnerEmail,
            Password = "WrongPassword!"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PartnerLogin_WithNonExistentEmail_Should_Return_BadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/identity/partners/auth/login", new
        {
            Email = $"nonexistent-{Guid.NewGuid():N}@test.com",
            Password = "Password123!"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProfile_WithValidToken_Should_Return_Profile_With_DiscountRate()
    {
        // Arrange
        var (_, partnerEmail, partnerPassword) = await CreateAndApprovePartnerAsync();
        var token = await GetPartnerTokenAsync(partnerEmail, partnerPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/identity/partners/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<PartnerProfileResponse>(_json);

        profile.Should().NotBeNull();
        profile!.Email.Should().Be(partnerEmail);
        profile.DiscountRate.Should().Be(0);
        profile.Company.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProfile_WithoutToken_Should_Return_Unauthorized()
    {
        // Act (no auth header)
        var response = await _client.GetAsync("/api/identity/partners/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_DiscountRate_Should_Reflect_Updated_Value()
    {
        // Arrange — create partner, set discount rate, then login and get profile
        var (companyId, partnerEmail, partnerPassword) = await CreateAndApprovePartnerAsync();

        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.PutAsJsonAsync($"/api/identity/admin/partners/{companyId}/pricing", new { DiscountRate = 15m });

        var partnerToken = await GetPartnerTokenAsync(partnerEmail, partnerPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        // Act
        var response = await _client.GetAsync("/api/identity/partners/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<PartnerProfileResponse>(_json);
        profile!.DiscountRate.Should().Be(15m);
    }

    [Fact]
    public async Task PartnerLogin_DiscountRate_In_Response_Should_Match_DB_Value()
    {
        // Arrange
        var (companyId, partnerEmail, partnerPassword) = await CreateAndApprovePartnerAsync();

        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.PutAsJsonAsync($"/api/identity/admin/partners/{companyId}/pricing", new { DiscountRate = 20m });

        // Act — partner login after discount rate was set
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/identity/partners/auth/login", new
        {
            Email = partnerEmail,
            Password = partnerPassword
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PartnerLoginResponse>(_json);
        result!.User.DiscountRate.Should().Be(20m);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<(string companyId, string email, string password)> CreateAndApprovePartnerAsync()
    {
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var partnerEmail = $"partner-{suffix}@testcompany.com";
        var partnerPassword = "PartnerPass123!";

        var createResponse = await _client.PostAsJsonAsync("/api/identity/admin/partners", new
        {
            CompanyName = $"Test Company {suffix}",
            TaxId = $"TAX-{suffix}",
            Email = $"company-{suffix}@testcompany.com",
            Phone = "555-0100",
            Address = "123 Test St",
            City = "Istanbul",
            State = "Istanbul",
            PostalCode = "34000",
            Country = "Turkey",
            Industry = (string?)null,
            Website = (string?)null,
            EmployeeCount = (int?)null,
            AnnualRevenue = (decimal?)null,
            AdminUser = new
            {
                FirstName = "Partner",
                LastName = "Admin",
                Email = partnerEmail,
                Password = partnerPassword
            }
        });
        createResponse.EnsureSuccessStatusCode();

        var createResult = await createResponse.Content.ReadFromJsonAsync<CreatePartnerResult>(_json);
        var companyId = createResult!.Id;

        var approveResponse = await _client.PutAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/approve",
            new { ApprovalNotes = "Test approval" });
        approveResponse.EnsureSuccessStatusCode();

        return (companyId, partnerEmail, partnerPassword);
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/auth/login", new
        {
            Email = "admin@storefront.com",
            Password = "AdminPassword123!"
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AdminLoginResponse>(_json);
        return result!.AccessToken;
    }

    private async Task<string> GetPartnerTokenAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/identity/partners/auth/login", new
        {
            Email = email,
            Password = password
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PartnerLoginResponse>(_json);
        return result!.AccessToken;
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────

    private class AdminLoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private class PartnerLoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public PartnerUserDto User { get; set; } = null!;
    }

    private class PartnerUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal DiscountRate { get; set; }
        public PartnerCompanyDto Company { get; set; } = null!;
    }

    private class PartnerCompanyDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private class PartnerProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal DiscountRate { get; set; }
        public PartnerCompanyDto Company { get; set; } = null!;
    }

    private class CreatePartnerResult
    {
        public string Id { get; set; } = string.Empty;
    }
}
