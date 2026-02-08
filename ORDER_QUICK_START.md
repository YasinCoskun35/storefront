# 🚀 Order System Quick Start Guide

How to test the new order system with cart and color charts.

---

## 📋 **Prerequisites**

1. ✅ PostgreSQL running (Docker: `docker-compose up -d`)
2. ✅ Backend built successfully (`dotnet build`)
3. ✅ API running (`cd src/API/Storefront.Api && dotnet run`)

---

## 🎯 **Test Workflow**

### **Step 1: Get Authentication Tokens**

#### **Admin Token**
```bash
curl -X POST https://localhost:7080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@storefront.com",
    "password": "Admin123!@#"
  }'
```

#### **Partner Token**
```bash
# First, admin creates partner (from previous implementation)
# Then partner logs in:
curl -X POST https://localhost:7080/api/auth/partner/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "partner@company.com",
    "password": "Partner123!"
  }'
```

---

### **Step 2: Admin Creates Color Chart**

```bash
curl -X POST https://localhost:7080/api/admin/color-charts \
  -H "Authorization: Bearer {ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Premium Fabrics 2024",
    "code": "FABRIC-2024-PREMIUM",
    "description": "High-quality upholstery fabrics for sofas and chairs",
    "type": "Fabric",
    "mainImageUrl": "https://example.com/fabrics-chart.jpg",
    "thumbnailUrl": "https://example.com/fabrics-thumb.jpg"
  }'
```

**Response:**
```json
{
  "id": "chart-abc123"
}
```

---

### **Step 3: Admin Adds Color Options**

```bash
# Royal Blue
curl -X POST https://localhost:7080/api/admin/color-charts/chart-abc123/options \
  -H "Authorization: Bearer {ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Royal Blue",
    "code": "RB-001",
    "hexColor": "#1E3A8A",
    "imageUrl": "https://example.com/royal-blue.jpg",
    "priceAdjustment": 50.00,
    "displayOrder": 1
  }'

# Charcoal Gray
curl -X POST https://localhost:7080/api/admin/color-charts/chart-abc123/options \
  -H "Authorization: Bearer {ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Charcoal Gray",
    "code": "CG-002",
    "hexColor": "#374151",
    "imageUrl": "https://example.com/charcoal-gray.jpg",
    "priceAdjustment": 0,
    "displayOrder": 2
  }'

# Cream White
curl -X POST https://localhost:7080/api/admin/color-charts/chart-abc123/options \
  -H "Authorization: Bearer {ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Cream White",
    "code": "CW-003",
    "hexColor": "#FFFBEB",
    "imageUrl": "https://example.com/cream-white.jpg",
    "priceAdjustment": 25.00,
    "displayOrder": 3
  }'
```

---

### **Step 4: Partner Adds Product to Cart**

```bash
curl -X POST https://localhost:7080/api/partner/cart/items \
  -H "Authorization: Bearer {PARTNER_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "product-123",
    "productName": "Modern 3-Seater Sofa",
    "productSKU": "SOFA-3S-001",
    "productImageUrl": "https://example.com/sofa.jpg",
    "quantity": 10,
    "colorChartId": "chart-abc123",
    "colorChartName": "Premium Fabrics 2024",
    "colorOptionId": "color-royal-blue-id",
    "colorOptionName": "Royal Blue",
    "colorOptionCode": "RB-001",
    "customizationNotes": "Need extra cushions in matching color"
  }'
```

**Response:**
```json
{
  "cartId": "cart-xyz789",
  "message": "Item added to cart"
}
```

---

### **Step 5: Partner Views Cart**

```bash
curl -X GET https://localhost:7080/api/partner/cart \
  -H "Authorization: Bearer {PARTNER_TOKEN}"
```

**Response:**
```json
{
  "id": "cart-xyz789",
  "itemCount": 1,
  "items": [
    {
      "id": "item-001",
      "productId": "product-123",
      "productName": "Modern 3-Seater Sofa",
      "productSKU": "SOFA-3S-001",
      "productImageUrl": "https://example.com/sofa.jpg",
      "quantity": 10,
      "colorChartId": "chart-abc123",
      "colorChartName": "Premium Fabrics 2024",
      "colorOptionId": "color-royal-blue-id",
      "colorOptionName": "Royal Blue",
      "colorOptionCode": "RB-001",
      "customizationNotes": "Need extra cushions in matching color"
    }
  ]
}
```

---

### **Step 6: Partner Creates Order**

