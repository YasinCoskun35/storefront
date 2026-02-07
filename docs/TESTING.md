# Storefront Testing Guide

## Overview

This document provides a comprehensive guide to testing the Storefront application, including architecture tests, unit tests, and integration tests.

## Test Projects

The testing strategy is organized into three distinct test projects:

### 1. **Storefront.ArchitectureTests**
- **Purpose**: Enforce architectural rules and design principles using NetArchTest
- **Technologies**: xUnit, NetArchTest.Rules, FluentAssertions
- **Location**: `tests/Storefront.ArchitectureTests/`

### 2. **Storefront.UnitTests**
- **Purpose**: Test individual components, domain logic, and services in isolation
- **Technologies**: xUnit, NSubstitute (mocking), Bogus (test data), FluentAssertions
- **Location**: `tests/Storefront.UnitTests/`

### 3. **Storefront.IntegrationTests**
- **Purpose**: Test the API endpoints with real database and application infrastructure
- **Technologies**: xUnit, Microsoft.AspNetCore.Mvc.Testing, Testcontainers.PostgreSql
- **Location**: `tests/Storefront.IntegrationTests/`

---

## Quick Start

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
# Architecture tests
dotnet test tests/Storefront.ArchitectureTests/Storefront.ArchitectureTests.csproj

# Unit tests
dotnet test tests/Storefront.UnitTests/Storefront.UnitTests.csproj

