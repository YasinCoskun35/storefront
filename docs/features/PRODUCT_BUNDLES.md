# Product Bundle Architecture Design

## Overview

Support for both simple and complex product structures:
- **Simple Product**: A single item (e.g., "3-Seater Sofa")
- **Bundle Product**: A set containing multiple products (e.g., "Living Room Set" = 3-Seater + Bergere + Coffee Table)

---

## Database Schema Changes

### 1. Add ProductType Enum

**File**: `src/Modules/Catalog/Core/Domain/Enums/ProductType.cs`

```csharp
namespace Storefront.Modules.Catalog.Core.Domain.Enums;

public enum ProductType
{
    Simple = 0,      // Single standalone product
    Bundle = 1,      // Set/Kit containing multiple products
    Configurable = 2 // Future: Product with variants (fabric/color options)
}
```

---

### 2. Update Product Entity

**Add to Product.cs**:

```csharp
// Product Type
public ProductType ProductType { get; set; } = ProductType.Simple;

// Bundle-specific properties
public bool CanBeSoldSeparately { get; set; } = true;  // Can components be sold individually?
public decimal? BundlePrice { get; set; }  // Optional: Override price (vs sum of components)

// Navigation properties
public virtual ICollection<ProductBundleItem> BundleItems { get; set; } = new List<ProductBundleItem>();
public virtual ICollection<ProductBundleItem> UsedInBundles { get; set; } = new List<ProductBundleItem>();
```

---

### 3. Create ProductBundleItem Entity

**File**: `src/Modules/Catalog/Core/Domain/Entities/ProductBundleItem.cs`

```csharp
namespace Storefront.Modules.Catalog.Core.Domain.Entities;

public class ProductBundleItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // The bundle/set product
    public string BundleProductId { get; set; }
    public virtual Product BundleProduct { get; set; } = null!;
    
    // The component product (what's included in the bundle)
    public string ComponentProductId { get; set; }
    public virtual Product ComponentProduct { get; set; } = null!;
    
    // How many of this component in the bundle?
    public int Quantity { get; set; } = 1;
    
    // Optional: Override price for this component when in bundle
    public decimal? PriceOverride { get; set; }
    
    // Can the customer customize/remove this component?
    public bool IsOptional { get; set; } = false;
    
    // Display order in the bundle
    public int DisplayOrder { get; set; } = 0;
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## Example Data Model

### Example 1: Living Room Set (Bundle)

**Bundle Product**:
```json
{
  "id": "bundle-001",
  "name": "Modern Living Room Set",
  "sku": "SET-LR-001",
  "productType": "Bundle",
  "price": 2999.99,  // Bundle price (could be less than sum of parts)
  "bundlePrice": 2999.99,
  "canBeSoldSeparately": false,  // This set can't be broken up
  "description": "Complete living room solution"
}
```

**Bundle Items** (What's in the set):
```json
[
  {
    "bundleProductId": "bundle-001",
    "componentProductId": "prod-sofa-3seater",
    "quantity": 1,
    "displayOrder": 1
  },
  {
    "bundleProductId": "bundle-001",
    "componentProductId": "prod-bergere",
    "quantity": 2,
    "displayOrder": 2
  },
  {
    "bundleProductId": "bundle-001",
    "componentProductId": "prod-coffee-table",
    "quantity": 1,
    "displayOrder": 3
  }
]
```

**Component Products** (Simple):
```json
{
  "id": "prod-sofa-3seater",
  "name": "Modern 3-Seater Sofa",
  "sku": "SOFA-3S-001",
  "productType": "Simple",
  "price": 1299.99,
  "canBeSoldSeparately": true  // Can also be bought alone
}
```

---

## Pricing Strategies

### Option 1: Fixed Bundle Price
```json
{
  "productType": "Bundle",
  "price": 2999.99,  // Fixed price regardless of components
  "bundlePrice": 2999.99
}
```

### Option 2: Dynamic Pricing (Sum of Components)
```json
{
  "productType": "Bundle",
  "price": null,  // Calculate from components
  "bundlePrice": null
}
```
**Calculated**: 3-Seater ($1,299) + 2x Bergere ($599 each) + Coffee Table ($399) = $2,896

### Option 3: Discounted Bundle
```json
{
  "productType": "Bundle",
  "price": 2599.99,  // Discounted from $2,896
  "bundlePrice": 2599.99
}
```

---

## Inventory Management

### Strategy 1: Independent Stock
```json
{
  "productType": "Bundle",
  "stockQuantity": 5,  // 5 complete sets available
  "trackComponentStock": false
}
```

### Strategy 2: Component-Based Stock
```json
{
  "productType": "Bundle",
  "stockQuantity": 0,  // Calculated dynamically
  "trackComponentStock": true
}
```
**Available Sets** = MIN(sofa_stock / 1, bergere_stock / 2, table_stock / 1)

---

## API Examples

### Create a Simple Product
```json
POST /api/catalog/products
{
  "name": "Modern 3-Seater Sofa",
  "sku": "SOFA-3S-001",
  "productType": "Simple",
  "price": 1299.99,
  "stockQuantity": 10,
  "canBeSoldSeparately": true
}
```

### Create a Bundle Product
```json
POST /api/catalog/products
{
  "name": "Living Room Set",
  "sku": "SET-LR-001",
  "productType": "Bundle",
  "price": 2999.99,
  "stockQuantity": 5,
  "canBeSoldSeparately": false,
  "bundleItems": [
    {
      "componentProductId": "prod-sofa-3seater",
      "quantity": 1,
      "displayOrder": 1
    },
    {
      "componentProductId": "prod-bergere",
      "quantity": 2,
      "displayOrder": 2
    }
  ]
}
```

### Get Bundle with Components
```json
GET /api/catalog/products/bundle-001?includeComponents=true

