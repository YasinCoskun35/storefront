using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Storefront.IntegrationTests.Infrastructure;

namespace Storefront.IntegrationTests.API;

public class CartAndOrderTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public CartAndOrderTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Cart operations ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetCart_WithoutAuth_Should_Return_Unauthorized()
    {
        var response = await _client.GetAsync("/api/partner/cart");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCart_WhenEmpty_Should_Return_Empty_Cart()
    {
        // Arrange
        var partnerToken = await CreateApproveAndLoginPartnerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        // Act
        var response = await _client.GetAsync("/api/partner/cart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartDto>(_json);
        cart.Should().NotBeNull();
        cart!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task AddToCart_Should_Return_CartId()
    {
        // Arrange
        var partnerToken = await CreateApproveAndLoginPartnerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        var productId = await CreateProductAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner/cart/items", new
        {
            ProductId = productId,
            ProductName = "Test Sofa",
            ProductSKU = "SOF-001",
            ProductImageUrl = (string?)null,
            Quantity = 2,
            SelectedVariants = (string?)null,
            CustomizationNotes = (string?)null
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AddToCartResult>(_json);
        content!.CartId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AddToCart_Should_Capture_UnitPrice_From_Catalog()
    {
        // Arrange
        var partnerToken = await CreateApproveAndLoginPartnerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        var productId = await CreateProductAsync(price: 199.99m);

        // Act
        await _client.PostAsJsonAsync("/api/partner/cart/items", new
        {
            ProductId = productId,
            ProductName = "Priced Chair",
            ProductSKU = "CHR-001",
            ProductImageUrl = (string?)null,
            Quantity = 1,
            SelectedVariants = (string?)null,
            CustomizationNotes = (string?)null
        });

        var cartResponse = await _client.GetAsync("/api/partner/cart");
        cartResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>(_json);

        // Assert — UnitPrice populated from catalog
        cart!.Items.Should().HaveCount(1);
        cart.Items[0].UnitPrice.Should().Be(199.99m);
    }

    [Fact]
    public async Task AddToCart_WithoutAuth_Should_Return_Unauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/partner/cart/items", new
        {
            ProductId = Guid.NewGuid().ToString(),
            ProductName = "Test",
            ProductSKU = "T-001",
            ProductImageUrl = (string?)null,
            Quantity = 1,
            SelectedVariants = (string?)null,
            CustomizationNotes = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveFromCart_Should_Return_NoContent()
    {
        // Arrange
        var partnerToken = await CreateApproveAndLoginPartnerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        var productId = await CreateProductAsync();

        await _client.PostAsJsonAsync("/api/partner/cart/items", new
        {
            ProductId = productId,
            ProductName = "Table",
            ProductSKU = "TBL-001",
            ProductImageUrl = (string?)null,
            Quantity = 1,
            SelectedVariants = (string?)null,
            CustomizationNotes = (string?)null
        });

        var cartResponse = await _client.GetAsync("/api/partner/cart");
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>(_json);
        var itemId = cart!.Items[0].Id;

        // Act
        var removeResponse = await _client.DeleteAsync($"/api/partner/cart/items/{itemId}");

        // Assert
        removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cartAfter = await (await _client.GetAsync("/api/partner/cart"))
            .Content.ReadFromJsonAsync<CartDto>(_json);
        cartAfter!.Items.Should().BeEmpty();
    }

    // ── Order creation ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrder_WithEmptyCart_Should_Return_BadRequest()
    {
        // Arrange
        var partnerToken = await CreateApproveAndLoginPartnerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        // Act (no items added)
        var response = await _client.PostAsJsonAsync("/api/partner/orders", new
        {
            DeliveryAddress = "123 Test St",
            DeliveryCity = "Istanbul",
            DeliveryState = "Istanbul",
            DeliveryPostalCode = "34000",
            DeliveryCountry = "Turkey",
            DeliveryNotes = (string?)null,
            RequestedDeliveryDate = (DateTime?)null,
            Notes = (string?)null
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithItems_Should_Return_Created()
    {
        // Arrange
        var partnerToken = await CreateApproveAndLoginPartnerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        var productId = await CreateProductAsync();
        await _client.PostAsJsonAsync("/api/partner/cart/items", new
        {
            ProductId = productId,
            ProductName = "Desk",
            ProductSKU = "DSK-001",
            ProductImageUrl = (string?)null,
            Quantity = 2,
            SelectedVariants = (string?)null,
            CustomizationNotes = (string?)null
        });

        // Act
        var response = await _client.PostAsJsonAsync("/api/partner/orders", new
        {
            DeliveryAddress = "789 Order St",
            DeliveryCity = "Izmir",
            DeliveryState = "Izmir",
            DeliveryPostalCode = "35000",
            DeliveryCountry = "Turkey",
            DeliveryNotes = (string?)null,
            RequestedDeliveryDate = (DateTime?)null,
            Notes = "Integration test order"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreateOrderResult>(_json);
        result!.OrderId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateOrder_Should_Apply_Partner_Discount_To_Item_Prices()
    {
        // Arrange — create partner with 20% discount, add priced product, checkout
        var (companyId, partnerToken) = await CreateApproveAndLoginPartnerWithCompanyIdAsync();

        // Set 20% discount on partner
        await AuthAsAdminAsync();
        await _client.PutAsJsonAsync($"/api/identity/admin/partners/{companyId}/pricing",
            new { DiscountRate = 20m });

        // Switch to partner auth and add product to cart
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);
        var productId = await CreateProductAsync(price: 100m);
        await _client.PostAsJsonAsync("/api/partner/cart/items", new
        {
            ProductId = productId,
            ProductName = "Armchair",
            ProductSKU = "ARM-001",
            ProductImageUrl = (string?)null,
            Quantity = 2,
            SelectedVariants = (string?)null,
            CustomizationNotes = (string?)null
        });

        // Act
        var orderResponse = await _client.PostAsJsonAsync("/api/partner/orders", new
        {
            DeliveryAddress = "1 Main St",
            DeliveryCity = "Istanbul",
            DeliveryState = "Istanbul",
            DeliveryPostalCode = "34000",
            DeliveryCountry = "Turkey",
            DeliveryNotes = (string?)null,
            RequestedDeliveryDate = (DateTime?)null,
            Notes = (string?)null
        });

        // Assert
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var orderResult = await orderResponse.Content.ReadFromJsonAsync<CreateOrderResult>(_json);

        // Fetch order details via admin
        await AuthAsAdminAsync();
        var detailsResponse = await _client.GetAsync($"/api/admin/orders/{orderResult!.OrderId}");
        detailsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await detailsResponse.Content.ReadFromJsonAsync<OrderDetailDto>(_json);

        // 20% discount on 100 TRY × 2 items → discounted price = 80, total = 160
        order!.Items.Should().HaveCount(1);
        order.Items[0].UnitPrice.Should().Be(80m);        // 100 * 0.80
        order.Items[0].TotalPrice.Should().Be(160m);      // 80 * 2
        order.Items[0].Discount.Should().Be(40m);         // 20 * 2
        order.TotalAmount.Should().Be(160m);
    }

    [Fact]
    public async Task CreateOrder_WithNoDiscount_Should_Use_Full_Price()
    {
        // Arrange
        var partnerToken = await CreateApproveAndLoginPartnerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        var productId = await CreateProductAsync(price: 250m);
        await _client.PostAsJsonAsync("/api/partner/cart/items", new
        {
            ProductId = productId,
            ProductName = "Bookshelf",
            ProductSKU = "BSH-001",
            ProductImageUrl = (string?)null,
            Quantity = 1,
            SelectedVariants = (string?)null,
            CustomizationNotes = (string?)null
        });

        // Act
        var orderResponse = await _client.PostAsJsonAsync("/api/partner/orders", new
        {
            DeliveryAddress = "5 Oak Ave",
            DeliveryCity = "Bursa",
            DeliveryState = "Bursa",
            DeliveryPostalCode = "16000",
            DeliveryCountry = "Turkey",
            DeliveryNotes = (string?)null,
            RequestedDeliveryDate = (DateTime?)null,
            Notes = (string?)null
        });

        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await orderResponse.Content.ReadFromJsonAsync<CreateOrderResult>(_json);

        // Fetch via admin
        await AuthAsAdminAsync();
        var order = await (await _client.GetAsync($"/api/admin/orders/{result!.OrderId}"))
            .Content.ReadFromJsonAsync<OrderDetailDto>(_json);

        // No discount → full price
        order!.Items[0].UnitPrice.Should().Be(250m);
        order.Items[0].Discount.Should().Be(0m);
        order.TotalAmount.Should().Be(250m);
    }

    [Fact]
    public async Task CartIsCleared_After_Order_Creation()
    {
        // Arrange
        var partnerToken = await CreateApproveAndLoginPartnerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", partnerToken);

        var productId = await CreateProductAsync();
        await _client.PostAsJsonAsync("/api/partner/cart/items", new
        {
            ProductId = productId,
            ProductName = "Wardrobe",
            ProductSKU = "WRD-001",
            ProductImageUrl = (string?)null,
            Quantity = 1,
            SelectedVariants = (string?)null,
            CustomizationNotes = (string?)null
        });

        await _client.PostAsJsonAsync("/api/partner/orders", new
        {
            DeliveryAddress = "10 Pine Rd",
            DeliveryCity = "Antalya",
            DeliveryState = "Antalya",
            DeliveryPostalCode = "07000",
            DeliveryCountry = "Turkey",
            DeliveryNotes = (string?)null,
            RequestedDeliveryDate = (DateTime?)null,
            Notes = (string?)null
        });

        // Act — check cart after order
        var cartResponse = await _client.GetAsync("/api/partner/cart");
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>(_json);

        // Assert
        cart!.Items.Should().BeEmpty();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> CreateApproveAndLoginPartnerAsync()
    {
        var (_, token) = await CreateApproveAndLoginPartnerWithCompanyIdAsync();
        return token;
    }

    private async Task<(string companyId, string partnerToken)> CreateApproveAndLoginPartnerWithCompanyIdAsync()
    {
        await AuthAsAdminAsync();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var partnerEmail = $"order-partner-{suffix}@test.com";
        var partnerPassword = "PartnerPass123!";

        var createResponse = await _client.PostAsJsonAsync("/api/identity/admin/partners", new
        {
            CompanyName = $"Order Test Co {suffix}",
            TaxId = $"OT-{suffix}",
            Email = $"ot-{suffix}@test.com",
            Phone = "555-0300",
            Address = "1 Commerce St",
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
                FirstName = "Order",
                LastName = "Test",
                Email = partnerEmail,
                Password = partnerPassword
            }
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CreatePartnerResult>(_json);
        var companyId = created!.Id;

        await _client.PutAsJsonAsync(
            $"/api/identity/admin/partners/{companyId}/approve",
            new { ApprovalNotes = (string?)null });

        var loginResponse = await _client.PostAsJsonAsync("/api/identity/partners/auth/login", new
        {
            Email = partnerEmail,
            Password = partnerPassword
        });
        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<PartnerLoginResponse>(_json);

        return (companyId, login!.AccessToken);
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

    private async Task<string> CreateProductAsync(decimal price = 99.99m)
    {
        await AuthAsAdminAsync();

        var response = await _client.PostAsJsonAsync("/api/catalog/products", new
        {
            Name = $"Test Product {Guid.NewGuid():N}",
            SKU = $"TP-{Guid.NewGuid():N}",
            Description = "Test product for cart/order tests",
            Price = price,
            CategoryId = (string?)null,
            StockStatus = "InStock",
            Quantity = 100,
            IsActive = true
        });
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductResult>(_json);
        return product!.Id;
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    private class AdminLoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private class PartnerLoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private class CreatePartnerResult
    {
        public string Id { get; set; } = string.Empty;
    }

    private class ProductResult
    {
        public string Id { get; set; } = string.Empty;
    }

    private class AddToCartResult
    {
        public string CartId { get; set; } = string.Empty;
    }

    private class CartDto
    {
        public string Id { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new();
    }

    private class CartItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }

    private class CreateOrderResult
    {
        public string OrderId { get; set; } = string.Empty;
    }

    private class OrderDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal? SubTotal { get; set; }
        public decimal? Discount { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    private class OrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Discount { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