# Integration tests
dotnet test tests/Storefront.IntegrationTests/Storefront.IntegrationTests.csproj
```

### Run Tests with Code Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Tests in Watch Mode (for TDD)
```bash
dotnet watch test --project tests/Storefront.UnitTests/Storefront.UnitTests.csproj
```

---

## Architecture Tests

Architecture tests automatically enforce design principles and catch violations at compile time.

### Test Coverage

1. **Dependency Rules**
   - Domain should not depend on Infrastructure or Application
   - Application should not depend on Infrastructure
   - Modules should not reference each other

2. **CQRS Patterns**
   - Commands must implement `IRequest<Result<T>>`
   - Queries must implement `IRequest<Result<T>>`
   - Handlers must follow naming conventions

3. **Controller Rules**
   - Controllers should not use DbContext directly
   - Controllers must use MediatR for business logic

4. **Naming Conventions**
   - Command handlers must end with "CommandHandler"
   - Query handlers must end with "QueryHandler"
   - Validators must end with "Validator"

5. **Layer Placement**
   - DbContexts must be in Infrastructure.Persistence namespace
   - Controllers must be in API.Controllers namespace

### Example Architecture Test

```csharp
[Fact]
public void Domain_Should_Not_Depend_On_Infrastructure()
{
    var result = Types.InAssemblies(new[] { CatalogAssembly, ContentAssembly })
        .That()
        .ResideInNamespace("*.Core.Domain.*")
        .ShouldNot()
        .HaveDependencyOn("*.Infrastructure.*")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

---

## Unit Tests

Unit tests focus on testing individual components in isolation with mocked dependencies.

### Test Organization

```
tests/Storefront.UnitTests/
├── SharedKernel/
│   └── ResultTests.cs                  # Test Result<T> pattern
├── Catalog/
│   └── Domain/
│       └── ProductTests.cs             # Test Product entity
└── Content/
    └── Application/
        └── SlugServiceTests.cs         # Test slug generation
```

### Writing Unit Tests

#### Example 1: Testing Domain Entities

```csharp
[Fact]
public void Product_Can_Be_Created_With_Required_Properties()
{
    // Arrange & Act
    var product = new Product
    {
        Id = Guid.NewGuid().ToString(),
        Name = "Cordless Drill",
        SKU = "CD-001",
        Price = 99.99m,
        StockStatus = StockStatus.InStock
    };

    // Assert
    product.Name.Should().Be("Cordless Drill");
    product.Price.Should().BePositive();
}
```

#### Example 2: Testing Services with Mocks

```csharp
[Fact]
public async Task GenerateSlug_Should_Convert_Title_To_Lowercase_With_Dashes()
{
    // Arrange
    var context = CreateInMemoryContext();
    var slugService = new SlugService(context);

    // Act
    var slug = await slugService.GenerateUniqueSlugAsync("My New Drill Guide");

    // Assert
    slug.Should().Be("my-new-drill-guide");
}
```

### Best Practices for Unit Tests

1. **Arrange-Act-Assert Pattern**: Always structure tests with clear AAA sections
2. **One Assert Per Test**: Focus on testing one behavior per test
3. **Descriptive Names**: Use `Method_Scenario_ExpectedResult` naming convention
4. **Isolated Tests**: Tests should not depend on each other
5. **Fast Execution**: Unit tests should run in milliseconds

---

## Integration Tests

Integration tests verify the entire application stack with a real database.

### Test Infrastructure

#### CustomWebApplicationFactory

The `CustomWebApplicationFactory` sets up the test environment:

- Spins up a PostgreSQL container using Testcontainers
- Replaces production DbContexts with test database connections
- Mocks file system operations (e.g., `IImageUploadService`)
- Applies migrations automatically

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public CustomWebApplicationFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("storefront_test")
            .Build();
    }

    // ... configuration
}
```

### Writing Integration Tests

#### Example: Testing API Endpoints

```csharp
public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_WithAuth_Should_Return_Success()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        var productDto = new
        {
            Name = "Cordless Drill Pro",
            SKU = "CDP-001",
            Price = 199.99m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/catalog/products", productDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Authentication in Integration Tests

To test protected endpoints:

```csharp
private async Task<string> GetAuthTokenAsync()
{
    var loginDto = new
    {
        Email = "admin@storefront.com",
        Password = "AdminPassword123!"
    };

    var response = await _client.PostAsJsonAsync("/api/identity/auth/login", loginDto);
    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
    return loginResponse.AccessToken;
}
```

### Test Database Management

- **Isolation**: Each test class gets a fresh database container
- **Cleanup**: Containers are automatically disposed after tests complete
- **Parallel Execution**: Tests can run in parallel without conflicts

---

## Test Data Generation

Use **Bogus** for generating realistic test data:

```csharp
var faker = new Faker<Product>()
    .RuleFor(p => p.Id, f => Guid.NewGuid().ToString())
    .RuleFor(p => p.Name, f => f.Commerce.ProductName())
    .RuleFor(p => p.SKU, f => f.Commerce.Ean13())
    .RuleFor(p => p.Price, f => f.Finance.Amount(10, 1000))
    .RuleFor(p => p.Description, f => f.Commerce.ProductDescription());

var products = faker.Generate(50);
```

---

## Continuous Integration

### Running Tests in CI/CD

#### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Run tests
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
```

---

## Code Coverage

### Generate Coverage Report

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Install ReportGenerator (one-time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"tests/**/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:Html

# Open the report
open coverage-report/index.html
```

### Coverage Goals

- **Minimum Overall**: 70%
- **Domain Layer**: 90%+
- **Application Layer**: 80%+
- **API Controllers**: 60%+ (since they're thin wrappers)

---

## Troubleshooting

### Docker Permission Issues (Integration Tests)

If you encounter Docker socket permission errors:

```bash
# Add your user to the docker group
sudo usermod -aG docker $USER

# Log out and back in for changes to take effect
```

### Testcontainers Timeout

If tests fail due to container startup timeout:

```csharp
_dbContainer = new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")
    .WithStartupTimeout(TimeSpan.FromMinutes(5))  // Increase timeout
    .Build();
```

### Test Database Not Cleaning Up

Ensure you're implementing `IAsyncLifetime` correctly:

```csharp
public class MyTests : IClassFixture<CustomWebApplicationFactory>
{
    // Tests run here
}
```

---

## Best Practices Summary

### DO:
✅ Write tests before or alongside code (TDD)  
✅ Use descriptive test names  
✅ Keep tests fast and isolated  
✅ Mock external dependencies (file system, email, etc.)  
✅ Test edge cases and error conditions  
✅ Use FluentAssertions for readable assertions  

### DON'T:
❌ Write tests that depend on external services  
❌ Use production database for tests  
❌ Test implementation details (test behavior)  
❌ Share state between tests  
❌ Ignore failing tests  

---

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [NetArchTest Documentation](https://github.com/BenMorris/NetArchTest)
- [Testcontainers Documentation](https://dotnet.testcontainers.org/)

---

## Test Metrics

Track these metrics for your test suite:

| Metric | Target | Current |
|--------|--------|---------|
| Total Tests | - | TBD |
| Architecture Tests | 10+ | 11 |
| Unit Tests | 50+ | 15 |
| Integration Tests | 20+ | 8 |
| Code Coverage | 70%+ | TBD |
| Execution Time | < 2 min | TBD |

---

## Contributing

When adding new features:

1. Write architecture tests if introducing new patterns
2. Write unit tests for all business logic
3. Write integration tests for all API endpoints
4. Ensure all tests pass before creating a pull request
5. Maintain or improve code coverage

---

## Questions?

If you have questions about testing:
1. Check this documentation
2. Review existing tests for examples
3. Consult the team lead

**Remember**: Good tests are an investment in code quality and maintainability!

