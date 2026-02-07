# API Reference

Complete REST API documentation for Storefront.

**Base URL:** `http://localhost:8080` (development)  
**Format:** JSON  
**Authentication:** JWT Bearer tokens

---

## Authentication

### Login

Authenticate and receive JWT tokens.

```http
POST /api/identity/auth/login
```

**Request Body:**
```json
{
  "email": "admin@storefront.com",
  "password": "AdminPassword123!"
}
```

**Response:** `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4-e5f6-...",
  "expiresIn": 1800,
  "tokenType": "Bearer"
}
```

**Errors:**
- `400 Bad Request` - Invalid credentials
- `401 Unauthorized` - Account locked/inactive

---

### Refresh Token

Get a new access token using refresh token.

```http
POST /api/identity/auth/refresh
```

**Request Body:**
```json
{
  "refreshToken": "a1b2c3d4-e5f6-..."
}
```

**Response:** `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "new-refresh-token",
  "expiresIn": 1800
}
```

---

## Products

### List/Search Products

Get paginated list of products with optional filtering.

```http
GET /api/catalog/products?searchTerm=sofa&categoryId=cat-123&pageNumber=1&pageSize=20
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `searchTerm` | string | Fuzzy search on name, SKU, description |
| `categoryId` | string | Filter by category |
| `brandId` | string | Filter by brand |
| `minPrice` | decimal | Minimum price (if pricing enabled) |
| `maxPrice` | decimal | Maximum price (if pricing enabled) |
| `isActive` | boolean | Filter by active status |
| `pageNumber` | int | Page number (default: 1) |
| `pageSize` | int | Items per page (default: 20, max: 100) |

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "prod-123",
      "name": "Modern 3-Seater Sofa",
      "sku": "SOFA-3S-001",
      "description": "Comfortable modern sofa",
      "shortDescription": "Premium 3-seater",
      "productType": "Simple",
      "price": null,
      "compareAtPrice": null,
      "bundlePrice": null,
      "canBeSoldSeparately": true,
      "stockStatus": "InStock",
      "quantity": 10,
      "categoryId": "cat-123",
      "categoryName": "Living Room",
      "brandId": "brand-456",
      "brandName": "Modern Living",
      "isActive": true,
      "isFeatured": false,
      "primaryImageUrl": "/uploads/products/sofa-thumb.webp",
      "createdAt": "2026-02-08T00:00:00Z",
      "pricingEnabled": false,
      "priceLabel": "Contact for Quote"
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 45,
  "totalPages": 3
}
```

---

### Get Product Details

Get detailed information about a specific product.

```http
GET /api/catalog/products/{id}
```

**Response:** `200 OK`
```json
{
  "id": "prod-123",
  "name": "Modern 3-Seater Sofa",
  "sku": "SOFA-3S-001",
  "description": "Full description...",
  "shortDescription": "Premium 3-seater",
  "price": null,
  "compareAtPrice": null,
  "stockStatus": "InStock",
  "quantity": 10,
  "weight": 85.5,
  "length": 220.0,
  "width": 90.0,
  "height": 85.0,
  "dimensionUnit": "cm",
  "weightUnit": "kg",
  "categoryId": "cat-123",
  "categoryName": "Living Room",
  "brandId": "brand-456",
  "brandName": "Modern Living",
  "isActive": true,
  "isFeatured": false,
  "images": [
    {
      "id": "img-1",
      "url": "/uploads/products/sofa-large.webp",
      "type": "Large",
      "isPrimary": true,
      "displayOrder": 1
    }
  ],
  "createdAt": "2026-02-08T00:00:00Z"
}
```

**Errors:**
- `404 Not Found` - Product doesn't exist

---

### Create Product

Create a new product (Simple or Bundle).

```http
POST /api/catalog/products
Authorization: Bearer {token}
```

**Request Body (Simple Product):**
```json
{
  "name": "Modern 3-Seater Sofa",
  "sku": "SOFA-3S-001",
  "description": "Comfortable modern sofa with premium fabric",
  "shortDescription": "Premium 3-seater sofa",
  "productType": "Simple",
  "price": null,
  "compareAtPrice": null,
  "canBeSoldSeparately": true,
  "stockStatus": "InStock",
  "quantity": 10,
  "categoryId": "cat-123",
  "brandId": "brand-456",
  "weight": 85.5,
  "length": 220.0,
  "width": 90.0,
  "height": 85.0,
  "isActive": true,
  "isFeatured": false
}
```

**Request Body (Bundle Product):**
```json
{
  "name": "Living Room Complete Set",
  "sku": "SET-LR-001",
  "description": "Complete living room furniture set",
  "productType": "Bundle",
  "bundlePrice": 4999.99,
  "canBeSoldSeparately": false,
  "stockStatus": "InStock",
  "quantity": 5,
  "categoryId": "cat-bundles",
  "isActive": true,
  "bundleItems": [
    {
      "componentProductId": "prod-sofa",
      "quantity": 1,
      "displayOrder": 1
    },
    {
      "componentProductId": "prod-chair",
      "quantity": 2,
      "priceOverride": 499.99,
      "displayOrder": 2
    }
  ]
}
```

