# Order System with Cart & Color Charts - Complete Design 🛒

Comprehensive B2B order management system with shopping cart and fabric/color selection.

---

## 📋 **System Overview**

### **Key Features:**
1. ✅ **Color/Fabric Chart Management** - Admin uploads and manages color options
2. ✅ **Shopping Cart** - Partners add products with color selections
3. ✅ **Order Creation** - Convert cart to order request
4. ✅ **Order Status Workflow** - 11 statuses from Draft to Delivered
5. ✅ **Comment System** - Communication between admin and partner

---

## 🏗️ **Database Schema**

### **Module: Orders**

New schema: `orders`

**Tables:**
1. `ColorCharts` - Fabric/color collections
2. `ColorOptions` - Individual colors in each chart
3. `ProductColorCharts` - Link products to available charts
4. `Carts` - Partner shopping carts
5. `CartItems` - Items in cart with color selections
6. `Orders` - Order requests
7. `OrderItems` - Order line items with colors
8. `OrderComments` - Communication thread

---

## 📊 **Entity Relationships**

```
ColorChart (1) ──→ (N) ColorOptions
ColorChart (1) ──→ (N) ProductColorCharts ──→ Product (Catalog)

Partner (Identity) ──→ Cart (1)
Cart (1) ──→ (N) CartItems ──→ Product + ColorOption

Partner ──→ (N) Orders
Order (1) ──→ (N) OrderItems
Order (1) ──→ (N) OrderComments
```

---

## 🎨 **Color Chart System**

### **ColorChart Entity**
```csharp
- Id, Name, Code
- Description, Type (Fabric/Leather/Wood/Metal)
- MainImageUrl, ThumbnailUrl
- IsActive
- ColorOptions (collection)
- ProductColorCharts (collection)
```

### **ColorOption Entity**
```csharp
- Id, Name, Code
- HexColor (for preview)
- ImageUrl (actual fabric photo)
- IsAvailable, StockLevel
- PriceAdjustment (optional)
- DisplayOrder
```

### **ProductColorChart Entity**
```csharp
- Links Product to ColorChart
- IsRequired (must select?)
- AllowMultiple (can select multiple colors?)
```

### **Example: Sofa with Fabric Options**

```
Product: "Modern 3-Seater Sofa"
  ├── ColorChart 1: "Premium Fabrics 2024"
  │   ├── Option: "Royal Blue" (#1E3A8A)
  │   ├── Option: "Charcoal Gray" (#374151)
  │   └── Option: "Cream White" (#FFFBEB)
  └── ColorChart 2: "Leather Collection"
      ├── Option: "Black Leather"
      ├── Option: "Brown Leather"
      └── Option: "Tan Leather"
```

---

## 🛒 **Cart System**

### **Cart Flow**

```
1. Partner browses products
   ↓
2. Clicks "Add to Cart"
   ↓
3. Selects color from available charts
   ↓
4. Enters quantity
   ↓
5. Adds customization notes (optional)
   ↓
6. Item added to cart
   ↓
7. Repeat for more products
   ↓
8. Review cart
   ↓
9. Checkout → Creates Order
```

### **Cart Entity**
```csharp
- Id
- PartnerUserId, PartnerCompanyId
- IsActive
- Items (collection)
```

### **CartItem Entity**
```csharp
- Product info (denormalized)
- ColorChartId, ColorOptionId
- Quantity
- CustomizationNotes
```

---

## 📦 **Order System**

### **Order Status Workflow**

```
Draft (0)           → Cart not submitted
  ↓
Pending (1)         → Order submitted, awaiting admin review
  ↓
QuoteSent (2)       → Admin sends quote with prices
  ↓
Confirmed (3)       → Partner confirms quote
  ↓
Preparing (4)       → Admin preparing order
  ↓
QualityCheck (5)    → QC inspection
  ↓
ReadyToShip (6)     → Packed and ready
  ↓
Shipping (7)        → In transit (with tracking)
  ↓
Delivered (8)       → Delivered to partner
  
---
Cancelled (9)       → Order cancelled
Rejected (10)       → Quote rejected by partner
```

### **Order Entity**
```csharp
- Id, OrderNumber (e.g., "ORD-2024-0001")
- PartnerCompanyId, PartnerUserId
- Status (enum)
- Pricing (SubTotal, Tax, Shipping, Discount, Total)
- Delivery Address (full address)
- RequestedDeliveryDate, ExpectedDeliveryDate
- Notes, InternalNotes
- TrackingNumber, ShippingProvider
- Timestamps (Created, Updated, Submitted, Confirmed)
- Items (collection)
- Comments (collection)
```

### **OrderItem Entity**
```csharp
- Product info (denormalized for history)
- ColorChart & ColorOption (selected)
- Quantity
- UnitPrice, Discount, TotalPrice (set by admin)
- CustomizationNotes
```

