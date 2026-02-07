# Price Configuration Feature

## Overview

Allow toggling price display on/off via configuration, perfect for:
- **B2B businesses** where prices are negotiated
- **Quote-based sales** ("Contact for price")
- **Showroom catalogs** (browse only, no pricing)

---

## Configuration Setup

### 1. Add to `appsettings.json`

```json
{
  "CatalogSettings": {
    "PricingEnabled": false,  // Set to false to hide all prices
    "RequirePriceForProducts": false,  // Allow creating products without prices
    "ShowPriceLabel": "Contact for Quote",  // What to show instead of price
    "AllowPriceInquiry": true  // Show "Request Quote" button
  }
}
```

### 2. Create Configuration Class

**File**: `src/Modules/Catalog/Core/Application/Settings/CatalogSettings.cs`

```csharp
namespace Storefront.Modules.Catalog.Core.Application.Settings;

public class CatalogSettings
{
    public const string SectionName = "CatalogSettings";
    
    /// <summary>
    /// Global toggle for pricing feature
    /// </summary>
    public bool PricingEnabled { get; set; } = true;
    
    /// <summary>
    /// Require price when creating/updating products
    /// </summary>
    public bool RequirePriceForProducts { get; set; } = true;
    
    /// <summary>
    /// Text to display when price is hidden (e.g., "Contact for Quote")
    /// </summary>
    public string ShowPriceLabel { get; set; } = "Contact for Quote";
    
    /// <summary>
    /// Show "Request Quote" button when prices are hidden
    /// </summary>
    public bool AllowPriceInquiry { get; set; } = true;
}
```

---

## Database Changes

### Price Fields Remain BUT Become Optional

**Product Entity** (already nullable):
```csharp
public decimal Price { get; set; }  // Keep as-is, but make validation conditional
public decimal? CompareAtPrice { get; set; }  // Already nullable
public decimal? Cost { get; set; }  // Already nullable
public decimal? BundlePrice { get; set; }  // New, already nullable
```

**No database changes needed!** Just update validation logic.

---

## Validation Updates

### Before (Price Always Required):
```csharp
RuleFor(x => x.Price)
    .GreaterThan(0)
    .WithMessage("Price must be greater than 0");
```

### After (Conditional Validation):
```csharp
RuleFor(x => x.Price)
    .GreaterThan(0)
    .When(x => _catalogSettings.RequirePriceForProducts)
    .WithMessage("Price must be greater than 0");
```

---

## API Response Changes

### When PricingEnabled = true (Current Behavior):
```json
GET /api/catalog/products/123

{
  "id": "123",
  "name": "Modern 3-Seater Sofa",
  "price": 1299.99,
  "compareAtPrice": 1499.99,
  "currency": "USD"
}
```

### When PricingEnabled = false (New Behavior):
```json
GET /api/catalog/products/123

{
  "id": "123",
  "name": "Modern 3-Seater Sofa",
  "price": null,  // or omit entirely
  "priceLabel": "Contact for Quote",
  "allowPriceInquiry": true
}
```

---

## Frontend/Mobile App Handling

### When Prices Are Enabled:
```
Modern 3-Seater Sofa
$1,299.99
[Add to Cart]
```

### When Prices Are Disabled:
```
Modern 3-Seater Sofa
Contact for Quote
[Request Quote]
```

---

## Implementation Files

### 1. Configuration Registration

**File**: `CatalogModuleExtensions.cs`

```csharp
// Register settings
services.Configure<CatalogSettings>(
    configuration.GetSection(CatalogSettings.SectionName));

services.AddSingleton(sp => 
    sp.GetRequiredService<IOptions<CatalogSettings>>().Value);
```

### 2. Update Validators

