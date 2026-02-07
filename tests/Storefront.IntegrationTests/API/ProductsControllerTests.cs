using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Storefront.IntegrationTests.Infrastructure;
using Storefront.Modules.Catalog.Core.Application.DTOs;
using Storefront.Modules.Catalog.Core.Domain.Enums;
using Storefront.SharedKernel;

namespace Storefront.IntegrationTests.API;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
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
        // Arrange
        var productDto = new
        {
            Name = "Test Drill",
            SKU = "TD-001",
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

        var productDto = new
        {
            Name = "Cordless Drill Pro",
            SKU = $"CDP-{Guid.NewGuid():N}",
            Description = "Professional cordless drill for heavy-duty work",
            Price = 199.99m,
            CategoryId = (string?)null,
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 25,
            IsActive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/catalog/products", productDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Name.Should().Be(productDto.Name);
        result.SKU.Should().Be(productDto.SKU);
        result.Price.Should().Be(productDto.Price);
    }

    [Fact]
    public async Task GetProductById_Should_Return_Product_Details()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new
        {
            Name = "Hammer Drill",
            SKU = $"HD-{Guid.NewGuid():N}",
            Description = "Heavy-duty hammer drill",
            Price = 149.99m,
            CategoryId = (string?)null,
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 15,
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/catalog/products", createDto);
        createResponse.EnsureSuccessStatusCode();

        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductDto>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act
        var response = await _client.GetAsync($"/api/catalog/products/{createdProduct!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<ProductDto>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        product.Should().NotBeNull();
        product!.Id.Should().Be(createdProduct.Id);
        product.Name.Should().Be(createDto.Name);
        product.SKU.Should().Be(createDto.SKU);
    }

    [Fact]
    public async Task SearchProducts_Should_Return_Matching_Products()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a product with searchable name
        var createDto = new
        {
            Name = "DeWalt Cordless Impact Driver",
            SKU = $"DW-{Guid.NewGuid():N}",
            Description = "20V MAX impact driver",
            Price = 129.99m,
            CategoryId = (string?)null,
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 20,
            IsActive = true
        };

        await _client.PostAsJsonAsync("/api/catalog/products", createDto);

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

        var sku = $"DUP-{Guid.NewGuid():N}";

        var product1 = new
        {
            Name = "Product 1",
            SKU = sku,
            Description = "First product",
            Price = 50m,
            CategoryId = (string?)null,
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
            CategoryId = (string?)null,
            StockStatus = StockStatus.InStock.ToString(),
            Quantity = 5,
            IsActive = true
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/catalog/products", product1);
        var response2 = await _client.PostAsJsonAsync("/api/catalog/products", product2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    private async Task<string> GetAuthTokenAsync()
    {
        // Login with default admin credentials
        var loginDto = new
        {
            Email = "admin@storefront.com",
            Password = "AdminPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/identity/auth/login", loginDto);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return loginResponse!.AccessToken;
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