**Response:** `201 Created`
```json
{
  "id": "prod-new-123"
}
```

**Errors:**
- `400 Bad Request` - Validation failed
- `404 Not Found` - Category/Brand/Components not found
- `409 Conflict` - SKU already exists

---

### Upload Product Image

Upload and process product images.

```http
POST /api/catalog/products/{id}/images?isPrimary=true
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Form Data:**
- `file` - Image file (JPEG, PNG, GIF, WebP)

**Response:** `202 Accepted`
```json
{
  "message": "Image upload queued for processing",
  "fileName": "temp_abc123.jpg"
}
```

**Processing:**
1. Image saved temporarily
2. Background service picks it up
3. Generates 3 WebP variants (200px, 600px, 1200px)
4. Saves to `/uploads/products/{id}/`
5. Updates database with image URLs

**Errors:**
- `404 Not Found` - Product doesn't exist
- `400 Bad Request` - Invalid file type
- `413 Payload Too Large` - File exceeds 20MB

---

## Bundles

### Get Bundle Details

Get bundle with all components.

```http
GET /api/catalog/products/{id}/bundle
```

**Response:** `200 OK`
```json
{
  "id": "bundle-123",
  "name": "Living Room Complete Set",
  "sku": "SET-LR-001",
  "description": "Complete living room furniture set",
  "bundlePrice": 4999.99,
  "calculatedPrice": 5497.96,
  "savings": 497.97,
  "components": [
    {
      "componentId": "prod-sofa",
      "name": "3-Seater Sofa",
      "sku": "SOFA-3S-001",
      "imageUrl": "/uploads/products/sofa-thumb.webp",
      "quantity": 1,
      "unitPrice": 1299.99,
      "priceOverride": null,
      "totalPrice": 1299.99,
      "isOptional": false,
      "displayOrder": 1
    },
    {
      "componentId": "prod-chair",
      "name": "Bergere Chair",
      "sku": "CHAIR-BERG-001",
      "imageUrl": "/uploads/products/chair-thumb.webp",
      "quantity": 2,
      "unitPrice": 599.99,
      "priceOverride": 499.99,
      "totalPrice": 999.98,
      "isOptional": false,
      "displayOrder": 2
    }
  ]
}
```

**Errors:**
- `404 Not Found` - Bundle doesn't exist
- `400 Bad Request` - Product is not a bundle

---

### Add Component to Bundle

Add a product to an existing bundle.

```http
POST /api/catalog/products/{bundleId}/components
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "componentProductId": "prod-ottoman",
  "quantity": 1,
  "priceOverride": 199.99,
  "isOptional": true,
  "displayOrder": 3
}
```

**Response:** `201 Created`
```json
{
  "bundleItemId": "bi-789"
}
```

**Errors:**
- `404 Not Found` - Bundle or component doesn't exist
- `409 Conflict` - Component already in bundle
- `400 Bad Request` - Product is not a bundle

---

### Remove Component from Bundle

Remove a product from a bundle.

```http
DELETE /api/catalog/products/{bundleId}/components/{componentId}
Authorization: Bearer {token}
```

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` - Bundle item doesn't exist

---

## Categories

### List Categories

Get all categories with hierarchy.

```http
GET /api/catalog/categories
```

**Response:** `200 OK`
```json
[
  {
    "id": "cat-123",
    "name": "Living Room",
    "description": "Living room furniture",
    "slug": "living-room",
    "parentId": null,
    "children": [
      {
        "id": "cat-456",
        "name": "Sofas",
        "slug": "sofas",
        "parentId": "cat-123"
      }
    ],
    "productCount": 45,
    "isActive": true,
    "displayOrder": 1
  }
]
```

---

### Create Category

Create a new category.

```http
POST /api/catalog/categories
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "name": "Living Room Sofas",
  "description": "Comfortable sofas for modern living rooms",
  "slug": null,
  "parentId": "cat-living-room",
  "displayOrder": 1,
  "isActive": true
}
```

**Response:** `201 Created`
```json
{
  "id": "cat-new-789"
}
```

**Notes:**
- `slug` is auto-generated from `name` if not provided
- `parentId` null = top-level category

---

## Error Responses

All errors follow this format:

```json
{
  "error": "Product.NotFound",
  "message": "Product with ID 'xyz' not found"
}
```

### Error Types

| Type | HTTP Status | Description |
|------|-------------|-------------|
| `NotFound` | 404 | Resource doesn't exist |
| `Validation` | 400 | Invalid request data |
| `Conflict` | 409 | Duplicate resource (e.g., SKU) |
| `Failure` | 500 | Internal server error |

---

## Rate Limiting

**Current:** No rate limiting (development)

**Production:** Recommended limits:
- Anonymous: 60 requests/minute
- Authenticated: 300 requests/minute

---

## Pagination

All list endpoints support pagination:

**Request:**
```http
GET /api/catalog/products?pageNumber=2&pageSize=50
```

**Response Headers:**
```
X-Pagination: {"PageNumber":2,"PageSize":50,"TotalCount":150,"TotalPages":3}
```

