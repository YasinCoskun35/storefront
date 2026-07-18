using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Storefront.IntegrationTests.Infrastructure;
using Storefront.Modules.Identity.Infrastructure.Persistence;

namespace Storefront.IntegrationTests.API;

[Collection("IntegrationTests")]
public class PartnerPasswordResetTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public PartnerPasswordResetTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_Should_Return_OK_Without_Leaking()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/partners/auth/forgot-password", new
        {
            Email = $"unknown-{Guid.NewGuid():N}@test.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_Should_Return_BadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/partners/auth/reset-password", new
        {
            Token = "not-a-real-token",
            NewPassword = "NewPassword123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_Should_Change_Password()
    {
        // Arrange — create a partner, then seed a reset token directly (the raw
        // token is only ever delivered by email, so tests plant its hash in the DB)
        var (partnerEmail, oldPassword) = await CreateAndApprovePartnerAsync();

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var user = await db.PartnerUsers.FirstAsync(pu => pu.Email == partnerEmail);
            user.PasswordResetTokenHash = tokenHash;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            await db.SaveChangesAsync();
        }

        const string newPassword = "BrandNewPass123!";

        // Act
        var resetResponse = await _client.PostAsJsonAsync("/api/identity/partners/auth/reset-password", new
        {
            Token = token,
            NewPassword = newPassword
        });

        // Assert
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var oldLogin = await _client.PostAsJsonAsync("/api/identity/partners/auth/login", new
        {
            Email = partnerEmail,
            Password = oldPassword
        });
        oldLogin.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var newLogin = await _client.PostAsJsonAsync("/api/identity/partners/auth/login", new
        {
            Email = partnerEmail,
            Password = newPassword
        });
        newLogin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_Should_Return_BadRequest()
    {
        var (partnerEmail, _) = await CreateAndApprovePartnerAsync();

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var user = await db.PartnerUsers.FirstAsync(pu => pu.Email == partnerEmail);
            user.PasswordResetTokenHash = tokenHash;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(-5);
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsJsonAsync("/api/identity/partners/auth/reset-password", new
        {
            Token = token,
            NewPassword = "NewPassword123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<(string email, string password)> CreateAndApprovePartnerAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/identity/auth/login", new
        {
            Email = "admin@storefront.com",
            Password = "AdminPassword123!"
        });
        loginResponse.EnsureSuccessStatusCode();
        var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>(_json);
        var adminToken = loginJson.GetProperty("accessToken").GetString()!;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var partnerEmail = $"reset-{suffix}@testcompany.com";
        var partnerPassword = "PartnerPass123!";

        var createResponse = await _client.PostAsJsonAsync("/api/identity/admin/partners", new
        {
            CompanyName = $"Reset Test Co {suffix}",
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
                FirstName = "Reset",
                LastName = "Tester",
                Email = partnerEmail,
                Password = partnerPassword
            }
        });
        createResponse.EnsureSuccessStatusCode();

        _client.DefaultRequestHeaders.Authorization = null;

        return (partnerEmail, partnerPassword);
    }
}
