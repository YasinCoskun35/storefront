# Contributing to Storefront

Thank you for considering contributing to Storefront! This document provides guidelines and instructions for contributing.

---

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on what is best for the project
- Show empathy towards other contributors

---

## Getting Started

### Prerequisites

1. Install prerequisites (see [Installation Guide](docs/INSTALLATION.md))
2. Fork the repository
3. Clone your fork: `git clone <your-fork-url>`
4. Create a feature branch: `git checkout -b feature/amazing-feature`

### Development Setup

```bash
# Start database
docker-compose up -d

# Restore packages
dotnet restore

# Run tests
dotnet test

# Start API
cd src/API/Storefront.Api
dotnet run

# Start admin panel (optional)
cd web
npm install
npm run dev
```

---

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/my-feature
# or
git checkout -b fix/my-bugfix
```

**Branch naming:**
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation updates
- `refactor/` - Code refactoring
- `test/` - Adding tests

### 2. Make Changes

Follow the project structure and patterns:
- Use CQRS pattern (Commands & Queries)
- Follow Result pattern for error handling
- Add FluentValidation for commands
- Write unit tests for business logic
- Add integration tests for API endpoints

### 3. Write Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Storefront.UnitTests
dotnet test tests/Storefront.ArchitectureTests
```

**Required:**
- Unit tests for business logic
- Architecture tests for new modules
- Integration tests for new endpoints

### 4. Commit Changes

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```bash
git commit -m "feat(catalog): add bundle product support"
git commit -m "fix(auth): resolve token refresh issue"
git commit -m "docs: update API reference"
```

**Format:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation
- `style` - Code style (formatting)
- `refactor` - Code refactoring
- `test` - Adding tests
- `chore` - Maintenance tasks

### 5. Push and Create PR

```bash
git push origin feature/my-feature
```

Then create a Pull Request on GitHub with:
- Clear title and description
- Reference any related issues
- Screenshots (if UI changes)
- Test results

---

## Coding Standards

### C# / .NET Backend

#### Naming Conventions

- **Classes/Interfaces**: PascalCase (`ProductService`, `IProductRepository`)
- **Methods**: PascalCase (`GetProductById`, `CreateProduct`)
- **Variables**: camelCase (`productId`, `isActive`)
- **Constants**: PascalCase (`MaxFileSize`, `DefaultPageSize`)
- **Private fields**: `_camelCase` (`_dbContext`, `_logger`)

#### Code Style

```csharp
// ✅ Good
public class ProductService
{
    private readonly CatalogDbContext _context;
    
    public async Task<Result<Product>> GetProductAsync(string productId)
    {
        var product = await _context.Products.FindAsync(productId);
        
        if (product is null)
            return Error.NotFound("Product.NotFound", "Product not found");
            
        return Result<Product>.Success(product);
    }
}

// ❌ Bad - No Result pattern
public class ProductService
{
    public Product GetProduct(string productId)
    {
        var product = _context.Products.Find(productId);
        
        if (product == null)
            throw new NotFoundException("Product not found");
            
        return product;
    }
}
```

#### CQRS Pattern

```csharp
// Commands (write operations)
public record CreateProductCommand(
    string Name,
    string SKU,
    decimal Price
) : IRequest<Result<string>>;

// Queries (read operations)
public record GetProductQuery(
    string ProductId
) : IRequest<Result<ProductDto>>;
```

#### Validation

Use FluentValidation for all commands:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(500);
            
        RuleFor(x => x.SKU)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}
```

### TypeScript / Next.js Frontend

#### Naming Conventions

- **Components**: PascalCase (`ProductCard`, `CategoryList`)
- **Files**: kebab-case (`product-card.tsx`, `category-list.tsx`)
- **Hooks**: camelCase with `use` prefix (`useProducts`, `useAuth`)
- **Types/Interfaces**: PascalCase (`Product`, `Category`)

#### Code Style

```typescript
// ✅ Good - Typed, explicit returns
interface ProductCardProps {
  product: Product;
  onSelect: (id: string) => void;
}

export function ProductCard({ product, onSelect }: ProductCardProps) {
  return (
    <div className="rounded-lg border p-4">
      <h3>{product.name}</h3>
      <button onClick={() => onSelect(product.id)}>
        View Details
      </button>
    </div>
  );
}

// ❌ Bad - No types
export function ProductCard({ product, onSelect }) {
  return <div>...</div>;
}
```

#### API Calls

Use TanStack Query:

```typescript
// ✅ Good
export function useProducts(searchTerm: string) {
  return useQuery({
    queryKey: ['products', searchTerm],
    queryFn: () => api.getProducts({ searchTerm }),
  });
}

