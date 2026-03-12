Read the CLAUDE.md file at the project root to understand the full backend architecture, API endpoints, authentication, and conventions.

## Task: Build a React Native (Expo) Mobile App — Partner Portal

Create a mobile app in a new `mobile/` directory at the project root. This app is the Partner Portal — it allows business partners to browse products, manage their cart, place order requests, and track orders. There is NO admin functionality in the mobile app.

### Tech Stack
- React Native with Expo (managed workflow)
- TypeScript (strict)
- Expo Router for file-based navigation
- TanStack Query v5 for data fetching/caching
- AsyncStorage for token persistence
- axios for HTTP requests
- Styling: NativeWind (Tailwind for React Native) OR plain StyleSheet — your choice based on what's cleaner

### Setup

Run this to create the project:

npx create-expo-app mobile --template blank-typescript
cd mobile
npx expo install expo-router expo-secure-store @tanstack/react-query axios

### Backend API (already running)
The .NET backend runs at http://localhost:8080. Use environment config so the base URL is configurable.

### Authentication
- Login: POST /api/identity/partners/auth/login with { email, password }
- Response includes accessToken, refreshToken, and user object with { id, email, firstName, lastName, role, company: { id, name, status } }
- Store accessToken in SecureStore. Attach as Authorization: Bearer <token> header on all subsequent requests.
- The JWT contains claims: sub (user ID), companyId, companyName, role, type: "Partner"

### Screens to Build

#### 1. Login Screen
- Email + password form
- "Sign In" button with loading state
- Show API error messages (invalid credentials, account suspended, etc.)
- On success: store token + user info, navigate to Home

#### 2. Home / Dashboard
- Welcome message with partner name + company name
- Stats cards: Total Orders, Pending, Active, Completed (from GET /api/partner/orders/stats)
- Quick action buttons: Browse Products, View Cart, My Orders
- Recent orders list (last 5)

#### 3. Products List
- Grid/list of products from GET /api/catalog/products?pageNumber=1&pageSize=20
- Search bar (uses ?searchTerm= query param)
- Category filter (categories from GET /api/catalog/categories)
- Product card: image, name, SKU, category
- Pull-to-refresh
- No prices shown (pricing is disabled)

#### 4. Product Detail
- Product images, name, SKU, description, specifications
- Color Chart Selection: Fetch from GET /api/products/{productId}/color-charts
  - Each chart has colorOptions array with id, name, code, hexColor
  - Show color swatches the partner can tap to select
  - Required charts must have a selection before adding to cart
- Quantity picker (min 1)
- Optional customization notes text input
- "Add to Cart" button calls POST /api/partner/cart/items with body:
  productId, productName, productSKU, productImageUrl, quantity, colorChartId, colorChartName, colorOptionId, colorOptionName, colorOptionCode, customizationNotes (optional)

#### 5. Cart Screen
- List of cart items from GET /api/partner/cart
- Each item shows: product name, SKU, color selection, quantity
- Update quantity: PATCH /api/partner/cart/items/{itemId} with { quantity }
- Remove item: DELETE /api/partner/cart/items/{itemId}
- "Proceed to Checkout" button (disabled if cart empty)

#### 6. Checkout Screen
- Delivery address form: Address, City, State, Postal Code, Country
- Optional: Delivery Notes, Requested Delivery Date (date picker)
- Optional: Order Notes
- "Place Order" button calls POST /api/partner/orders with: deliveryAddress, deliveryCity, deliveryState, deliveryPostalCode, deliveryCountry, deliveryNotes (optional), requestedDeliveryDate (optional), notes (optional)
- On success: navigate to order details

#### 7. Orders List
- List from GET /api/partner/orders?pageNumber=1&pageSize=20
- Filter tabs: All, Pending, Active (Confirmed/InProduction/ReadyToShip/Shipping), Completed, Cancelled
- Each row: order number, status badge (color-coded), item count, date
- Tap to open order details

#### 8. Order Detail
- Order number, status badge, timeline/progress indicator
- Items list with color info
- Delivery address
- Shipping info (tracking number, carrier) — if available
- Comments thread with ability to add new comment: POST /api/partner/orders/{id}/comments with { content, type: "General" }
- Cancel Order button (only visible when status is "Pending" or "QuoteSent"): POST /api/partner/orders/{id}/cancel with { reason }

#### 9. Profile Screen
- Fetch from GET /api/identity/partners/profile
- Show: name, email, phone, role, company details
- Logout button (clear tokens, navigate to login)

### Order Statuses
Map these to colors/labels:
- Pending → Yellow — "Pending Review"
- QuoteSent → Blue — "Quote Sent"
- Confirmed → Green — "Confirmed"
- InProduction → Purple — "In Production"
- ReadyToShip → Indigo — "Ready to Ship"
- Shipping → Orange — "Shipping"
- Delivered → Green — "Delivered"
- Completed → Gray — "Completed"
- Cancelled → Red — "Cancelled"

### Image Handling
Product images come as relative paths like /uploads/products/xxx/image.webp. Prepend the API base URL: http://localhost:8080/uploads/...

### Important Notes
- This is a B2B app — professional, clean UI. No playful design.
- Pricing is disabled. Don't show any prices anywhere.
- The partner does NOT create their own account — admin creates them via the web panel.
- Handle loading states, error states, empty states for all screens.
- Handle token expiration gracefully (401 → redirect to login).
- The app should work on both iOS and Android.
- Use a tab navigator for the main screens: Home, Products, Cart, Orders, Profile

### File Structure Suggestion

mobile/
  app/                    — Expo Router pages
    (auth)/
      login.tsx
    (tabs)/
      index.tsx           — Home/Dashboard
      products/
        index.tsx         — Products list
        [id].tsx          — Product detail
      cart.tsx
      orders/
        index.tsx         — Orders list
        [id].tsx          — Order detail
      profile.tsx
    checkout.tsx
    _layout.tsx
  components/
    ui/                   — Reusable components (Button, Badge, Card)
    orders/               — Order-specific components
    products/             — Product-specific components
  lib/
    api.ts                — Axios instance + auth interceptor
    api/                  — API clients (catalog.ts, orders.ts, auth.ts)
    types.ts              — Shared TypeScript interfaces
    auth.ts               — Auth context/provider
  constants/
    colors.ts             — Color palette

After creating the app, provide instructions on how to run it with npx expo start and test it with Expo Go on a physical device or simulator.