```bash
curl -X POST https://localhost:7080/api/partner/orders \
  -H "Authorization: Bearer {PARTNER_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "deliveryAddress": "123 Main Street, Suite 500",
    "deliveryCity": "New York",
    "deliveryState": "NY",
    "deliveryPostalCode": "10001",
    "deliveryCountry": "United States",
    "deliveryNotes": "Please call 30 minutes before delivery",
    "requestedDeliveryDate": "2024-03-15T00:00:00Z",
    "notes": "Urgent order for new showroom opening. Need by March 15th."
  }'
```

**Response:**
```json
{
  "orderId": "order-def456"
}
```

---

### **Step 7: Partner Views Orders**

```bash
curl -X GET "https://localhost:7080/api/partner/orders?pageNumber=1&pageSize=20" \
  -H "Authorization: Bearer {PARTNER_TOKEN}"
```

**Response:**
```json
{
  "items": [
    {
      "id": "order-def456",
      "orderNumber": "ORD-2024-0001",
      "status": "Pending",
      "itemCount": 1,
      "totalAmount": null,
      "currency": "USD",
      "createdAt": "2024-02-08T03:00:00Z",
      "requestedDeliveryDate": "2024-03-15T00:00:00Z",
      "hasUnreadComments": false
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

---

### **Step 8: Partner Views Order Details**

```bash
curl -X GET https://localhost:7080/api/partner/orders/order-def456 \
  -H "Authorization: Bearer {PARTNER_TOKEN}"
```

**Response:**
```json
{
  "id": "order-def456",
  "orderNumber": "ORD-2024-0001",
  "status": "Pending",
  "partnerCompanyId": "company-123",
  "partnerCompanyName": "Partner Company",
  "subTotal": null,
  "taxAmount": null,
  "shippingCost": null,
  "discount": null,
  "totalAmount": null,
  "currency": "USD",
  "deliveryAddress": "123 Main Street, Suite 500",
  "deliveryCity": "New York",
  "deliveryState": "NY",
  "deliveryPostalCode": "10001",
  "deliveryCountry": "United States",
  "deliveryNotes": "Please call 30 minutes before delivery",
  "requestedDeliveryDate": "2024-03-15T00:00:00Z",
  "expectedDeliveryDate": null,
  "trackingNumber": null,
  "shippingProvider": null,
  "notes": "Urgent order for new showroom opening. Need by March 15th.",
  "createdAt": "2024-02-08T03:00:00Z",
  "submittedAt": "2024-02-08T03:00:00Z",
  "confirmedAt": null,
  "items": [
    {
      "id": "item-001",
      "productId": "product-123",
      "productName": "Modern 3-Seater Sofa",
      "productSKU": "SOFA-3S-001",
      "productImageUrl": "https://example.com/sofa.jpg",
      "quantity": 10,
      "colorChartName": "Premium Fabrics 2024",
      "colorOptionName": "Royal Blue",
      "colorOptionCode": "RB-001",
      "colorOptionImageUrl": null,
      "unitPrice": null,
      "discount": null,
      "totalPrice": null,
      "customizationNotes": "Need extra cushions in matching color"
    }
  ],
  "comments": [
    {
      "id": "comment-001",
      "content": "Order created with 1 item(s)",
      "type": "StatusChange",
      "authorName": "System",
      "authorType": "System",
      "isInternal": false,
      "attachmentUrl": null,
      "attachmentFileName": null,
      "createdAt": "2024-02-08T03:00:00Z"
    }
  ]
}
```

---

### **Step 9: Admin Reviews Order**

```bash
# Admin views all orders
curl -X GET "https://localhost:7080/api/admin/orders?pageNumber=1&pageSize=20" \
  -H "Authorization: Bearer {ADMIN_TOKEN}"

# Admin views order details
curl -X GET https://localhost:7080/api/admin/orders/order-def456 \
  -H "Authorization: Bearer {ADMIN_TOKEN}"
```

---

### **Step 10: Admin Updates Order Status**

```bash
# Admin sends quote
curl -X PUT https://localhost:7080/api/admin/orders/order-def456/status \
  -H "Authorization: Bearer {ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "newStatus": 2,
    "notes": "Quote sent: $12,500 including shipping"
  }'

# Status values:
# 0 = Draft, 1 = Pending, 2 = QuoteSent, 3 = Confirmed
# 4 = Preparing, 5 = QualityCheck, 6 = ReadyToShip, 7 = Shipping
# 8 = Delivered, 9 = Cancelled, 10 = Rejected
```

---

### **Step 11: Partner Adds Comment**

```bash
curl -X POST https://localhost:7080/api/partner/orders/order-def456/comments \
  -H "Authorization: Bearer {PARTNER_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Quote approved! Please proceed with order.",
    "type": 2,
    "attachmentUrl": null,
    "attachmentFileName": null
  }'

