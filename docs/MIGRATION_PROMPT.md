# Prompt: Two Repos Migration (B2B and B2C)

Copy the text below and give it to Claude when you want to continue the migration work.

---

## Prompt

We are splitting our Storefront codebase into two separate product repositories. The current codebase is a .NET 10 + Next.js 15 + Expo app that supports both B2B (admin + partner) and B2C (public storefront with cart/checkout) via AppMode configuration.

**New approach:**
- **Two separate repos** — no monorepo. Each product is its own full repository.
- **B2B first** — admin + partner (web + mobile) in one repo.
- **B2C later** — copy of codebase, strip B2B, keep B2C features. Can copy/reuse components from B2B.

**Product 1 — B2B repo (first focus):**
- Backend: .NET API (Identity, Catalog, Content, Orders) — keep as-is
- Web: Next.js with `/admin/*` and `/partner/*` only. Admin panel + Partner portal in one app.
- Mobile: Expo app for partner portal.
- Remove or strip: B2C routes (`/cart`, `/checkout`), B2C cart API, iyzico payments, public storefront pages (or keep minimal catalog view).
- Set AppMode to B2B always; remove `if (APP_MODE === "B2C")` branches.

**Product 2 — B2C repo (later):**
- Copy of full codebase.
- Web: Public storefront (`/`, `/products`, `/cart`, `/checkout`, `/blog`, etc.). Remove `/admin/*` and `/partner/*` (or keep admin if B2C needs it).
- Backend: Same; keep B2C cart, checkout, iyzico.
- Mobile: Optional; add B2C mobile later if needed.
- Can copy shared components from B2B.

**Migration steps:**
1. Copy repo → Storefront-B2B. Strip B2C (routes, controllers, components).
2. When ready, copy repo → Storefront-B2C. Strip B2B (admin/partner routes, partner mobile).
3. Optional: Extract shared UI to npm package or use git submodules to avoid drift.

**File mapping for B2B:**
- Keep: `web/app/admin/*`, `web/app/partner/*`, `web/app/login`, `mobile/`, backend `src/`
- Remove: `web/app/cart`, `web/app/checkout`, B2CCartsController, B2CCheckoutController, B2CPaymentsController (or gate by config)
- Simplify: `web/app/page.tsx`, `web/app/products` (catalog view only if needed)

Help me execute this migration. Start with the B2B repo.
