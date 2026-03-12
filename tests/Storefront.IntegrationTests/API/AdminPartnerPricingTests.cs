using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Storefront.IntegrationTests.Infrastructure;

namespace Storefront.IntegrationTests.API;

public class AdminPartnerPricingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public AdminPartnerPricingTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Pricing policy ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdatePricing_Should_Set_DiscountRate_On_Company()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/pricing",
            new { DiscountRate = 25m });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detailsResponse = await _client.GetAsync($"/api/identity/admin/partners/{companyId}");
        detailsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await detailsResponse.Content.ReadFromJsonAsync<PartnerDetailsDto>(_json);
        details!.DiscountRate.Should().Be(25m);
    }

    [Fact]
    public async Task UpdatePricing_WithRateAbove100_Should_Return_BadRequest()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/pricing",
            new { DiscountRate = 150m });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePricing_WithNegativeRate_Should_Return_BadRequest()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/pricing",
            new { DiscountRate = -5m });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdatePricing_WithNonExistentCompany_Should_Return_NotFound()
    {
        // Arrange
        await AuthAsAdminAsync();

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/identity/admin/partners/{Guid.NewGuid()}/pricing",
            new { DiscountRate = 10m });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePricing_WithoutAuth_Should_Return_Unauthorized()
    {
        // Act (no auth header set)
        var response = await _client.PutAsJsonAsync(
            $"/api/identity/admin/partners/{Guid.NewGuid()}/pricing",
            new { DiscountRate = 10m });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Account transactions ──────────────────────────────────────────────────

    [Fact]
    public async Task RecordOrderDebit_Should_Increase_CurrentBalance()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/account/transactions",
            new
            {
                Type = "OrderDebit",
                Amount = 5000m,
                PaymentMethod = (string?)null,
                OrderReference = "ORD-2024-0001",
                Notes = "Test order debit"
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var details = await GetPartnerDetailsAsync(companyId);
        details.CurrentBalance.Should().Be(5000m);
        details.Transactions.Should().HaveCount(1);
        details.Transactions[0].Type.Should().Be("OrderDebit");
        details.Transactions[0].Amount.Should().Be(5000m);
    }

    [Fact]
    public async Task RecordPaymentCredit_Should_Decrease_CurrentBalance()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        // First add a debit so balance is positive
        await _client.PostAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "OrderDebit", Amount = 10000m, PaymentMethod = (string?)null, OrderReference = (string?)null, Notes = (string?)null });

        // Act — record a payment
        var response = await _client.PostAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/account/transactions",
            new
            {
                Type = "PaymentCredit",
                Amount = 3000m,
                PaymentMethod = "BankTransfer",
                OrderReference = (string?)null,
                Notes = "Partial payment received"
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var details = await GetPartnerDetailsAsync(companyId);
        details.CurrentBalance.Should().Be(7000m); // 10000 - 3000
    }

    [Fact]
    public async Task RecordTransaction_WithZeroAmount_Should_Return_BadRequest()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "OrderDebit", Amount = 0m, PaymentMethod = (string?)null, OrderReference = (string?)null, Notes = (string?)null });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordTransaction_WithNegativeAmount_Should_Return_BadRequest()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "OrderDebit", Amount = -500m, PaymentMethod = (string?)null, OrderReference = (string?)null, Notes = (string?)null });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecordTransaction_WithInvalidType_Should_Return_BadRequest()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "InvalidType", Amount = 100m, PaymentMethod = (string?)null, OrderReference = (string?)null, Notes = (string?)null });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPartnerDetails_Should_Include_Transactions_And_Balance()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        await _client.PostAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "OrderDebit", Amount = 8000m, PaymentMethod = (string?)null, OrderReference = "ORD-001", Notes = (string?)null });

        await _client.PostAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "PaymentCredit", Amount = 2000m, PaymentMethod = "Cash", OrderReference = (string?)null, Notes = "Cash payment" });

        // Act
        var details = await GetPartnerDetailsAsync(companyId);

        // Assert
        details.CurrentBalance.Should().Be(6000m);
        details.Transactions.Should().HaveCount(2);
    }

    [Fact]
    public async Task MultipleTransactions_Should_Accumulate_Balance_Correctly()
    {
        // Arrange
        var companyId = await CreatePartnerAsync();
        await AuthAsAdminAsync();

        // Act — 3 debits + 1 credit
        await _client.PostAsJsonAsync($"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "OrderDebit", Amount = 1000m, PaymentMethod = (string?)null, OrderReference = (string?)null, Notes = (string?)null });
        await _client.PostAsJsonAsync($"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "OrderDebit", Amount = 2000m, PaymentMethod = (string?)null, OrderReference = (string?)null, Notes = (string?)null });
        await _client.PostAsJsonAsync($"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "PaymentCredit", Amount = 500m, PaymentMethod = "Check", OrderReference = (string?)null, Notes = (string?)null });
        await _client.PostAsJsonAsync($"/api/identity/admin/partners/{companyId}/account/transactions",
            new { Type = "OrderDebit", Amount = 3000m, PaymentMethod = (string?)null, OrderReference = (string?)null, Notes = (string?)null });

        // Assert: 1000 + 2000 - 500 + 3000 = 5500
        var details = await GetPartnerDetailsAsync(companyId);
        details.CurrentBalance.Should().Be(5500m);
        details.Transactions.Should().HaveCount(4);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> CreatePartnerAsync()
    {
        await AuthAsAdminAsync();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var response = await _client.PostAsJsonAsync("/api/identity/admin/partners", new
        {
            CompanyName = $"Pricing Test Co {suffix}",
            TaxId = $"TX-{suffix}",
            Email = $"co-{suffix}@test.com",
            Phone = "555-0200",
            Address = "456 Test Ave",
            City = "Ankara",
            State = "Ankara",
            PostalCode = "06000",
            Country = "Turkey",
            Industry = (string?)null,
            Website = (string?)null,
            EmployeeCount = (int?)null,
            AnnualRevenue = (decimal?)null,
            AdminUser = new
            {
                FirstName = "Test",
                LastName = "User",
                Email = $"user-{suffix}@test.com",
                Password = "TestPass123!"
            }
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreatePartnerResult>(_json);
        return result!.Id;
    }

    private async Task AuthAsAdminAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/auth/login", new
        {
            Email = "admin@storefront.com",
            Password = "AdminPassword123!"
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AdminLoginResponse>(_json);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", result!.AccessToken);
    }

    private async Task<PartnerDetailsDto> GetPartnerDetailsAsync(string companyId)
    {
        var response = await _client.GetAsync($"/api/identity/admin/partners/{companyId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PartnerDetailsDto>(_json))!;
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    private class AdminLoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private class CreatePartnerResult
    {
        public string Id { get; set; } = string.Empty;
    }

    private class PartnerDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal DiscountRate { get; set; }
        public decimal CurrentBalance { get; set; }
        public List<TransactionDto> Transactions { get; set; } = new();
    }

    private class TransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? OrderReference { get; set; }
        public string? Notes { get; set; }
    }
}