Response:
{
  "id": "bundle-001",
  "name": "Living Room Set",
  "productType": "Bundle",
  "price": 2999.99,
  "components": [
    {
      "componentId": "prod-sofa-3seater",
      "name": "Modern 3-Seater Sofa",
      "quantity": 1,
      "price": 1299.99,
      "displayOrder": 1
    },
    {
      "componentId": "prod-bergere",
      "name": "Bergere Chair",
      "quantity": 2,
      "price": 599.99,
      "displayOrder": 2
    }
  ],
  "totalComponentPrice": 2498.98,
  "bundleDiscount": -500.99
}
```

---

## Implementation Plan

### Phase 1: Database Changes
1. ✅ Add `ProductType` enum
2. ✅ Update `Product` entity with bundle fields
3. ✅ Create `ProductBundleItem` entity
4. ✅ Update `CatalogDbContext` with configuration
5. ✅ Update database initialization to create `ProductBundleItems` table

### Phase 2: Domain Logic
1. ✅ Create `CreateBundleProductCommand`
2. ✅ Create `AddComponentToBundleCommand`
3. ✅ Create `GetBundleDetailsQuery`
4. ✅ Update `GetProductDetailsQuery` to include components
5. ✅ Add price calculation logic (bundle vs sum)
6. ✅ Add stock calculation logic (if component-based)

### Phase 3: API Endpoints
1. ✅ `POST /api/catalog/products` - Support productType
2. ✅ `POST /api/catalog/products/{id}/components` - Add component to bundle
3. ✅ `GET /api/catalog/products/{id}/components` - Get bundle contents
4. ✅ `DELETE /api/catalog/products/{id}/components/{componentId}` - Remove component

### Phase 4: Frontend (Mobile App)
1. ✅ Display bundle products with "Set" badge
2. ✅ Show "What's Included" section for bundles
3. ✅ Display savings vs buying separately
4. ✅ Allow expanding to see all components

---

## Benefits of This Design

✅ **Flexible**: Supports simple products, bundles, and future variants
✅ **Reusable**: Same product can be sold alone AND in bundles
✅ **Scalable**: Easy to add new bundle types (e.g., "Build Your Set")
✅ **SEO-Friendly**: Each bundle and component has its own page/slug
✅ **Pricing Control**: Fixed, dynamic, or discounted pricing
✅ **Inventory Aware**: Track bundle stock independently or calculate from components

---

## Database Migration Preview

```sql
-- Add ProductType column
ALTER TABLE catalog."Products" 
ADD COLUMN "ProductType" varchar(50) NOT NULL DEFAULT 'Simple',
ADD COLUMN "BundlePrice" numeric(18,2),
ADD COLUMN "CanBeSoldSeparately" boolean NOT NULL DEFAULT true;

-- Create ProductBundleItems table
CREATE TABLE catalog."ProductBundleItems" (
    "Id" varchar(450) PRIMARY KEY,
    "BundleProductId" varchar(450) NOT NULL,
    "ComponentProductId" varchar(450) NOT NULL,
    "Quantity" int NOT NULL DEFAULT 1,
    "PriceOverride" numeric(18,2),
    "IsOptional" boolean NOT NULL DEFAULT false,
    "DisplayOrder" int NOT NULL DEFAULT 0,
    "CreatedAt" timestamp NOT NULL DEFAULT now(),
    CONSTRAINT "FK_BundleItems_Bundle" FOREIGN KEY ("BundleProductId") 
        REFERENCES catalog."Products"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_BundleItems_Component" FOREIGN KEY ("ComponentProductId") 
        REFERENCES catalog."Products"("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_BundleItems_BundleProductId" ON catalog."ProductBundleItems" ("BundleProductId");
CREATE INDEX "IX_BundleItems_ComponentProductId" ON catalog."ProductBundleItems" ("ComponentProductId");
```

---

## Next Steps

1. **Review this design** - Any changes needed?
2. **I'll implement** all the code changes
3. **Test in Swagger** - Create simple products, then create bundles
4. **Mobile UI** - Design how bundles look in your app

**Ready to implement? Let me know if you want any changes to this design!** 🚀
