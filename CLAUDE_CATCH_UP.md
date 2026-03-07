# Claude Catch-Up / Session Handoff

> Use this document to quickly understand the current state of the Storefront project and recent changes. Read alongside `CLAUDE_MULTITIER_PROMPT.md` for full architecture context.

---

## Project Summary

**Storefront** is a .NET 10 + Next.js 15 modular monolith for B2B/B2C product catalog and order management. It supports three app modes via config: **Catalog** (showcase only), **B2B** (quote-to-order), **B2C** (retail e-commerce).

- **Backend:** .NET 10, PostgreSQL 16, MediatR, EF Core
- **Frontend:** Next.js 15 (App Router), React 19, Tailwind, shadcn/ui
- **Mobile:** React Native (Expo) for B2B partners

---

## Current Configuration (as of last session)

- **AppMode:** B2C (`appsettings.json` + `web/.env.local` with `NEXT_PUBLIC_APP_MODE=B2C`)
- **Database:** Fresh reset with seeded mock data
- **Feature flags:** Managed in Admin → Settings (content."AppSettings")

---

## Recent Changes (This Session)

### 1. Header & Navbar
- **Layout:** Logo left, search center, cart/favorites right
- **Mega menu:** Opens on hover/click; does not navigate directly (prevents accidental navigation)
- **Mega menu visibility:** Rendered via `createPortal` into `document.body` to avoid overflow clipping
- **Categories:** Only categories with `showInNavbar: true` appear in the navbar

### 2. Home Page Sliders (B2C / Catalog only)
- **Feature flag:** `Features.HomeSliders.Enabled` (default: true)
- **Visibility:** Hidden in B2B mode
- **Data source:** Database (content schema), not frontend mock data
- **Entities:** `HeroSlides`, `HomeCategorySlides`, `FeaturedBrands` in Content module
- **API:** `GET /api/content/home-sliders` returns hero, category, and brand data

### 3. Database Seed Data
- **Catalog:** Categories (10), brands (6), products (22), product images
- **Content:** Hero slides (4), home category slides (6), featured brands (8)
- **Important:** Product image `Type` must be `Original`, `Thumbnail`, `Medium`, or `Large` — not `Primary` (causes 500 and 404 on product detail)
- **Decimal formatting:** Use `InvariantCulture` when inserting prices in raw SQL (e.g. `price.ToString("F2", CultureInfo.InvariantCulture)`)

### 4. Next.js 15 Params
- Dynamic route `params` are a `Promise`; use `use(params)` in client components
- Updated: `admin/categories/[id]`, `admin/orders/[id]`, `partner/orders/[id]`

### 5. Category "Show in Navbar"
- Query invalidation added for `categories-tree`, `admin-categories`, `categories` when a category is updated
- Ensures navbar and admin lists refresh after edits

---

## Key Paths

| Purpose | Path |
|--------|------|
| API entry | `src/API/Storefront.Api/Program.cs` |
| DB init & seed | `src/API/Storefront.Api/Extensions/DatabaseExtensions.cs` |
| App mode (web) | `web/src/lib/app-mode.ts` |
| Header | `web/src/components/layout/header.tsx` |
| Home sliders | `web/src/components/home/home-sliders-section.tsx` |
| Product detail | `web/src/app/products/[id]/page.tsx` |
| Content entities | `src/Modules/Content/.../Entities/` (HeroSlide, HomeCategorySlide, FeaturedBrand) |

---

## How to Run

```bash
# 1. Start PostgreSQL (Docker)
docker-compose up -d

# 2. Reset DB (optional, for fresh seed)
docker exec storefront-db psql -U postgres -c "DROP DATABASE IF EXISTS storefront;"
docker exec storefront-db psql -U postgres -c "CREATE DATABASE storefront;"
docker exec storefront-db psql -U postgres -d storefront -c "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";"
docker exec storefront-db psql -U postgres -d storefront -c "CREATE EXTENSION IF NOT EXISTS pg_trgm;"

# 3. Start API
cd src/API/Storefront.Api && dotnet run

# 4. Start web
cd web && npm run dev
```

**Admin:** `admin@storefront.com` / `AdminPassword123!`

---

## Gotchas

1. **Product 404:** If product detail returns 404, check that `ProductImages.Type` is a valid `ImageType` enum value (`Original`, `Thumbnail`, `Medium`, `Large`), not `Primary`.
2. **Sliders not showing:** Requires `NEXT_PUBLIC_APP_MODE` = `Catalog` or `B2C`, and `Features.HomeSliders.Enabled` = true.
3. **Public storefront:** Enable `Features.PublicStorefront.Enabled` in Admin Settings to see the public site.
4. **Seed runs once:** Catalog and slider seeds skip if data already exists. Reset DB to re-seed.

---

## Planned Cleanup (User Noted)

- Remove mock seed logic (`SeedCatalogDataAsync`, `SeedHomeSlidersDataAsync`) when no longer needed for testing.
- Keep `SeedDefaultSettingsAsync` (app settings).

---

## Related Docs

- `CLAUDE_MULTITIER_PROMPT.md` — Multi-tier architecture, variants migration
- `CLAUDE_MOBILE_PROMPT.md` — Mobile app context
- `README.md` — General project overview
