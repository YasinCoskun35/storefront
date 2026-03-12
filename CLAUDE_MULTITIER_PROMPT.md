# Storefront — Multi-Tier Product Architecture Implementation

## Project Context

This is a .NET 10 + Next.js 15 modular monolith platform currently built as a B2B furniture manufacturer portal. Partners (businesses) browse a catalog, build a cart, submit order requests. Admin manages products, partners, and processes orders. There is also a React Native (Expo) mobile app for B2B partners.

The project needs to be restructured so the same codebase can be deployed as three distinct product tiers via configuration. This is a white-label product — each customer gets their own deployed instance with different environment variables. We do NOT want separate repositories.

## Current State

Tech Stack:
- Backend: .NET 10, C# 14, PostgreSQL 16, EF Core 10, MediatR, FluentValidation, JWT auth
- Frontend: Next.js 15 (App Router), React 19, TypeScript, Tailwind CSS, shadcn/ui, TanStack Query v5
- Mobile: React Native (Expo), for B2B partner portal only
- Infrastructure: Docker Compose (dev), Nginx (prod)

Backend Modules (each has its own PostgreSQL schema, DbContext, and Clean Architecture layers):
- Identity (schema: identity) — Admin users, partner companies, partner users, JWT auth
- Catalog (schema: catalog) — Products (simple + bundle), categories, brands, images
- Content (schema: content) — Blog posts, static pages, AppSettings (feature flags)
- Orders (schema: orders) — Color charts, color options, product-color-chart assignments, carts, cart items, orders, order items, order comments, saved addresses