---

## 💬 **Comment System**

### **CommentType Enum**
- General - Regular communication
- StatusChange - Auto-generated status updates
- Quote - Price/quote information
- Payment - Payment details
- Shipping - Shipping updates
- Internal - Admin-only notes

### **OrderComment Entity**
```csharp
- Content, Type
- AuthorId, AuthorName, AuthorType (Admin/Partner)
- IsInternal (admin-only?)
- IsSystemGenerated (auto-created?)
- AttachmentUrl (optional file)
- Timestamps
```

### **Comment Thread Example**

```
Order #ORD-2024-0001

[SYSTEM] Order created - Jan 10, 2024 10:00 AM
Partner: John Doe requested 50 chairs

[John - Partner] Jan 10, 10:05 AM
We need these by February 15th for our new showroom opening.

[Admin - Sarah] Jan 10, 2:00 PM (Quote)
Quote attached. Total: $12,500 including shipping.
Delivery by Feb 10th confirmed.

[John - Partner] Jan 10, 3:00 PM
Quote approved! Please proceed.

[SYSTEM] Order confirmed - Jan 10, 3:01 PM

[Admin - Sarah] Jan 15 (Internal)
Manufacturing scheduled for Jan 20-25.

[Admin - Sarah] Jan 25 (Shipping)
Order shipped! Tracking: UPS-123456789
Expected delivery: Feb 8th

[SYSTEM] Order delivered - Feb 8, 2024
```

---

## 🔄 **API Endpoints**

### **Admin - Color Charts**
```http
POST   /api/admin/color-charts           # Upload new chart
GET    /api/admin/color-charts           # List all charts
GET    /api/admin/color-charts/{id}      # Chart details
PUT    /api/admin/color-charts/{id}      # Update chart
DELETE /api/admin/color-charts/{id}      # Delete chart
POST   /api/admin/color-charts/{id}/options  # Add color option
PUT    /api/admin/color-charts/{id}/options/{optionId}  # Update option
DELETE /api/admin/color-charts/{id}/options/{optionId}  # Delete option
```

### **Admin - Product Color Assignment**
```http
POST   /api/admin/products/{productId}/color-charts     # Assign chart to product
GET    /api/admin/products/{productId}/color-charts     # Get assigned charts
DELETE /api/admin/products/{productId}/color-charts/{chartId}  # Remove chart
```

### **Partner - Cart**
```http
GET    /api/partner/cart                 # Get my cart
POST   /api/partner/cart/items           # Add item to cart
PUT    /api/partner/cart/items/{id}      # Update item (quantity, color)
DELETE /api/partner/cart/items/{id}      # Remove item
DELETE /api/partner/cart                 # Clear cart
```

### **Partner - Orders**
```http
POST   /api/partner/orders               # Create order from cart
GET    /api/partner/orders               # My orders list
GET    /api/partner/orders/{id}          # Order details
POST   /api/partner/orders/{id}/comments # Add comment
POST   /api/partner/orders/{id}/confirm  # Confirm quote
POST   /api/partner/orders/{id}/cancel   # Cancel order
```

### **Admin - Orders**
```http
GET    /api/admin/orders                 # All orders
GET    /api/admin/orders/{id}            # Order details
PUT    /api/admin/orders/{id}/status     # Update status
PUT    /api/admin/orders/{id}/pricing    # Set pricing
POST   /api/admin/orders/{id}/comments   # Add comment
PUT    /api/admin/orders/{id}/shipping   # Add tracking info
```

### **Public - Color Charts (for browsing)**
```http
GET    /api/color-charts/{productId}     # Get charts for product
GET    /api/color-charts/{chartId}/options  # Get color options
```

---

## 🎨 **Frontend Pages**

### **Admin Pages**

1. **Color Chart Management** (`/admin/color-charts`)
   - List all charts
   - Upload new chart
   - Manage color options
   - Assign to products

2. **Color Chart Details** (`/admin/color-charts/[id]`)
   - Chart information
   - Color options grid
   - Add/edit/delete options
   - View assigned products

3. **Product Color Assignment** (`/admin/products/[id]` - tab)
   - Assign/remove color charts
   - Set as required/optional

4. **Order Management** (`/admin/orders`)
   - List all orders
   - Filter by status, partner, date
   - Quick actions (review, send quote)

5. **Order Details** (`/admin/orders/[id]`)
   - Order information
   - Items with colors
   - Status timeline
   - Comment thread
   - Actions (update status, add pricing, ship)

### **Partner Pages**

1. **Product View** (updated)
   - Color chart selector
   - Color swatches/images
   - Add to cart with color

