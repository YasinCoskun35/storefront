using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Application.Commands;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;

namespace Storefront.UnitTests.Identity.Application;

public class UpdatePartnerPricingCommandHandlerTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly UpdatePartnerPricingCommandHandler _handler;

    public UpdatePartnerPricingCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _handler = new UpdatePartnerPricingCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Update_DiscountRate_On_Company()
    {
        // Arrange
        var company = CreateCompany();
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        var command = new UpdatePartnerPricingCommand(company.Id, 15m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.PartnerCompanies.FindAsync(company.Id);
        updated!.DiscountRate.Should().Be(15m);
    }

    [Fact]
    public async Task Handle_WithZeroRate_Should_Succeed()
    {
        // Arrange
        var company = CreateCompany(initialRate: 20m);
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new UpdatePartnerPricingCommand(company.Id, 0m), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.PartnerCompanies.FindAsync(company.Id);
        updated!.DiscountRate.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_WithMaxRate100_Should_Succeed()
    {
        // Arrange
        var company = CreateCompany();
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new UpdatePartnerPricingCommand(company.Id, 100m), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.PartnerCompanies.FindAsync(company.Id);
        updated!.DiscountRate.Should().Be(100m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-10)]
    [InlineData(-100)]
    public async Task Handle_WithNegativeRate_Should_Return_ValidationError(double rate)
    {
        // Arrange
        var company = CreateCompany();
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new UpdatePartnerPricingCommand(company.Id, (decimal)rate), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be("Validation");
        result.Error.Code.Should().Be("Partner.InvalidDiscountRate");
    }

    [Theory]
    [InlineData(100.01)]
    [InlineData(150)]
    [InlineData(200)]
    public async Task Handle_WithRateAbove100_Should_Return_ValidationError(double rate)
    {
        // Arrange
        var company = CreateCompany();
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new UpdatePartnerPricingCommand(company.Id, (decimal)rate), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be("Validation");
    }

    [Fact]
    public async Task Handle_WithNonExistentCompany_Should_Return_NotFoundError()
    {
        // Act
        var result = await _handler.Handle(
            new UpdatePartnerPricingCommand(Guid.NewGuid().ToString(), 10m),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be("NotFound");
        result.Error.Code.Should().Be("Partner.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Update_UpdatedAt_Timestamp()
    {
        // Arrange
        var company = CreateCompany();
        var beforeUpdate = DateTime.UtcNow.AddSeconds(-1);
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        // Act
        await _handler.Handle(new UpdatePartnerPricingCommand(company.Id, 30m), CancellationToken.None);

        // Assert
        var updated = await _context.PartnerCompanies.FindAsync(company.Id);
        updated!.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    public void Dispose() => _context.Dispose();

    private static PartnerCompany CreateCompany(decimal initialRate = 0m) => new()
    {
        Id = Guid.NewGuid().ToString(),
        CompanyName = "Test Company",
        TaxId = $"TAX-{Guid.NewGuid():N}",
        Email = $"test-{Guid.NewGuid():N}@test.com",
        Phone = "555-0001",
        Address = "1 Test St",
        City = "Istanbul",
        Country = "Turkey",
        Status = PartnerStatus.Active,
        DiscountRate = initialRate,
        CurrentBalance = 0m,
        CreatedAt = DateTime.UtcNow
    };
}