**Response Body:**
```json
{
  "items": [...],
  "pageNumber": 2,
  "pageSize": 50,
  "totalCount": 150,
  "totalPages": 3
}
```

---

## Filtering

### Product Filters

```http
GET /api/catalog/products?categoryId=cat-123&brandId=brand-456&isActive=true
```

### Search

Fuzzy search using PostgreSQL trigrams:

```http
GET /api/catalog/products?searchTerm=lether sofa
```

**Matches:**
- "Leather Sofa"
- "Leather 3-Seater Sofa"
- "Modern Leather Sectional"

---

## Authentication

### How to Authenticate

1. **Login** to get tokens:
   ```http
   POST /api/identity/auth/login
   ```

2. **Use access token** in subsequent requests:
   ```http
   Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
   ```

3. **Refresh** when token expires (30 minutes):
   ```http
   POST /api/identity/auth/refresh
   ```

### Protected Endpoints

Endpoints requiring authentication are marked with 🔒 in Swagger.

---

## Request Examples

### cURL

```bash
# Login
curl -X POST http://localhost:8080/api/identity/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@storefront.com","password":"AdminPassword123!"}'

# Create product
curl -X POST http://localhost:8080/api/catalog/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "name": "Modern Sofa",
    "sku": "SOFA-001",
    "productType": "Simple",
    "categoryId": "cat-123",
    "stockStatus": "InStock",
    "quantity": 10,
    "canBeSoldSeparately": true,
    "isActive": true
  }'
```

### JavaScript (Axios)

```javascript
// Login
const { data } = await axios.post('/api/identity/auth/login', {
  email: 'admin@storefront.com',
  password: 'AdminPassword123!'
});

// Store token
localStorage.setItem('accessToken', data.accessToken);

// Create product
await axios.post('/api/catalog/products', {
  name: 'Modern Sofa',
  sku: 'SOFA-001',
  productType: 'Simple',
  categoryId: 'cat-123',
  stockStatus: 'InStock',
  quantity: 10,
  canBeSoldSeparately: true,
  isActive: true
}, {
  headers: {
    Authorization: `Bearer ${data.accessToken}`
  }
});
```

---

## Response Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| `200 OK` | Success | GET requests |
| `201 Created` | Resource created | POST requests |
| `202 Accepted` | Async processing | Image uploads |
| `204 No Content` | Success, no body | DELETE requests |
| `400 Bad Request` | Validation error | Invalid input |
| `401 Unauthorized` | Not authenticated | Missing/invalid token |
| `403 Forbidden` | Not authorized | Insufficient permissions |
| `404 Not Found` | Resource missing | Invalid ID |
| `409 Conflict` | Duplicate resource | SKU/slug exists |
| `500 Internal Server Error` | Server error | Unexpected failures |

---

## Advanced Features

### Image Processing

**Upload Flow:**
1. POST image → Returns 202 Accepted
2. Background service processes image
3. Generates 3 WebP sizes:
   - Thumbnail: 200px width
   - Medium: 600px width
   - Large: 1200px width
4. Updates database with URLs

**Check Processing Status:**
- Poll GET /api/catalog/products/{id}
- Check if `images` array populated

### Bundle Pricing

**Automatic Calculation:**
```json
{
  "bundlePrice": null,  // Will be calculated from components
  "calculatedPrice": 2499.98,  // Sum of component prices
  "savings": 0
}
```

**Fixed Price:**
```json
{
  "bundlePrice": 1999.99,  // Set by admin
  "calculatedPrice": 2499.98,
  "savings": 499.99  // Discount
}
```

---

## Testing with Swagger

1. Open: `http://localhost:8080/swagger`
2. Click **Authorize** button (top-right)
3. Login to get token
4. Paste token: `Bearer YOUR_TOKEN_HERE`
5. Test any endpoint!

**Swagger Features:**
- 🔒 Shows which endpoints require auth
- 📝 Shows request/response schemas
- ✅ Validates requests
- 🧪 Try it out interactively

---

## API Conventions

### Naming

- **Endpoints:** Plural nouns (`/products`, `/categories`)
- **IDs:** String GUIDs (`prod-abc-123`)
- **Dates:** ISO 8601 UTC (`2026-02-08T12:00:00Z`)
- **Enums:** PascalCase strings (`InStock`, `OutOfStock`)

### Headers

**Request:**
```
Content-Type: application/json
Authorization: Bearer {token}
```

**Response:**
```
Content-Type: application/json
X-Correlation-Id: abc-123  (future)
```

---

## Postman Collection

Import this URL to Postman:  
`https://github.com/yourusername/storefront/blob/main/docs/postman_collection.json`

Or manually add:
- Base URL: `http://localhost:8080`
- Authorization: Bearer Token
- Variable: `{{accessToken}}`

---

**More Documentation:**
- [Installation](INSTALLATION.md)
- [Architecture](ARCHITECTURE.md)
- [Product Bundles](features/PRODUCT_BUNDLES.md)
- [Price Configuration](features/PRICE_CONFIGURATION.md)
