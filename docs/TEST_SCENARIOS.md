# Storefront API — Test Scenarios

This document describes test scenarios for the Storefront API. Use the Postman collection (`Storefront.postman_collection.json`) to execute these flows.

## Prerequisites

- Backend running at `http://localhost:8080` (or update `baseUrl` in Postman)
- Database seeded with default data (admin, partner, categories, products)
- **Default credentials** (from `IdentityDataSeeder`):
  - Admin: `admin@storefront.com` / `AdminPassword123!`
  - Partner: `partner@example.com` / `PartnerPassword123!` (partner must be approved)

---

## Scenario 1: Authentication

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Config → Get App Mode | 200, `{ "mode": "B2C" }` or `"B2B"` | No auth |
| 2 | Identity — Auth → Admin Login | 200, `accessToken`, `refreshToken` | Saves `adminToken` |
| 3 | Identity — Auth → Partner Login | 200, `accessToken` | Saves `partnerToken`; partner must exist and be approved |
| 4 | Identity — Auth → Get Partner Profile | 200, partner details | Requires `partnerToken` |

**Negative:** Admin Login with wrong password → 401

---

## Scenario 2: Catalog (Public)

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Catalog — Products → Search Products | 200, `items`, `totalCount` | No auth; pagination works |
| 2 | Catalog — Categories → Get Categories | 200, array of categories | Saves `categoryId` if creating category |
| 3 | Catalog — Products → Get Product By ID | 200, product details | Set `productId` first (e.g. from Search) |
| 4 | Catalog — Variant Groups (Public) → Get Product Variant Groups | 200, variant groups for product | Requires valid `productId` |

**Negative:** Get Product By ID with invalid ID → 404

---

## Scenario 3: Admin — Catalog Management

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Admin Login | 200 | Get `adminToken` |
| 2 | Catalog — Categories → Create Category | 200, `id` | Saves `categoryId` |
| 3 | Catalog — Products → Create Product | 200, `id` | Saves `productId`; use `{{categoryId}}` |
| 4 | Catalog — Variant Groups (Admin) → Create Variant Group | 200, `id` | Saves `variantGroupId` |
| 5 | Catalog — Variant Groups (Admin) → Add Variant Option | 200, `id` | Saves `variantOptionId` |
| 6 | Catalog — Variant Groups (Admin) → Assign Variant Group to Product | 200 | Links variant group to product |
| 7 | Catalog — Products → Update Product | 200 | Modify product |
| 8 | Catalog — Products → Delete Product | 200 | Cleanup (optional) |

---

## Scenario 4: B2B Partner — Cart & Order Flow

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Admin Login | 200 | |
| 2 | Identity — Admin Partners → Create Partner Company | 200, `id` | Saves `partnerId` |
| 3 | Identity — Admin Partners → Approve Partner | 200 | Partner can login after approval |
| 4 | Identity — Auth → Partner Login | 200 | Use admin user email from created partner |
| 5 | Catalog — Products → Search Products | 200 | Get a `productId` |
| 6 | Orders — Partner Cart → Add to Cart | 200, `cartId` | Use `{{productId}}` |
| 7 | Orders — Partner Cart → Get Cart | 200, `items` | Saves `cartItemId` from first item |
| 8 | Orders — Partner Cart → Update Cart Item Quantity | 200 | |
| 9 | Orders — Partner Orders → Create Order (from cart) | 200, `id` | Saves `orderId`; requires delivery address |
| 10 | Orders — Partner Orders → Get Order Details | 200, order with items | |
| 11 | Orders — Partner Orders → Add Comment to Order | 200 | |

**Negative:** Create Order with empty cart → 400

---

## Scenario 5: Admin — Order Management (B2B)

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Admin Login | 200 | |
| 2 | Orders — Admin → Get All Orders | 200, list of orders | |
| 3 | Orders — Admin → Get Order Details (Admin) | 200 | Use `{{orderId}}` |
| 4 | Orders — Admin → Set Order Pricing | 200 | Set `orderItemId` from order details; B2B orders need pricing |
| 5 | Orders — Admin → Update Order Status | 200 | e.g. `newStatus: "Confirmed"` |
| 6 | Orders — Admin → Add Admin Comment | 200 | |
| 7 | Orders — Admin → Update Shipping Info | 200 | Tracking, provider, expected date |

---

## Scenario 6: B2C Guest — Cart & Checkout Flow

