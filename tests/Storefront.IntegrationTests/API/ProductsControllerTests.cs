using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Storefront.IntegrationTests.Infrastructure;
using Storefront.Modules.Catalog.Core.Application.DTOs;
using Storefront.Modules.Catalog.Core.Domain.Enums;

namespace Storefront.IntegrationTests.API;

[Collection("IntegrationTests")]
public class ProductsControllerTests
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_Should_Return_Success()
    {
        // Act
        var response = await _client.GetAsync("/api/catalog/products?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_Should_Return_Unauthorized()
    {
        // Arrange — no auth header set
        var productDto = new
        {
            Name = "Test Drill",
            SKU = $"TD-{Guid.NewGuid():N}",
            Description = "A test drill",
            Price = 99.99m,
            CategoryId = Guid.NewGuid().ToString(),
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 10,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/catalog/products", productDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithAuth_Should_Return_Success()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var categoryId = await CreateCategoryAsync();

        var sku = $"CDP-{Guid.NewGuid():N}";
        var productDto = new
        {
            Name = "Cordless Drill Pro",
            SKU = sku,
            Description = "Professional cordless drill for heavy-duty work",
            Price = 199.99m,
            CategoryId = categoryId,
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 25,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/catalog/products", productDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResult = await response.Content.ReadFromJsonAsync<CreateProductResult>(_json);
        createResult.Should().NotBeNull();
        createResult!.Id.Should().NotBeNullOrEmpty();

        // Verify by fetching the created product
        var getResponse = await _client.GetAsync($"/api/catalog/products/{createResult.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>(_json);
        product!.Name.Should().Be(productDto.Name);
        product.SKU.Should().Be(productDto.SKU);
    }

    [Fact]
    public async Task GetProductById_Should_Return_Product_Details()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var categoryId = await CreateCategoryAsync();

        var createDto = new
        {
            Name = "Hammer Drill",
            SKU = $"HD-{Guid.NewGuid():N}",
            Description = "Heavy-duty hammer drill",
            Price = 149.99m,
            CategoryId = categoryId,
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 15,
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/catalog/products", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateProductResult>(_json);

        // Act
        var response = await _client.GetAsync($"/api/catalog/products/{createResult!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(_json);

        product.Should().NotBeNull();
        product!.Id.Should().Be(createResult.Id);
        product.Name.Should().Be(createDto.Name);
        product.SKU.Should().Be(createDto.SKU);
    }

    [Fact]
    public async Task SearchProducts_Should_Return_Matching_Products()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var categoryId = await CreateCategoryAsync();

        var createDto = new
        {
            Name = "DeWalt Cordless Impact Driver",
            SKU = $"DW-{Guid.NewGuid():N}",
            Description = "20V MAX impact driver",
            Price = 129.99m,
            CategoryId = categoryId,
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 20,
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/catalog/products", createDto);
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await _client.GetAsync("/api/catalog/products?searchTerm=dewalt&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("DeWalt");
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSKU_Should_Return_Conflict()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var categoryId = await CreateCategoryAsync();

        var sku = $"DUP-{Guid.NewGuid():N}";

        var product1 = new
        {
            Name = "Product 1",
            SKU = sku,
            Description = "First product",
            Price = 50m,
            CategoryId = categoryId,
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 10,
            IsActive = true
        };

        var product2 = new
        {
            Name = "Product 2",
            SKU = sku,
            Description = "Second product with same SKU",
            Price = 75m,
            CategoryId = categoryId,
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 5,
            IsActive = true
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/catalog/products", product1);
        var response2 = await _client.PostAsJsonAsync("/api/catalog/products", product2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> GetAuthTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/auth/login", new
        {
            Email = "admin@storefront.com",
            Password = "AdminPassword123!"
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_json);
        return result!.AccessToken;
    }

    private async Task<string> CreateCategoryAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/catalog/categories", new
        {
            Name = $"Test Category {Guid.NewGuid():N}",
            Description = "Test category"
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateProductResult>(_json);
        return result!.Id;
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

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

    private class CreateProductResult
    {
        public string Id { get; set; } = string.Empty;
    }
}