Frontend Routes:
- /admin/* — Admin panel (protected, requires admin JWT)
- /partner/* — Partner portal (protected, requires partner JWT)
- / — Public storefront (toggleable via feature flag)
- /products/* — Public product catalog

Existing Feature Flags (stored in content."AppSettings" table, managed via admin settings page):
- Features.Blog.Enabled (default: false)
- Features.Pricing.Enabled (default: false) — hides/shows prices and order pricing UI
- Features.PublicCatalog.Enabled (default: true)
- Features.PublicStorefront.Enabled (default: false) — when off, redirects public routes to partner login

Key Files:
- src/API/Storefront.Api/Program.cs — Module registration, middleware pipeline
- src/API/Storefront.Api/appsettings.json — Configuration (DB, JWT, CatalogSettings)
- src/API/Storefront.Api/Extensions/DatabaseExtensions.cs — Auto-creates schemas, tables, seeds settings on startup
- web/src/lib/api.ts — Shared Axios instance with JWT interceptor
- web/src/lib/api/settings.ts — Settings API client with isFeatureEnabled() helper
- web/src/components/layout/conditional-public-layout.tsx — Mode-aware public layout
- CLAUDE.md — Full project rules and conventions (READ THIS FIRST)

## IMPORTANT: Color Charts to Product Variants Migration

The current codebase has a "Color Charts" system in the Orders module (ColorChart, ColorOption, ProductColorChart entities). This was a rigid, domain-specific implementation that only handled colors for sofa manufacturers.

This MUST be replaced with a generic Product Variants system. Color charts are just one type of product variant. The new system should handle any variant type: colors, sizes, materials, finishes, or any custom attribute the admin defines.

What to do:
1. DELETE all color chart entities, commands, queries, controllers, and frontend pages from the Orders module
2. DELETE the color chart tables from DatabaseExtensions.cs (orders."ColorCharts", orders."ColorOptions", orders."ProductColorCharts")
3. BUILD the new Product Variants system in the Catalog module (where it logically belongs)
4. UPDATE cart items and order items to reference variant selections instead of color fields
5. UPDATE the admin and partner frontend to use variants instead of color charts
6. UPDATE the mobile app API clients to use the new variant endpoints

The new Product Variants system design is described in detail below under "Product Variants System".

## Three Product Tiers

### Tier 1: Catalog (Showcase Only)
- Product catalog website with NO pricing, NO cart, NO orders, NO partners
- Admin panel: manage products, categories, content, variants only
- Public-facing: browse products, view details with variant options displayed, contact info
- Use case: Any manufacturer who just wants a digital catalog

### Tier 2: B2B (Current Model — Quote-to-Order)
- Partners (businesses) browse catalog, select product variants, build cart, submit order requests
- Admin processes orders, manages partners, pricing (optional)
- No public storefront (partners log in directly)
- Mobile app for partners
- Use case: Manufacturer selling to dealers/retailers

### Tier 3: B2C (Retail E-Commerce)
- Individual customers register, browse, select variants, add to cart, checkout with online payment
- Full pricing required, public storefront enabled
- Admin manages customers, orders, payments, refunds
- No partner portal
- Use case: Direct-to-consumer retail

### Feature Matrix by Tier

- Product catalog: Catalog YES, B2B YES, B2C YES
- Product variants: Catalog YES (display only), B2B YES (selectable), B2C YES (selectable with price adjustments)
- Pricing display: Catalog NO, B2B OPTIONAL, B2C YES (required)
- Partner portal: Catalog NO, B2B YES, B2C NO
- Customer accounts: Catalog NO, B2B NO, B2C YES
- Cart: Catalog NO, B2B YES (quote), B2C YES (purchase)
- Online payment (Stripe): Catalog NO, B2B NO, B2C YES
- Blog/Content: Catalog OPTIONAL, B2B OPTIONAL, B2C OPTIONAL
- Admin panel: Catalog BASIC, B2B FULL, B2C FULL
- Mobile app: Catalog NO, B2B YES, B2C FUTURE

## Product Variants System (Replaces Color Charts)

### Concept

A VariantGroup is a category of customization (e.g., "Upholstery Color", "Frame Size", "Wood Type", "Leg Finish"). Each VariantGroup contains VariantOptions (e.g., "Navy Blue", "3-Seater", "Walnut", "Chrome").

Admin creates variant groups and their options globally. Then assigns relevant variant groups to specific products. When a partner/customer adds a product to cart, they select one option from each assigned variant group.

Each variant option can have an optional price adjustment (+/- from the base product price). There is no separate inventory per variant — stock is tracked at the product level only.

### Backend Entities (in the Catalog module, schema: catalog)

VariantGroup:
- Id (string, PK)
- Name (string, required) — e.g., "Upholstery Color", "Frame Size"
- Code (string, unique) — e.g., "upholstery-color", "frame-size"
- Description (string)
- DisplayType (enum: Swatch, Dropdown, RadioButtons, ImageGrid) — controls how options are rendered on the frontend
- MainImageUrl (string, nullable) — optional reference image for the group
- ThumbnailUrl (string, nullable)
- IsActive (bool, default true)
- DisplayOrder (int, default 0)
- CreatedAt (DateTime)
- UpdatedAt (DateTime, nullable)
- CreatedBy (string)

VariantOption:
- Id (string, PK)
- VariantGroupId (string, FK to VariantGroup)
- Name (string, required) — e.g., "Navy Blue", "3-Seater"
- Code (string, unique within group) — e.g., "navy-blue", "3-seater"
- HexColor (string, nullable) — only for color-type variants, e.g., "#1B3A5C"
- ImageUrl (string, nullable) — swatch or sample image
- IsAvailable (bool, default true)
- PriceAdjustment (decimal, nullable) — +/- from product base price, e.g., +50.00 or -10.00
- DisplayOrder (int, default 0)
- CreatedAt (DateTime)

ProductVariantGroup:
- Id (string, PK)
- ProductId (string, indexed, NOT a FK since products are in catalog schema and this will also be in catalog schema)
- VariantGroupId (string, FK to VariantGroup)
- IsRequired (bool, default true) — must the customer select an option?
- AllowMultiple (bool, default false) — can customer select more than one option?
- DisplayOrder (int, default 0)
- CreatedAt (DateTime)
- Unique constraint on (ProductId, VariantGroupId)

### Backend Commands and Queries (Catalog module)

Commands:
- CreateVariantGroupCommand(Name, Code, Description, DisplayType, MainImageUrl, ThumbnailUrl)
- UpdateVariantGroupCommand(Id, Name, Code, Description, DisplayType, MainImageUrl, ThumbnailUrl, IsActive)
- DeleteVariantGroupCommand(Id) — fails if assigned to any product
- AddVariantOptionCommand(VariantGroupId, Name, Code, HexColor, ImageUrl, PriceAdjustment, DisplayOrder)
- UpdateVariantOptionCommand(VariantGroupId, OptionId, Name, Code, HexColor, ImageUrl, IsAvailable, PriceAdjustment, DisplayOrder)
- DeleteVariantOptionCommand(VariantGroupId, OptionId)
- AssignVariantGroupToProductCommand(ProductId, VariantGroupId, IsRequired, AllowMultiple, DisplayOrder)
- RemoveVariantGroupFromProductCommand(ProductId, VariantGroupId)

Queries:
- GetAllVariantGroupsQuery — admin list with option counts
- GetVariantGroupByIdQuery(Id) — full detail with all options
- GetProductVariantGroupsQuery(ProductId) — all variant groups assigned to a product, with their available options. Used by admin product edit page, partner add-to-cart, and public product page.

### Backend Controllers (Catalog module)

AdminVariantGroupsController (route: /api/admin/variant-groups, [Authorize(Roles = "Admin")]):
- GET / — list all variant groups
- GET /{id} — get variant group with options
- POST / — create variant group
- PUT /{id} — update variant group
- DELETE /{id} — delete variant group
- POST /{id}/options — add option
- PUT /{id}/options/{optionId} — update option
- DELETE /{id}/options/{optionId} — delete option
- GET /product/{productId} — get variant groups assigned to a product
- POST /product/{productId} — assign variant group to product
- DELETE /product/{productId}/{variantGroupId} — remove variant group from product

PublicVariantGroupsController (route: /api/products/{productId}/variants, no auth):
- GET / — get variant groups assigned to a product (only active groups and available options)

### Cart and Order Item Changes (Orders module)

Remove from CartItem entity:
- ColorChartId, ColorChartName, ColorOptionId, ColorOptionName, ColorOptionCode

Add to CartItem entity:
- SelectedVariants (string, JSON) — serialized array of selected variant options

The SelectedVariants JSON structure:

    [
      {
        "variantGroupId": "vg-1",
        "variantGroupName": "Upholstery Color",
        "variantOptionId": "vo-1",
        "variantOptionName": "Navy Blue",
        "variantOptionCode": "navy-blue",
        "priceAdjustment": 50.00
      },
      {
        "variantGroupId": "vg-2",
        "variantGroupName": "Frame Size",
        "variantOptionId": "vo-5",
        "variantOptionName": "3-Seater",
        "variantOptionCode": "3-seater",
        "priceAdjustment": 200.00
      }
    ]

Apply same changes to OrderItem entity — remove color fields, add SelectedVariants JSON column.

Update AddToCartCommandHandler to accept variant selections instead of color chart/option fields.
Update CreateOrderCommandHandler to copy variant selections from cart items to order items.

### Frontend Changes for Variants

Admin pages:
- Replace /admin/color-charts with /admin/variant-groups — list page showing all variant groups with option counts
- Replace /admin/color-charts/[id] with /admin/variant-groups/[id] — detail page to manage options (add, edit, delete, toggle availability)
- Update /admin/products/[id] page — replace the ProductColorCharts component with a ProductVariantGroups component that allows assigning/removing variant groups

Partner/Customer product interaction:
- Update the AddToCartSection component — instead of a single color picker, render each assigned variant group according to its DisplayType:
  - Swatch: color circles (using HexColor) or small image thumbnails
  - Dropdown: select element
  - RadioButtons: radio button group
  - ImageGrid: grid of clickable images
- Show price adjustment next to each option if pricing is enabled (e.g., "Navy Blue (+$50)")

API clients:
- Create web/src/lib/api/variants.ts — replaces color chart API calls
- Update web/src/lib/api/orders.ts — update cart/order types to use selectedVariants instead of color fields

Mobile app:
- Update mobile/lib/api/ to use new variant endpoints instead of color chart endpoints
- Update product detail screen to render variant selection UI
- Update cart types to use selectedVariants

### Database Migration (DatabaseExtensions.cs)

Remove these table creation statements:
- orders."ColorCharts"
- orders."ColorOptions"
- orders."ProductColorCharts"

Add these table creation statements to the Catalog schema section:
- catalog."VariantGroups"
- catalog."VariantOptions"
- catalog."ProductVariantGroups"

Modify these existing tables:
- orders."CartItems" — drop ColorChartId, ColorChartName, ColorOptionId, ColorOptionName, ColorOptionCode columns; add SelectedVariants text column
- orders."OrderItems" — drop ColorChartId, ColorChartName, ColorOptionId, ColorOptionName, ColorOptionCode, ColorOptionImageUrl columns; add SelectedVariants text column

## Implementation Plan

### Phase 1: Make the Existing Codebase Mode-Aware and Replace Color Charts with Variants

1.1 Backend — AppMode Configuration

Add to appsettings.json:

    {
      "AppMode": "B2B"
    }

Create an AppMode enum (Catalog, B2B, B2C) in the Shared project.

Make module registration in Program.cs conditional:

    builder.Services.AddIdentityModule(builder.Configuration);
    builder.Services.AddCatalogModule(builder.Configuration);
    builder.Services.AddContentModule(builder.Configuration);

    var appMode = builder.Configuration.GetValue<string>("AppMode") ?? "B2B";
    if (appMode is "B2B" or "B2C")
        builder.Services.AddOrdersModule(builder.Configuration);

Add an API endpoint GET /api/config/mode that returns the current AppMode (public, no auth) so the frontend knows what tier it is running as.

Update DatabaseExtensions.cs to only create schemas/tables for registered modules based on AppMode.

Seed different default feature flag values based on AppMode:
- Catalog mode: Features.Pricing.Enabled=false, Features.PublicStorefront.Enabled=true, Features.PublicCatalog.Enabled=true
- B2B mode: Features.Pricing.Enabled=false, Features.PublicStorefront.Enabled=false, Features.PublicCatalog.Enabled=true
- B2C mode: Features.Pricing.Enabled=true, Features.PublicStorefront.Enabled=true, Features.PublicCatalog.Enabled=true

1.2 Backend — Replace Color Charts with Product Variants

Follow the detailed design above under "Product Variants System":
1. Delete all ColorChart, ColorOption, ProductColorChart entities, commands, queries, controllers from the Orders module
2. Create VariantGroup, VariantOption, ProductVariantGroup entities in the Catalog module
3. Add DbSets and configurations to CatalogDbContext
4. Create all commands, queries, and controllers as specified above
5. Update CartItem and OrderItem entities in the Orders module to use SelectedVariants JSON column
6. Update AddToCartCommandHandler and CreateOrderCommandHandler
7. Update DatabaseExtensions.cs — remove color chart tables, add variant tables, modify cart/order item tables
8. Register MediatR handlers for the new variant commands and queries

1.3 Frontend — Mode-Aware Configuration

Add to web/.env.local:

    NEXT_PUBLIC_APP_MODE=B2B

Create web/src/lib/app-mode.ts:

    export type AppMode = 'Catalog' | 'B2B' | 'B2C';
    export const appMode: AppMode = (process.env.NEXT_PUBLIC_APP_MODE as AppMode) || 'B2B';

    export const features = {
      hasPricing: appMode === 'B2C',
      hasPartnerPortal: appMode === 'B2B',
      hasCustomerAccounts: appMode === 'B2C',
      hasPayments: appMode === 'B2C',
      hasCart: appMode !== 'Catalog',
      hasVariants: true,
      hasPublicStorefront: appMode === 'Catalog' || appMode === 'B2C',
    };

Update admin sidebar in web/src/app/admin/layout.tsx to conditionally show menu items:
- Catalog mode: Products, Categories, Variant Groups, Content, Settings
- B2B mode: + Partners, Orders
- B2C mode: + Customers (future), Orders, Payments (future) — no Partners

Create web/src/middleware.ts for route protection:
- Catalog mode: block /partner/*, /checkout
- B2B mode: allow /partner/*, block /register, /account/*
- B2C mode: block /partner/*, allow /register, /account/*, /checkout

1.4 Frontend — Replace Color Chart UI with Variants UI

1. Delete web/src/app/admin/color-charts/ directory entirely
2. Create web/src/app/admin/variant-groups/page.tsx — list page
3. Create web/src/app/admin/variant-groups/[id]/page.tsx — detail page with option management
4. Create web/src/lib/api/variants.ts — API client for variant CRUD
5. Replace web/src/components/admin/product-color-charts.tsx with product-variant-groups.tsx
6. Update web/src/app/admin/products/[id]/page.tsx to use the new component
7. Update web/src/components/products/add-to-cart-section.tsx to render variant selection UI based on DisplayType
8. Update web/src/lib/api/orders.ts — remove color chart types, add variant selection types
9. Update cart and order detail pages to display selected variants instead of color info

1.5 Deployment Templates

Create separate docker-compose override files:
- docker-compose.yml — Base (PostgreSQL only)
- docker-compose.catalog.yml — Adds APP_MODE=Catalog env vars
- docker-compose.b2b.yml — Adds APP_MODE=B2B env vars
- docker-compose.b2c.yml — Adds APP_MODE=B2C env vars

### Phase 2: Build B2C Capabilities (New modules and frontend)

2.1 Customers Module (Backend)

Create src/Modules/Customers/Storefront.Modules.Customers/ following the existing module structure:
- Schema: customers
- Entities: Customer (id, email, passwordHash, firstName, lastName, phone, createdAt, isActive), CustomerAddress (id, customerId, label, address, city, state, postalCode, country, isDefault)
- Commands: RegisterCustomerCommand, CustomerLoginCommand, UpdateCustomerProfileCommand, AddCustomerAddressCommand, UpdateCustomerAddressCommand, DeleteCustomerAddressCommand
- Queries: GetCustomerProfileQuery, GetCustomerAddressesQuery, GetAllCustomersQuery (admin)
- Controller: CustomerAuthController (POST /api/customers/auth/register, POST /api/customers/auth/login), CustomerProfileController (GET/PUT /api/customers/profile, GET/POST/PUT/DELETE /api/customers/addresses), AdminCustomersController (GET /api/admin/customers, GET /api/admin/customers/{id})
- JWT: Include role: "Customer" claim, use localStorage.customer_access_token on frontend
- This is SEPARATE from the Identity module partner system — customers are individuals, partners are businesses

2.2 Payments Module (Backend)

Create src/Modules/Payments/Storefront.Modules.Payments/ following the existing module structure:
- Schema: payments
- Entities: PaymentTransaction (id, orderId, amount, currency, provider, providerTransactionId, status, metadata, createdAt), Refund (id, paymentTransactionId, amount, reason, status, createdAt)
- Integration: Start with Stripe (use Stripe.net NuGet package). Make provider configurable.
- Commands: ProcessPaymentCommand, CreateRefundCommand, HandleWebhookCommand
- Queries: GetPaymentsByOrderQuery, GetAllTransactionsQuery (admin)
- Controller: PaymentsController (POST /api/payments/process, POST /api/payments/webhook), AdminPaymentsController (GET /api/admin/payments, POST /api/admin/payments/{id}/refund)
- Add Stripe keys to appsettings.json under a Payments section

2.3 Extend Orders Module for B2C

The existing Orders module handles B2B quote flow. For B2C, add a parallel flow:
- B2B flow: Cart -> Order Request -> Admin Reviews -> Quotes Price -> Partner Confirms -> Preparing -> Shipping -> Delivered
- B2C flow: Cart -> Checkout -> Payment Processed -> Auto-Confirmed -> Preparing -> Shipping -> Delivered

Add OrderType enum to the Order entity: QuoteRequest (B2B) or DirectPurchase (B2C).
- In CreateOrderCommandHandler, branch logic based on OrderType
- B2C orders require pricing at creation time (cart items must have prices)
- B2C orders auto-transition to Confirmed after successful payment
- B2C cart uses CustomerId instead of PartnerUserId/PartnerCompanyId

2.4 B2C Frontend Pages

Create these new pages:
- web/src/app/register/page.tsx — Customer registration form
- web/src/app/account/page.tsx — Customer profile and order history
- web/src/app/account/addresses/page.tsx — Address management
- web/src/app/account/orders/page.tsx — Order list
- web/src/app/account/orders/[id]/page.tsx — Order detail
- web/src/app/cart/page.tsx — Public cart (different from partner cart at /partner/cart)
- web/src/app/checkout/page.tsx — Checkout with Stripe payment form
- web/src/app/admin/customers/page.tsx — Admin customer list
- web/src/app/admin/customers/[id]/page.tsx — Admin customer detail
- web/src/app/admin/payments/page.tsx — Admin payment/transaction list

Create API clients:
- web/src/lib/api/customers.ts — Customer auth, profile, addresses
- web/src/lib/api/payments.ts — Payment processing

Update the public product detail page (web/src/app/products/[id]/page.tsx):
- In B2C mode: Show "Add to Cart" button (with price, including variant price adjustments)
- In B2B mode: Show the existing partner add-to-cart section
- In Catalog mode: Show "Contact Us" or "Request Quote" button

### Phase 3: White-Label Branding and Documentation

- Make site name, logo URL, primary colors configurable via AppSettings
- Add Site.LogoUrl, Site.PrimaryColor, Site.FooterText to seeded settings
- Update the frontend header, footer, and login pages to use these settings
- Update CLAUDE.md, README.md, and all documentation to reflect multi-tier architecture and variants system
- Create a deployment guide for each tier

## Architecture Rules

- ALWAYS read CLAUDE.md first — it has the full coding conventions
- Modules never import from each other namespaces
- Each module has its own PostgreSQL schema and DbContext
- All business logic in command/query handlers, never in controllers or React components
- Use the Result pattern (Result<T>) — never throw for business errors
- Frontend uses TanStack Query v5 for data fetching, sonner for toasts
- Use shadcn/ui components — never build from scratch if one exists
- The AppMode configuration is read at startup and does NOT change at runtime
- Feature flags (AppSettings) CAN be changed at runtime via admin UI
- New tables must be added both to the module DbContext AND to DatabaseExtensions.cs
- Product variants live in the Catalog module, NOT the Orders module

## What NOT to Build

- Multi-tenancy (each deployment is single-tenant)
- Microservices (keep the modular monolith)
- GraphQL (REST only)
- SSR for admin/partner pages (client-side only, behind auth)
- B2C mobile app (future, not now)
- Per-variant inventory tracking (stock is at the product level only)
- Full SKU-per-variant-combination system (variants are additive price adjustments only)

## Running the Project

    # Start database
    docker-compose up -d

    # Start backend (auto-migrates on startup)
    cd src/API/Storefront.Api && dotnet run

    # Start frontend
    cd web && npm run dev

Ports: Backend http://localhost:8080, Frontend http://localhost:3000, PostgreSQL localhost:5432

Admin credentials: admin@storefront.com / AdminPassword123!

## Git Workflow

Use branch-per-phase with logical chunk commits. This gives clean rollback points and a professional history.

Branch strategy:
- main — stable, working code (current state)
- phase/1-mode-aware-and-variants — Phase 1 work (branched from main)
- phase/2-b2c — Phase 2 work (branched from main after Phase 1 is merged)
- phase/3-branding — Phase 3 work (branched from main after Phase 2 is merged)

Before starting any work:
1. Make sure you are on main and everything is committed
2. Create the phase branch: git checkout -b phase/1-mode-aware-and-variants

Commit frequency — commit after each complete logical chunk that builds and works:
- "Add AppMode enum and conditional module registration"
- "Add GET /api/config/mode endpoint"
- "Delete color chart entities and controllers from Orders module"
- "Add VariantGroup and VariantOption entities to Catalog module"
- "Add variant management commands, queries, and controllers"
- "Update CartItem and OrderItem to use SelectedVariants JSON"
- "Update DatabaseExtensions for variant tables and schema changes"
- "Add frontend app-mode configuration and middleware"
- "Replace admin color charts pages with variant groups pages"
- "Update add-to-cart section for variant selection UI"
- "Add docker-compose tier override files"

Each commit message should be a short imperative sentence describing what was done. Do NOT squash — keep the granular history.

After completing a phase:
1. Verify everything builds and works: dotnet build, npx tsc --noEmit, npx next lint
2. Commit any remaining changes
3. Switch to main and merge: git checkout main && git merge phase/1-mode-aware-and-variants
4. Tag the milestone: git tag v1.0-phase1
5. Create the next phase branch: git checkout -b phase/2-b2c

## Execution Order

1. Start with Phase 1 — make everything configurable AND replace color charts with variants. This is the biggest phase. Test that existing B2B functionality still works with AppMode=B2B using the new variants system, and that AppMode=Catalog correctly hides B2B features.
2. Then Phase 2.1 (Customers module) and 2.4 (B2C frontend registration/profile).
3. Then Phase 2.3 (extend Orders for B2C cart/checkout flow).
4. Then Phase 2.2 (Payments module with Stripe).
5. Finally Phase 3 (branding, docs).

IMPORTANT: You MUST drop and recreate the database when starting Phase 1 since the schema changes are breaking (color chart tables removed, variant tables added, cart/order item columns changed). Run: docker-compose down -v && docker-compose up -d to reset the database before starting.

After each phase, verify: dotnet build, npx tsc --noEmit, npx next lint, and manual testing.
