using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Application.Commands;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;

namespace Storefront.UnitTests.Identity.Application;

public class RecordAccountTransactionCommandHandlerTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly RecordAccountTransactionCommandHandler _handler;

    public RecordAccountTransactionCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _handler = new RecordAccountTransactionCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_OrderDebit_Should_Increase_Balance()
    {
        // Arrange
        var company = CreateCompany(initialBalance: 0m);
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        var command = new RecordAccountTransactionCommand(
            company.Id, TransactionType.OrderDebit, 5000m, null, "ORD-001", null, "admin");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();

        var updated = await _context.PartnerCompanies.FindAsync(company.Id);
        updated!.CurrentBalance.Should().Be(5000m);
    }

    [Fact]
    public async Task Handle_PaymentCredit_Should_Decrease_Balance()
    {
        // Arrange
        var company = CreateCompany(initialBalance: 8000m);
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        var command = new RecordAccountTransactionCommand(
            company.Id, TransactionType.PaymentCredit, 3000m, PaymentMethod.BankTransfer, null, null, "admin");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updated = await _context.PartnerCompanies.FindAsync(company.Id);
        updated!.CurrentBalance.Should().Be(5000m);
    }

    [Fact]
    public async Task Handle_ManualAdjustment_Should_Increase_Balance()
    {
        // Arrange
        var company = CreateCompany(initialBalance: 1000m);
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        var command = new RecordAccountTransactionCommand(
            company.Id, TransactionType.ManualAdjustment, 500m, null, null, "Manual correction", "admin");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.PartnerCompanies.FindAsync(company.Id);
        updated!.CurrentBalance.Should().Be(1500m);
    }

    [Fact]
    public async Task Handle_Should_Persist_Transaction_Record()
    {
        // Arrange
        var company = CreateCompany();
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        var command = new RecordAccountTransactionCommand(
            company.Id, TransactionType.OrderDebit, 2500m, null, "ORD-TEST-001", "Test notes", "admin-user");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var transaction = await _context.PartnerAccountTransactions
            .FirstOrDefaultAsync(t => t.Id == result.Value);

        transaction.Should().NotBeNull();
        transaction!.PartnerCompanyId.Should().Be(company.Id);
        transaction.Type.Should().Be(TransactionType.OrderDebit);
        transaction.Amount.Should().Be(2500m);
        transaction.OrderReference.Should().Be("ORD-TEST-001");
        transaction.Notes.Should().Be("Test notes");
        transaction.CreatedBy.Should().Be("admin-user");
    }

    [Fact]
    public async Task Handle_WithPaymentMethod_Should_Persist_PaymentMethod()
    {
        // Arrange
        var company = CreateCompany(initialBalance: 5000m);
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        var command = new RecordAccountTransactionCommand(
            company.Id, TransactionType.PaymentCredit, 1000m, PaymentMethod.Check, null, null, "admin");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var transaction = await _context.PartnerAccountTransactions
            .FirstOrDefaultAsync(t => t.Id == result.Value);

        transaction!.PaymentMethod.Should().Be(PaymentMethod.Check);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Handle_WithNonPositiveAmount_Should_Return_ValidationError(decimal amount)
    {
        // Arrange
        var company = CreateCompany();
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        var command = new RecordAccountTransactionCommand(
            company.Id, TransactionType.OrderDebit, amount, null, null, null, "admin");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be("Validation");
        result.Error.Code.Should().Be("Transaction.InvalidAmount");
    }

    [Fact]
    public async Task Handle_WithNonExistentCompany_Should_Return_NotFound()
    {
        // Act
        var result = await _handler.Handle(
            new RecordAccountTransactionCommand(
                Guid.NewGuid().ToString(), TransactionType.OrderDebit, 100m, null, null, null, "admin"),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be("NotFound");
        result.Error.Code.Should().Be("Partner.NotFound");
    }

    [Fact]
    public async Task Handle_MultipleTransactions_Should_Accumulate_Balance_Correctly()
    {
        // Arrange
        var company = CreateCompany(initialBalance: 0m);
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        // Act — debit 3000, debit 2000, credit 1000
        await _handler.Handle(
            new RecordAccountTransactionCommand(company.Id, TransactionType.OrderDebit, 3000m, null, null, null, "admin"),
            CancellationToken.None);
        await _handler.Handle(
            new RecordAccountTransactionCommand(company.Id, TransactionType.OrderDebit, 2000m, null, null, null, "admin"),
            CancellationToken.None);
        await _handler.Handle(
            new RecordAccountTransactionCommand(company.Id, TransactionType.PaymentCredit, 1000m, PaymentMethod.Cash, null, null, "admin"),
            CancellationToken.None);

        // Assert: 0 + 3000 + 2000 - 1000 = 4000
        var updated = await _context.PartnerCompanies.FindAsync(company.Id);
        updated!.CurrentBalance.Should().Be(4000m);

        var txCount = await _context.PartnerAccountTransactions
            .CountAsync(t => t.PartnerCompanyId == company.Id);
        txCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_Transaction_Should_Have_Non_Empty_Id()
    {
        // Arrange
        var company = CreateCompany();
        _context.PartnerCompanies.Add(company);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(
            new RecordAccountTransactionCommand(company.Id, TransactionType.OrderDebit, 100m, null, null, null, "admin"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        Guid.TryParse(result.Value, out _).Should().BeTrue();
    }

    public void Dispose() => _context.Dispose();

    private static PartnerCompany CreateCompany(decimal initialBalance = 0m) => new()
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
        DiscountRate = 0m,
        CurrentBalance = initialBalance,
        CreatedAt = DateTime.UtcNow
    };
}