2. **Shopping Cart** (`/partner/cart`)
   - Cart items with colors
   - Quantity adjustment
   - Remove items
   - Checkout button

3. **Checkout** (`/partner/checkout`)
   - Review items
   - Delivery address
   - Delivery date preference
   - Special notes
   - Submit order request

4. **My Orders** (`/partner/orders`)
   - Order list with statuses
   - Filter by status
   - Quick view

5. **Order Details** (`/partner/orders/[id]`)
   - Order information
   - Items with colors
   - Status timeline
   - Comment thread
   - Actions (confirm quote, add comment, track shipment)

---

## 💡 **User Flows**

### **Flow 1: Admin Uploads Color Chart**

```
1. Admin → /admin/color-charts
2. Click "Upload Color Chart"
3. Fill form:
   - Name: "Premium Fabrics 2024"
   - Code: "FABRIC-2024-PREMIUM"
   - Type: "Fabric"
   - Upload main image
4. Submit → Chart created
5. Click "Add Colors"
6. For each color:
   - Name: "Royal Blue"
   - Code: "RB-001"
   - Hex: #1E3A8A
   - Upload fabric photo
   - Price adjustment: +$50
7. Save → Colors added
8. Go to product page
9. Click "Assign Color Charts"
10. Select "Premium Fabrics 2024"
11. Set as Required
12. Save
```

### **Flow 2: Partner Orders Product with Color**

```
1. Partner → Browse products
2. Click "Modern Sofa"
3. See available color charts
4. Expand "Premium Fabrics 2024"
5. View color swatches
6. Select "Royal Blue"
7. See preview/price adjustment
8. Enter quantity: 10
9. Add notes: "Need armrests in matching color"
10. Click "Add to Cart"
11. Item added to cart
12. Continue shopping or checkout
13. Click "View Cart"
14. Review items
15. Click "Checkout"
16. Fill delivery info
17. Set delivery date: Feb 15, 2024
18. Add notes: "Please call before delivery"
19. Submit → Order created (Status: Pending)
20. Redirect to order details
21. Wait for admin quote
```

### **Flow 3: Admin Processes Order**

```
1. Admin → /admin/orders
2. See new order (Status: Pending)
3. Click to view details
4. Review items and colors
5. Calculate pricing:
   - 10x Sofas @ $1,200 = $12,000
   - Royal Blue fabric +$50 each = +$500
   - Subtotal: $12,500
   - Shipping: $500
   - Total: $13,000
6. Click "Set Pricing"
7. Enter amounts
8. Click "Send Quote"
9. Status → QuoteSent
10. Add comment: "Quote attached. Delivery by Feb 10."
11. Partner confirms → Status: Confirmed
12. Admin updates: Preparing → QC → Ready → Shipping
13. Add tracking: UPS-123456
14. Status → Shipping
15. Delivered → Status: Delivered
```

---

## 🎯 **Benefits**

### **For Partners:**
- ✅ Visual color selection (see actual fabrics)
- ✅ Shopping cart for multiple items
- ✅ Clear order status tracking
- ✅ Direct communication with admin
- ✅ Order history

### **For Admin:**
- ✅ Centralized order management
- ✅ Easy color chart maintenance
- ✅ Flexible pricing per order
- ✅ Status workflow
- ✅ Internal notes
- ✅ Communication history

### **For Business:**
- ✅ Professional B2B experience
- ✅ Reduced email/phone communication
- ✅ Clear audit trail
- ✅ Customization support
- ✅ Scalable system

---

## 📦 **Implementation Plan**

### **Phase 1: Backend (Current)**
1. ✅ Created all entities
2. ⏳ Create DbContext
3. ⏳ Create Commands (Add to cart, Create order, etc.)
4. ⏳ Create Queries (Get cart, Get orders, etc.)
5. ⏳ Create API controllers
6. ⏳ Update database initialization

### **Phase 2: Admin UI**
1. Color chart management pages
2. Product color assignment
3. Order management pages
4. Order details with actions

### **Phase 3: Partner UI**
1. Product page with color selector
2. Shopping cart page
3. Checkout flow
4. Order list and details

### **Phase 4: Integration**
1. Connect Orders module to Identity (partners)
2. Connect Orders module to Catalog (products)
3. Real-time updates (optional)
4. Email notifications

---

## 🚀 **Next Steps**

I'll continue implementing:
1. Orders DbContext
2. Key commands (AddToCart, CreateOrder, AddComment)
3. Queries (GetCart, GetOrders)
4. API controllers
5. Frontend pages

This is a comprehensive system - let me know if you want me to continue or make any adjustments to the design!

---

**Total Entities:** 10  
**Total Endpoints:** ~30  
**Total Pages:** ~15  
**Complexity:** High (but worth it for B2B! 💪)
