# End-to-End Testing Scenarios

## Prerequisites

```bash
# Terminal 1 — Database
docker compose up -d

# Terminal 2 — Backend (wait for "Application started" log)
cd src/API/Storefront.Api && dotnet run

# Terminal 3 — Frontend
cd web && npm run dev
```

- Backend: http://localhost:8080 (Swagger: http://localhost:8080/swagger)
- Frontend: http://localhost:3000

---

## 1. Admin Flow

### 1.1 Admin Login
- URL: `http://localhost:3000/login`
- Credentials: `admin@storefront.com` / `AdminPassword123!`
- Expected: Redirect to `/admin/dashboard` with real stats

### 1.2 Create a Category
- Navigate: Admin → Categories → New Category
- Fill: Name, Description
- Expected: Category created, appears in list

### 1.3 Create a Product
- Navigate: Admin → Products → New Product
- Fill: Name, SKU, Description, assign to category
- Note: Price and inventory fields are optional (pricing is disabled by default)
- Expected: Product created, appears in list

### 1.4 Create a Color Chart
- Navigate: Admin → Color Charts → Create New Chart
- Fill: Name, Code, Type (e.g., "Fabric"), Description
- Expected: Chart created, opens detail page

### 1.5 Add Color Options to Chart
- On the chart detail page, click "Add Color"
- Add 3-4 colors with Name, Code, and Hex Color
- Expected: Colors appear as colored cards in the grid
- Test: Hover a color card → menu appears → Edit and Delete should work

### 1.6 Assign Color Chart to Product
- Navigate: Admin → Products → click a product to edit
- Right panel: "Color & Material Options" card
- Click "Assign Chart" → select the chart → toggle "Required selection"
- Expected: Chart appears in the assigned list with color count

### 1.7 Create a Partner Company
- Navigate: Admin → Partners → New Partner
- Fill: Company name, tax ID, email, phone, address, admin user email/password
- Expected: Partner created with status "Active"

### 1.8 Check Settings
- Navigate: Admin → Settings
- Verify: "Enable Pricing" should be OFF by default
- Verify: "Enable Public Storefront" should be OFF by default
- Toggle features on/off
- Click Save
- Expected: Toast "Settings saved successfully"

---

## 2. Partner Flow

### 2.1 Partner Login
- URL: `http://localhost:3000/partner/login`
- Use the partner credentials created in step 1.7
- Expected: Redirect to `/partner/dashboard` with stats

### 2.2 Browse Products
- Navigate to `/products` (or use "Browse Products" from orders page)
- Click on a product that has a color chart assigned
- Expected: Product detail page shows color selection UI, quantity picker, and "Add to Cart" button
- Note: No prices should be visible (pricing is disabled)
- If not logged in as partner: should show "Partner login required" message

### 2.3 Add to Cart
- On product detail page:
  - Expand a color chart → select a color
  - Set quantity to 2
  - Optionally add customization notes
  - Click "Add to Cart"
- Expected: Toast "Added to cart" with "View Cart" action link

### 2.4 Cart Management
- Navigate: `/partner/cart`
- Expected: See the item with selected color, quantity
- Test: Change quantity → should update
- Test: Remove item → should disappear
- Add another product, then proceed

### 2.5 Checkout / Create Order
- From cart, click "Proceed to Checkout"
- Fill delivery address (address, city, state, postal code, country)
- Add optional delivery notes and requested delivery date
- Click "Place Order"
- Expected: Order created, redirect to order details

### 2.6 View Order Details
- Navigate: Partner → Orders → click the order
- Expected: See order timeline, items with color info, delivery address
- Check: Status should be "Pending"
- Check: No pricing/total section should be visible

### 2.7 Add Comment
- On order detail page, scroll to comments section
- Type a message and submit
- Expected: Comment appears in the thread

### 2.8 Cancel Order (optional)
- On a Pending order, click "Cancel Order"
- Enter a reason
- Expected: Order status changes to "Cancelled"

### 2.9 Profile Page
- Navigate: `/partner/profile`
- Expected: Shows personal info, company info, password change section

---

## 3. Admin Order Management

### 3.1 View Orders
- Navigate: Admin → Orders
- Expected: See the order(s) created by the partner

### 3.2 Verify No Pricing UI
- Click an order to view details
- Expected: No "Set Pricing" button visible (pricing feature is disabled)
- Expected: No pricing/total card in the sidebar

### 3.3 Update Status
- Click "Update Status"
- Change: Pending → Confirmed → InProduction → ReadyToShip
- Expected: Status badge updates, timeline reflects change

### 3.4 Add Shipping Info
- Click "Shipping" button
- Fill: Carrier (e.g., "DHL"), Tracking Number, Expected Delivery Date
- Save
- Expected: Shipping info saved, status may change to "Shipping"

### 3.5 Add Admin Comment
- Scroll to comments section
- Add a comment (can be internal or visible to partner)
- Expected: Comment appears, internal comments should be marked

### 3.6 Verify Partner Sees Updates
- Switch to partner login
- Open the same order
- Expected: Partner sees updated status, shipping info, and non-internal admin comments

---

## 4. Admin Panel Other Pages

### 4.1 Dashboard
- Expected: Real stats (total products, orders, pending/active/completed counts)

### 4.2 Users Page
- Admin → Users
- Expected: Shows admin users from the database

### 4.3 Category Edit
- Admin → Categories → click a category
- Edit name/description, save
- Expected: Changes persisted

### 4.4 Product Delete
- Admin → Products → delete a product
- Expected: Product removed from list

### 4.5 Color Chart — Edit/Delete Options
- Admin → Color Charts → click a chart
- Hover a color → menu → Edit → change name/hex → Save
- Hover another → Delete → confirm
- Expected: Changes reflected immediately

---

## 5. Feature Flags

### 5.1 Public Storefront Toggle
- Admin → Settings → verify "Enable Public Storefront" is OFF
- Visit `http://localhost:3000/` (logged out)
- Expected: "Partner Access Only" page with redirect to partner login
- Enable the toggle, save, revisit `/`
- Expected: Public storefront with header, products, etc.

### 5.2 Blog Toggle
- Admin → Settings → enable/disable "Enable Blog"
- Check public header navigation (when storefront is enabled)
- Expected: "Blog" link appears/disappears

### 5.3 Pricing Toggle
- Admin → Settings → enable "Enable Pricing", save
- Go to Admin → Orders → click an order
- Expected: "Set Pricing" button now visible, pricing sidebar card appears
- Disable "Enable Pricing", save, refresh
- Expected: "Set Pricing" button and pricing card gone