// ❌ Bad - Direct fetch in component
useEffect(() => {
  fetch('/api/products').then(res => res.json()).then(setProducts);
}, []);
```

---

## Project Structure

### Adding a New Module

1. Create module structure:
   ```
   src/Modules/NewModule/
   ├── Storefront.Modules.NewModule/
   │   ├── Core/
   │   │   ├── Domain/
   │   │   │   ├── Entities/
   │   │   │   └── Enums/
   │   │   └── Application/
   │   │       ├── Commands/
   │   │       ├── Queries/
   │   │       └── DTOs/
   │   ├── Infrastructure/
   │   │   ├── Persistence/
   │   │   └── Services/
   │   └── API/
   │       └── Controllers/
   ```

2. Create `DbContext` with schema:
   ```csharp
   protected override void OnModelCreating(ModelBuilder builder)
   {
       builder.HasDefaultSchema("newmodule");
   }
   ```

3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddNewModule(configuration);
   ```

4. Add architecture tests:
   ```csharp
   [Fact]
   public void NewModule_Should_Not_DependOn_OtherModules()
   {
       var result = Types.InAssembly(NewModuleAssembly)
           .Should().NotHaveDependencyOn("Storefront.Modules.*")
           .GetResult();
           
       result.IsSuccessful.Should().BeTrue();
   }
   ```

### Adding a New Endpoint

1. **Create Command/Query:**
   ```csharp
   // Commands/CreateThingCommand.cs
   public record CreateThingCommand(...) : IRequest<Result<string>>;
   ```

2. **Create Validator:**
   ```csharp
   // Commands/CreateThingCommandValidator.cs
   public class CreateThingCommandValidator : AbstractValidator<CreateThingCommand> { }
   ```

3. **Create Handler:**
   ```csharp
   // Commands/CreateThingCommandHandler.cs
   public class CreateThingCommandHandler : IRequestHandler<CreateThingCommand, Result<string>> { }
   ```

4. **Add Controller Endpoint:**
   ```csharp
   [HttpPost]
   public async Task<IActionResult> CreateThing([FromBody] CreateThingCommand command)
   {
       var result = await _mediator.Send(command);
       return result.IsSuccess 
           ? CreatedAtAction(nameof(GetThing), new { id = result.Value }, result.Value)
           : BadRequest(result.Error);
   }
   ```

5. **Add Tests:**
   ```csharp
   // Unit test
   [Fact]
   public async Task CreateThing_WithValidData_ReturnsSuccess() { }
   
   // Integration test
   [Fact]
   public async Task POST_Things_ReturnsCreated() { }
   ```

---

## Testing Guidelines

### Unit Tests

Test business logic in isolation:

```csharp
public class ProductTests
{
    [Fact]
    public void Product_WithNegativePrice_ShouldFail()
    {
        // Arrange
        var command = new CreateProductCommand { Price = -10 };
        var validator = new CreateProductCommandValidator();
        
        // Act
        var result = validator.Validate(command);
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
}
```

### Integration Tests

Test full API flow:

```csharp
public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task POST_Products_ReturnsCreated()
    {
        // Arrange
        var command = new CreateProductCommand { Name = "Test", SKU = "TST-001" };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/catalog/products", command);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### Architecture Tests

Enforce design rules:

```csharp
[Fact]
public void Domain_Should_Not_DependOn_Infrastructure()
{
    var result = Types.InAssembly(DomainAssembly)
        .Should().NotHaveDependencyOn("Storefront.*.Infrastructure")
        .GetResult();
        
    result.IsSuccessful.Should().BeTrue();
}
```

---

## Pull Request Guidelines

### PR Checklist

Before submitting:

- [ ] Code follows project conventions
- [ ] All tests pass (`dotnet test`)
- [ ] Architecture tests pass
- [ ] No linter errors
- [ ] Documentation updated (if needed)
- [ ] Commit messages follow Conventional Commits
- [ ] PR has clear title and description
- [ ] Referenced related issues

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed

## Screenshots (if applicable)

## Related Issues
Closes #123
```

---

## Code Review Process

### For Reviewers

- Check code quality and conventions
- Verify tests are comprehensive
- Ensure documentation is updated
- Test locally if significant changes
- Be constructive and respectful

### For Contributors

- Respond to feedback promptly
- Make requested changes
- Update tests if needed
- Rebase if conflicts arise

---

## Release Process

1. Bump version in `.csproj` files
2. Update `CHANGELOG.md`
3. Create release branch: `release/v1.2.0`
4. Run full test suite
5. Build Docker images
6. Tag release: `git tag v1.2.0`
7. Push tags: `git push --tags`
8. Create GitHub Release with notes

---

## Getting Help

- 📖 Read the [documentation](docs/)
- 🐛 Check [existing issues](../../issues)
- 💬 Start a [discussion](../../discussions)
- 📧 Contact maintainers

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

**Thank you for contributing! 🎉**
