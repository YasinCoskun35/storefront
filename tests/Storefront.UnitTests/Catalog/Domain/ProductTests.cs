using FluentAssertions;
using Storefront.Modules.Catalog.Core.Domain.Entities;
using Storefront.Modules.Catalog.Core.Domain.Enums;

namespace Storefront.UnitTests.Catalog.Domain;

public class ProductTests
{
    [Fact]
    public void Product_Can_Be_Created_With_Required_Properties()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Cordless Drill",
            SKU = "CD-001",
            Description = "Professional cordless drill",
            Price = 99.99m,
            CategoryId = "cat-1",
            StockStatus = StockStatus.InStock,
            Quantity = 50,
            IsActive = true
        };

        // Assert
        product.Name.Should().Be("Cordless Drill");
        product.SKU.Should().Be("CD-001");
        product.Price.Should().Be(99.99m);
        product.StockStatus.Should().Be(StockStatus.InStock);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Product_StockStatus_Can_Be_Updated()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Drill",
            SKU = "D-001",
            Price = 50m,
            CategoryId = "cat-1",
            StockStatus = StockStatus.InStock,
            Quantity = 10
        };

        // Act
        product.StockStatus = StockStatus.OutOfStock;
        product.Quantity = 0;

        // Assert
        product.StockStatus.Should().Be(StockStatus.OutOfStock);
        product.Quantity.Should().Be(0);
    }

    [Fact]
    public void Product_Price_Should_Be_Positive()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Product",
            SKU = "TP-001",
            Price = 100m,
            CategoryId = "cat-1",
            StockStatus = StockStatus.InStock,
            Quantity = 1
        };

        // Assert
        product.Price.Should().BePositive();
    }

    [Fact]
    public void Product_Can_Have_Multiple_Images()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Product",
            SKU = "TP-001",
            Price = 100m,
            CategoryId = "cat-1",
            StockStatus = StockStatus.InStock,
            Quantity = 1
        };

        var image1 = new ProductImage
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = product.Id,
            Url = "/uploads/image1.webp",
            Type = ImageType.Thumbnail,
            IsPrimary = true,
            DisplayOrder = 0
        };

        var image2 = new ProductImage
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = product.Id,
            Url = "/uploads/image2.webp",
            Type = ImageType.Large,
            IsPrimary = false,
            DisplayOrder = 1
        };

        // Act
        product.Images.Add(image1);
        product.Images.Add(image2);

        // Assert
        product.Images.Should().HaveCount(2);
        product.Images.Should().Contain(i => i.IsPrimary);
    }
}