# CommentType values:
# 0 = General, 1 = StatusChange, 2 = Quote, 3 = Payment, 4 = Shipping, 5 = Internal
```

---

### **Step 12: Admin Adds Internal Note**

```bash
curl -X POST https://localhost:7080/api/admin/orders/order-def456/comments \
  -H "Authorization: Bearer {ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Manufacturing scheduled for Feb 15-20. Premium fabric batch confirmed.",
    "type": 0,
    "isInternal": true,
    "attachmentUrl": null,
    "attachmentFileName": null
  }'
```

---

## 📊 **Complete Order Lifecycle Test**

```bash
# 1. Order Created (Status: Pending)
curl -X POST https://localhost:7080/api/partner/orders -H "Authorization: Bearer {PARTNER_TOKEN}" -d '{...}'

# 2. Admin Sends Quote (Status: QuoteSent)
curl -X PUT https://localhost:7080/api/admin/orders/{id}/status -H "Authorization: Bearer {ADMIN_TOKEN}" -d '{"newStatus": 2}'

# 3. Partner Confirms (Status: Confirmed)
curl -X PUT https://localhost:7080/api/admin/orders/{id}/status -H "Authorization: Bearer {ADMIN_TOKEN}" -d '{"newStatus": 3}'

# 4. Admin Preparing (Status: Preparing)
curl -X PUT https://localhost:7080/api/admin/orders/{id}/status -H "Authorization: Bearer {ADMIN_TOKEN}" -d '{"newStatus": 4}'

# 5. Quality Check (Status: QualityCheck)
curl -X PUT https://localhost:7080/api/admin/orders/{id}/status -H "Authorization: Bearer {ADMIN_TOKEN}" -d '{"newStatus": 5}'

# 6. Ready to Ship (Status: ReadyToShip)
curl -X PUT https://localhost:7080/api/admin/orders/{id}/status -H "Authorization: Bearer {ADMIN_TOKEN}" -d '{"newStatus": 6}'

# 7. Shipping (Status: Shipping)
curl -X PUT https://localhost:7080/api/admin/orders/{id}/status -H "Authorization: Bearer {ADMIN_TOKEN}" -d '{"newStatus": 7, "notes": "Tracking: UPS-123456"}'

# 8. Delivered (Status: Delivered)
curl -X PUT https://localhost:7080/api/admin/orders/{id}/status -H "Authorization: Bearer {ADMIN_TOKEN}" -d '{"newStatus": 8}'
```

---

## ✅ **Success Indicators**

After following these steps, you should see:

1. ✅ Color chart created with 3 color options
2. ✅ Product added to partner cart with color selection
3. ✅ Order created from cart (Status: Pending)
4. ✅ System comment auto-generated
5. ✅ Order visible in partner's order list
6. ✅ Order visible in admin's order list
7. ✅ Status updates creating system comments
8. ✅ Partner and admin comments in thread
9. ✅ Order progressing through statuses
10. ✅ Delivery tracking information stored

---

## 🔍 **Database Verification**

```sql
-- Check color charts
SELECT * FROM orders."ColorCharts";

-- Check color options
SELECT * FROM orders."ColorOptions";

-- Check carts
SELECT * FROM orders."Carts";

-- Check cart items
SELECT * FROM orders."CartItems";

-- Check orders
SELECT * FROM orders."Orders";

-- Check order items
SELECT * FROM orders."OrderItems";

-- Check comments
SELECT * FROM orders."OrderComments";
```

---

## 🎯 **What's Working**

- ✅ Color chart management
- ✅ Color option management  
- ✅ Shopping cart (add items with colors)
- ✅ Order creation from cart
- ✅ Order listing with pagination
- ✅ Order details with items and comments
- ✅ Status updates with automatic comments
- ✅ Partner comments on orders
- ✅ Admin internal notes
- ✅ Comment type categorization
- ✅ B2B workflow (no upfront pricing)

---

## 📚 **Related Documentation**

- Design: `/ORDER_SYSTEM_DESIGN.md`
- Backend Summary: `/ORDER_BACKEND_COMPLETE.md`
- This Guide: `/ORDER_QUICK_START.md`

---

## 🚀 **Ready for Frontend!**

The backend is fully functional and tested. Now we can build the UI to make this user-friendly!

**Next:** Implement Admin and Partner UI pages 🎨
