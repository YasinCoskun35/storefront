# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned
- Order management system (B2B workflow)
- Partner portal with authentication
- Mobile app (React Native / Flutter)
- Product variants (fabric, color options)
- Bulk import/export
- Payment gateway integration
- Analytics dashboard

---

## [1.0.0] - 2026-02-08

### Added - Foundation
- ✅ Modular monolith architecture with .NET 10
- ✅ PostgreSQL 16 with schema isolation (identity, catalog, content)
- ✅ Clean Architecture with CQRS pattern (MediatR)
- ✅ Result pattern for explicit error handling
- ✅ JWT authentication with refresh tokens
- ✅ Next.js 15 admin dashboard with shadcn/ui

### Added - Catalog Module
- ✅ Product management (Simple products)
- ✅ Product bundles/sets (complex products)
- ✅ Hierarchical categories (parent/child)
- ✅ Brand management
- ✅ Image upload with async WebP processing (3 sizes)
- ✅ Fuzzy search using PostgreSQL trigrams (pg_trgm)
- ✅ Price configuration toggle (B2B mode)

### Added - Content Module
- ✅ Blog post management with rich text editor
- ✅ Static pages (About, Contact, etc.)
- ✅ SEO metadata (auto-generated slugs, meta tags)
- ✅ Sitemap generation

### Added - Identity Module
- ✅ Admin user authentication
- ✅ Role-based access control (Admin, Manager, User)
- ✅ JWT token generation and refresh
- ✅ Password hashing with BCrypt

### Added - Infrastructure
- ✅ Docker Compose for development
- ✅ Multi-stage Docker builds for production
- ✅ Nginx reverse proxy with caching
- ✅ Automatic database initialization on startup
- ✅ Retry logic for database connections
- ✅ Health checks for all services

### Added - Testing
- ✅ Unit tests with xUnit
- ✅ Integration tests with Testcontainers
- ✅ Architecture tests with NetArchTest
- ✅ FluentAssertions for readable assertions

### Added - Documentation
- ✅ Complete README with badges
- ✅ Installation guide (5-minute setup)
- ✅ Architecture guide (patterns, design decisions)
- ✅ API reference (all endpoints)
- ✅ Quick reference (command cheat sheet)
- ✅ Contributing guide
- ✅ Feature guides (bundles, pricing, B2B orders)
- ✅ Troubleshooting guides
- ✅ Admin panel guide
- ✅ Design system documentation

### Changed
- Made `Product.Price` nullable to support B2B mode
- Updated validators to respect `CatalogSettings.RequirePriceForProducts`
- Improved `DatabaseExtensions` with retry logic and manual DDL fallback

### Fixed
- Port conflicts (fixed to 8080/7080)
- CORS issues (added `AllowFrontend` policy)
- Database table creation (manual SQL for complex indexes)
- ImageSharp version conflicts (updated to 3.1.7)
- Package version mismatches (updated to .NET 10)
- Admin panel features (settings, add product, add category)
- Frontend dependency conflicts (adjusted versions)

---

## Version History

- **v1.0.0** (2026-02-08) - Initial release with complete catalog system
- **v0.8.0** (2026-02-07) - Bundle products and price configuration
- **v0.7.0** (2026-02-06) - Admin panel implementation
- **v0.6.0** (2026-02-05) - Content module and SEO
- **v0.5.0** (2026-02-04) - Catalog module with image processing
- **v0.4.0** (2026-02-03) - Identity module and JWT auth
- **v0.3.0** (2026-02-02) - Docker setup and infrastructure
- **v0.2.0** (2026-02-01) - Clean Architecture foundation
- **v0.1.0** (2026-01-31) - Project initialization

---

## Links

- [Releases](../../releases)
- [Issues](../../issues)
- [Documentation](docs/)

---

**Legend:**
- ✅ Completed
- 🚧 In Progress
- 📋 Planned
- 🔮 Future