**Prerequisite:** `AppMode` must be `B2C` in `appsettings.json`. Products must have `Price` set.

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Catalog — Products → Search Products | 200 | Get `productId`; ensure product has price |
| 2 | Set `guestId` | — | Run any B2C request; pre-request auto-generates UUID |
| 3 | Orders — B2C Cart → Add to B2C Cart | 200, `cartId` | Requires `X-Guest-Id` header |
| 4 | Orders — B2C Cart → Get B2C Cart | 200, `items` | Saves `cartItemId` |
| 5 | Orders — B2C Cart → Update B2C Cart Item Quantity | 200 | |
| 6 | Orders — B2C Checkout → B2C Checkout (Create Order) | 200, `orderId`, `orderNumber`, `totalAmount` | Creates order from cart; cart is cleared |

**Negative:** Get B2C Cart without `X-Guest-Id` → 400  
**Negative:** B2C Checkout with empty cart → 400/500

---

## Scenario 7: B2C — Payment (iyzico)

**Prerequisite:** iyzico sandbox configured in `appsettings.json` (`Iyzico:ApiKey`, `Iyzico:SecretKey`).

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Add items to B2C cart (Scenario 6, steps 1–4) | — | |
| 2 | Orders — B2C Payments → Initialize Payment (iyzico) | 200, `paymentPageUrl`, `token` | Creates order + initializes iyzico; `basketItems` must match cart |
| 3 | Open `paymentPageUrl` in browser | — | Complete payment on iyzico sandbox |
| 4 | Callback | Redirect to success/failure | Handled by iyzico redirect; not directly testable in Postman |

**Note:** The Initialize request creates the order from the guest cart and returns a URL. The callback is invoked by iyzico after the user pays; it updates the order status and redirects the user.

---

## Scenario 8: Content — Blog & Settings

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Content — Blog → Get Published Blog Posts | 200, paginated posts | No auth |
| 2 | Admin Login | 200 | |
| 3 | Content — Blog → Create Blog Post | 200 | |
| 4 | Content — Blog → Get Blog Post by Slug | 200 | Use slug from created post |
| 5 | Content — Pages & Settings → Get All Settings | 200, key-value list | |
| 6 | Content — Pages & Settings → Update Setting | 200 | Admin only; e.g. `Features.Pricing.Enabled` |
| 7 | Content — Pages & Settings → Get Home Sliders | 200, hero, category, brands | B2C/Catalog mode |

---

## Scenario 9: Partner Saved Addresses

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Partner Login | 200 | |
| 2 | Orders — Partner Saved Addresses → Get Saved Addresses | 200, array | May be empty |
| 3 | Orders — Partner Saved Addresses → Create Saved Address | 200 | |
| 4 | Orders — Partner Saved Addresses → Delete Saved Address | 200/204 | Use `savedAddressId` from step 2 |

---

## Scenario 10: Health & Config

| Step | Request | Expected | Notes |
|------|---------|----------|-------|
| 1 | Health Check → Health | 200 | No auth |
| 2 | Config → Get App Mode | 200 | Returns `B2B`, `B2C`, or `Catalog` |

---

## Postman Collection Variables

| Variable | Set By | Used In |
|----------|--------|---------|
| `baseUrl` | Manual | All requests |
| `adminToken` | Admin Login | Admin, Partner management |
| `partnerToken` | Partner Login | Partner cart, orders, addresses |
| `guestId` | Auto-generated (B2C pre-request) | B2C cart, checkout, payments |
| `productId` | Create Product, Search Products | Cart, bundles, variants |
| `categoryId` | Create Category | Create Product |
| `orderId` | Create Order, B2C Checkout, Initialize Payment | Order details, admin actions |
| `partnerId` | Create Partner Company | Partner management |
| `variantGroupId` | Create Variant Group | Variant options, product assignment |
| `variantOptionId` | Add Variant Option | Product variants |
| `cartItemId` | Get Cart, Get B2C Cart | Update quantity, Remove |

---

## Running the Collection

1. Import `Storefront.postman_collection.json` into Postman.
2. Set `baseUrl` if not using `http://localhost:8080`.
3. Run **Config → Get App Mode** to verify backend is up.
4. Run **Identity — Auth → Admin Login** to obtain `adminToken`.
5. Execute scenarios in order; variables are chained (e.g. `productId` from Create Product is used in Add to Cart).

### Collection Runner

To run all requests:

1. Use Collection Runner.
2. Run **Admin Login** and **Partner Login** first (or use a pre-run script to log in).
3. For B2C flows, ensure `AppMode=B2C` and products have prices.

---

## Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| 401 on Partner Cart | Partner not approved or wrong credentials | Approve partner via Admin Partners → Approve |
| 400 on B2C Cart | Missing `X-Guest-Id` | B2C folder pre-request auto-sets `guestId`; run any B2C request first |
| 400 on B2C Checkout | Empty cart | Add items to B2C cart before checkout |
| 404 on Get Product | Invalid `productId` | Run Search Products, use returned `id` |
| 500 on B2C Checkout | Product has no price | Set `Price` on product in admin; B2C requires pricing |
