# B2B Order Management System Design

## Business Model

**B2B Quote-to-Order System** for furniture/sofa manufacturers:
- Partners browse catalog (no prices shown)
- Partners request orders
- You review, quote, and manage the fulfillment process
- Communication via order comments (payment links, updates, etc.)

---

## System Modules

### 1. **Catalog Module** (Already Being Built)
- Products (Simple + Bundles)
- Categories
- Brands
- Images
- **No public pricing** (configured off)

### 2. **Order Management Module** (NEW)
- Order requests from partners
- Status workflow
- Comments/communication
- Document attachments

### 3. **Partner Management Module** (NEW)
- Partner accounts (B2B customers)
- Company info
- Contact persons
- Credit terms

---

## Order Status Workflow

```
┌─────────────────┐
│  Order Request  │ ← Partner submits order
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Under Review   │ ← You review the request
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Quoted/Pending │ ← You send quote (via comment)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Confirmed     │ ← Partner accepts, payment arranged
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Preparing     │ ← Manufacturing/assembly
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Ready/QC      │ ← Quality check complete
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Shipping     │ ← In transit
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Delivered    │ ← Completed
└─────────────────┘

         ┌─────────────────┐
         │    Cancelled    │ ← Can cancel at any time
         └─────────────────┘
```

---

## Database Schema

### Order Entity

```csharp
public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderNumber { get; set; }  // e.g., "ORD-2026-001"
    
    // Partner Information
    public string PartnerId { get; set; }
    public virtual Partner Partner { get; set; } = null!;
    
    // Order Status
    public OrderStatus Status { get; set; } = OrderStatus.Requested;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    
    // Pricing (Internal - not shown to partner initially)
    public decimal? QuotedAmount { get; set; }
    public decimal? FinalAmount { get; set; }
    public string? Currency { get; set; } = "USD";
    
    // Delivery
    public string? ShippingAddress { get; set; }
    public string? DeliveryNotes { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    
    // Payment
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    
    // Notes
    public string? InternalNotes { get; set; }  // Only visible to admin
    
    // Relationships
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderComment> Comments { get; set; } = new List<OrderComment>();
    public virtual ICollection<OrderDocument> Documents { get; set; } = new List<OrderDocument>();
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
```

### OrderItem Entity