**File**: `CreateProductCommandValidator.cs`

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private readonly CatalogSettings _settings;
    
    public CreateProductCommandValidator(CatalogSettings settings)
    {
        _settings = settings;
        
        RuleFor(x => x.Name).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(100);
        
        // Conditional price validation
        When(x => _settings.RequirePriceForProducts, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price is required when pricing is enabled");
        });
    }
}
```

### 3. Update DTOs

**File**: `ProductDto.cs`

```csharp
public class ProductDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string SKU { get; set; }
    
    // Price fields (may be null when pricing disabled)
    public decimal? Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    
    // New: Indicate if pricing is enabled
    public bool PricingEnabled { get; set; }
    public string? PriceLabel { get; set; }  // "Contact for Quote"
}
```

### 4. Update Query Handlers

**File**: `GetProductDetailsQueryHandler.cs`

```csharp
public async Task<Result<ProductDto>> Handle(...)
{
    var product = await _context.Products.FindAsync(id);
    
    return new ProductDto
    {
        Id = product.Id,
        Name = product.Name,
        Price = _settings.PricingEnabled ? product.Price : null,
        CompareAtPrice = _settings.PricingEnabled ? product.CompareAtPrice : null,
        PricingEnabled = _settings.PricingEnabled,
        PriceLabel = _settings.PricingEnabled ? null : _settings.ShowPriceLabel
    };
}
```

---

## Configuration Scenarios

### Scenario 1: Public E-commerce (Default)
```json
{
  "CatalogSettings": {
    "PricingEnabled": true,
    "RequirePriceForProducts": true,
    "ShowPriceLabel": null,
    "AllowPriceInquiry": false
  }
}
```
**Result**: Normal store with prices, "Add to Cart"

---

### Scenario 2: B2B Catalog (No Prices)
```json
{
  "CatalogSettings": {
    "PricingEnabled": false,
    "RequirePriceForProducts": false,
    "ShowPriceLabel": "Contact for Quote",
    "AllowPriceInquiry": true
  }
}
```
**Result**: Showroom catalog, no prices shown, "Request Quote" button

---

### Scenario 3: Hybrid (Internal Prices, Hidden from Public)
```json
{
  "CatalogSettings": {
    "PricingEnabled": false,  // Hide from customers
    "RequirePriceForProducts": true,  // But admins must enter prices
    "ShowPriceLabel": "Contact for Quote",
    "AllowPriceInquiry": true
  }
}
```
**Result**: Prices stored but not shown to customers, visible in admin panel

---

## Admin Panel Behavior

**Always show prices in admin panel**, regardless of public setting:

```typescript
// Admin product form - always shows price fields
{
  "price": 1299.99,  // ✅ Always editable
  "visibility": {
    "showPriceToCustomers": false  // Public setting
  }
}
```

---

## Benefits

✅ **Flexibility**: Toggle pricing without code changes
✅ **Data Preservation**: Prices stored even when hidden (for future use)
✅ **B2B Ready**: Perfect for quote-based sales models
✅ **Easy Migration**: Turn pricing on/off as business model changes
✅ **Admin Control**: Prices always visible to staff

---

## Migration Path

### Phase 1: No Prices
```json
"PricingEnabled": false
```
- Products created without prices
- Mobile app shows "Contact for Quote"

### Phase 2: Add Pricing Later
```json
"PricingEnabled": true
```
- Bulk import prices via admin
- Enable e-commerce features
- Add payment gateway

---

## Implementation Order

1. ✅ Create `CatalogSettings` class
2. ✅ Add configuration to `appsettings.json`
3. ✅ Register settings in `CatalogModuleExtensions`
4. ✅ Update validators (make price conditional)
5. ✅ Update DTOs (add `PricingEnabled`, `PriceLabel`)
6. ✅ Update query handlers (respect settings)
7. ✅ **THEN** implement bundle products

---

**Ready to implement this + bundle products together?** 

I'll make sure:
- ✅ Prices are optional (configurable)
- ✅ Bundle products work with/without pricing
- ✅ Everything is ready for your B2B mobile app

**Confirm and I'll start coding!** 🚀