```csharp
public class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
    
    // Product Reference
    public string ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    // Snapshot of product details (in case product changes later)
    public string ProductName { get; set; } = null!;
    public string ProductSKU { get; set; } = null!;
    public ProductType ProductType { get; set; }
    
    // Quantity & Pricing
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }  // Price at time of quote
    public decimal? TotalPrice { get; set; }
    
    // Customization (for future: fabric, color, etc.)
    public string? Specifications { get; set; }
    public string? CustomizationNotes { get; set; }
    
    // Bundle components (if this is a bundle)
    public virtual ICollection<OrderItemComponent> Components { get; set; } = new List<OrderItemComponent>();
    
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### OrderComment Entity

```csharp
public class OrderComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
    
    // Comment Details
    public string Content { get; set; } = null!;
    public CommentType Type { get; set; } = CommentType.General;
    
    // Visibility
    public bool IsVisibleToPartner { get; set; } = true;  // false = internal only
    public bool IsSystemGenerated { get; set; } = false;
    
    // Author
    public string AuthorId { get; set; }
    public string AuthorName { get; set; } = null!;
    public CommentAuthorType AuthorType { get; set; }  // Admin, Partner, System
    
    // Attachments (payment links, documents, etc.)
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }
}
```

### OrderStatusHistory Entity

```csharp
public class OrderStatusHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
    
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    
    public string? Notes { get; set; }
    public string ChangedBy { get; set; } = null!;
    public string ChangedByName { get; set; } = null!;
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
```

### Partner Entity (B2B Customer)

```csharp
public class Partner
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PartnerCode { get; set; } = null!;  // e.g., "PART-001"
    
    // Company Information
    public string CompanyName { get; set; } = null!;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    
    // Contact Information
    public string ContactPerson { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? SecondaryPhone { get; set; }
    
    // Address
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string? State { get; set; }
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
    
    // Business Details
    public PartnerType PartnerType { get; set; }  // Retailer, Distributor, Designer
    public string? Website { get; set; }
    public string? Notes { get; set; }
    
    // Credit Terms
    public decimal? CreditLimit { get; set; }
    public int? PaymentTermDays { get; set; }  // e.g., Net 30
    
    // Status
    public bool IsActive { get; set; } = true;
    public bool IsApproved { get; set; } = false;
    
    // Relationships
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
```

---

## Enums

```csharp
public enum OrderStatus
{
    Requested = 0,      // Partner submitted order request
    UnderReview = 1,    // Admin reviewing
    Quoted = 2,         // Quote sent to partner
    Confirmed = 3,      // Partner confirmed, payment arranged
    Preparing = 4,      // Manufacturing/assembly
    ReadyForQC = 5,     // Quality check
    QCPassed = 6,       // QC approved
    Shipping = 7,       // In transit
    Delivered = 8,      // Completed
    Cancelled = 9       // Cancelled by admin or partner
}

public enum PaymentStatus
{
    Pending = 0,
    PartiallyPaid = 1,
    Paid = 2,
    Refunded = 3
}

public enum CommentType
{
    General = 0,
    Quote = 1,          // Quote/pricing information
    Payment = 2,        // Payment link or info
    Shipping = 3,       // Shipping updates
    Internal = 4,       // Internal notes (not visible to partner)
    StatusChange = 5    // System-generated status change
}

public enum CommentAuthorType
{
    Admin = 0,
    Partner = 1,
    System = 2
}

public enum PartnerType
{
    Retailer = 0,
    Distributor = 1,
    InteriorDesigner = 2,
    Contractor = 3,
    Other = 4
}
```

---

## API Endpoints

### Partner Endpoints (Mobile App)

```
# Browse catalog
GET /api/catalog/products
GET /api/catalog/products/{id}
GET /api/catalog/categories

# Order Management
POST /api/orders                          # Create order request
GET /api/orders                           # List partner's orders
GET /api/orders/{id}                      # Get order details
POST /api/orders/{id}/comments            # Add comment
GET /api/orders/{id}/comments             # Get comments
POST /api/orders/{id}/cancel              # Cancel order
```

### Admin Endpoints (Back Office)

```
# Order Management
GET /api/admin/orders                     # List all orders
GET /api/admin/orders/{id}                # Get order details
PUT /api/admin/orders/{id}/status         # Update status
POST /api/admin/orders/{id}/quote         # Add quote/pricing
POST /api/admin/orders/{id}/comments      # Add comment
POST /api/admin/orders/{id}/documents     # Upload document
GET /api/admin/orders/stats               # Dashboard stats

# Partner Management
GET /api/admin/partners                   # List partners
POST /api/admin/partners                  # Create partner
PUT /api/admin/partners/{id}              # Update partner
PUT /api/admin/partners/{id}/approve      # Approve partner
```

---

## Mobile App Flow (Partner)

### 1. Browse Catalog
```
┌─────────────────────────────┐
│   Product List              │
│   [No prices shown]         │
│   "Modern 3-Seater Sofa"    │
│   [View Details]            │
└─────────────────────────────┘
```

### 2. View Product Details
```
┌─────────────────────────────┐
│   Modern 3-Seater Sofa      │
│   Images, Specifications    │
│   [Request Quote]           │
└─────────────────────────────┘
```

### 3. Create Order Request
```
┌─────────────────────────────┐
│   Order Request             │
│   Products: (selected)      │
│   - 3-Seater Sofa x 2       │
│   - Bergere Chair x 4       │
│                             │
│   Delivery Address: [...]   │
│   Requested Date: [...]     │
│   Notes: [...]              │
│                             │
│   [Submit Request]          │
└─────────────────────────────┘
```

### 4. Track Order
```
┌─────────────────────────────┐
│   Order #ORD-2026-001       │
│   Status: Confirmed         │
│   ████░░░░░░░░ 30%          │
│                             │
│   Timeline:                 │
│   ✓ Requested (Jan 1)       │
│   ✓ Quoted (Jan 2)          │
│   ✓ Confirmed (Jan 3)       │
│   ➤ Preparing (Now)         │
│   ○ Shipping                │
│   ○ Delivered               │
│                             │
│   Comments (3)              │
│   [View Comments]           │
└─────────────────────────────┘
```

### 5. Communication
```
┌─────────────────────────────┐
│   Order Comments            │
│   ┌─────────────────────┐   │
│   │ Admin, Jan 2        │   │
│   │ Quote: $5,499       │   │
│   │ Payment Link: [URL] │   │
│   └─────────────────────┘   │
│   ┌─────────────────────┐   │
│   │ You, Jan 3          │   │
│   │ Payment completed   │   │
│   └─────────────────────┘   │
│   ┌─────────────────────┐   │
│   │ System, Jan 5       │   │
│   │ Order confirmed     │   │
│   └─────────────────────┘   │
│                             │
│   [Add Comment]             │
└─────────────────────────────┘
```

---

## Admin Panel Flow

### 1. Dashboard
```
┌─────────────────────────────────────┐
│   Orders Dashboard                  │
│   ┌──────────┬──────────┬─────────┐ │
│   │ Pending  │ Active   │ Complete││
│   │   12     │   45     │   234   ││
│   └──────────┴──────────┴─────────┘ │
│                                     │
│   Recent Orders:                    │
│   • ORD-001 - ACME Corp - Quoted   │
│   • ORD-002 - XYZ Ltd - Preparing  │
│   • ORD-003 - ABC Inc - Shipping   │
└─────────────────────────────────────┘
```

### 2. Order Details
```
┌─────────────────────────────────────┐
│   Order #ORD-2026-001               │
│   ┌─────────────────────────────┐   │
│   │ Partner: ACME Corporation   │   │
│   │ Status: [Preparing ▼]       │   │
│   │ Amount: $5,499.00           │   │
│   └─────────────────────────────┘   │
│                                     │
│   Items:                            │
│   1. Modern 3-Seater Sofa x 2       │
│   2. Bergere Chair x 4              │
│                                     │
│   Actions:                          │
│   [Update Status] [Add Quote]       │
│   [Add Comment] [Upload Document]   │
└─────────────────────────────────────┘
```

### 3. Add Quote/Comment
```
┌─────────────────────────────────────┐
│   Add Quote to Order                │
│   ┌─────────────────────────────┐   │
│   │ Quote Amount: [5499.00]     │   │
│   │ Delivery Fee: [200.00]      │   │
│   │ Total: $5,699.00            │   │
│   │                             │   │
│   │ Message to Partner:         │   │
│   │ [Thank you for your order...│   │
│   │                             │   │
│   │ Payment Link:               │   │
│   │ [https://pay.example.com/...│   │
│   │                             │   │
│   │ ☑ Send notification         │   │
│   └─────────────────────────────┘   │
│   [Send Quote]                      │
└─────────────────────────────────────┘
```

---

## Implementation Priority

### Phase 1: Order Module Foundation (Week 1)
- ✅ Create Order, OrderItem, OrderComment entities
- ✅ Create OrderStatus enum and workflow
- ✅ Database tables and relationships
- ✅ Basic CRUD endpoints

### Phase 2: Order Workflow (Week 1)
- ✅ Status change logic with history tracking
- ✅ Comment system (admin ↔ partner)
- ✅ Order creation from catalog
- ✅ Quote management

### Phase 3: Partner Management (Week 2)
- ✅ Partner entity and registration
- ✅ Partner authentication
- ✅ Partner dashboard
- ✅ Order history per partner

### Phase 4: Mobile App (Week 2-3)
- ✅ Catalog browsing
- ✅ Order request flow
- ✅ Order tracking
- ✅ Comment/communication UI

### Phase 5: Admin Panel Enhancements (Week 3)
- ✅ Order management dashboard
- ✅ Status workflow UI
- ✅ Quote management UI
- ✅ Document uploads

---

## Benefits

✅ **B2B Optimized**: Quote-to-order workflow
✅ **Transparent**: Partners see order progress in real-time
✅ **Communication**: Built-in commenting system
✅ **Flexible**: Can share payment links, documents, updates
✅ **Scalable**: Ready for multiple partners
✅ **Professional**: Complete order lifecycle management

---

## Next Steps

Ready to implement:
1. **Catalog Module** (Products + Bundles + Price Config)
2. **Order Module** (Order management + Status workflow)
3. **Partner Module** (Partner accounts + authentication)
4. **Mobile App** (Browse + Request + Track)

**Shall I start implementing? Which module first?**
- Option A: Complete Catalog (Bundles + Price Config) first
- Option B: Start Order Module alongside Catalog
- Option C: Your preference?

Let me know! 🚀
