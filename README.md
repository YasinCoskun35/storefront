# Storefront - B2B Furniture Catalog & Order Management System

> A modern B2B platform for furniture manufacturers to showcase products and manage partner orders with quote-based pricing workflow.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)
[![Next.js](https://img.shields.io/badge/Next.js-15-000000?logo=next.js)](https://nextjs.org/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

---

## 📋 **Table of Contents**

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Documentation](#documentation)
- [Project Structure](#project-structure)
- [API Reference](#api-reference)
- [Contributing](#contributing)

---

## 🎯 **Overview**

Storefront is a production-ready B2B catalog and order management system designed for furniture manufacturers. It enables:

- **Product Catalog**: Browse products and bundles without public pricing
- **Quote-Based Orders**: Partners request orders, you provide quotes and manage fulfillment
- **Order Tracking**: Real-time status updates from request to delivery
- **Communication**: Built-in commenting system for quotes, payment links, and updates
- **Bundle Products**: Create product sets (e.g., Living Room Set = Sofa + Chairs + Table)

Perfect for **B2B furniture manufacturers, wholesalers, and distributors** moving from traditional phone/email orders to a digital platform.

---

## ✨ **Features**

### **Catalog Management**
- ✅ **Product Types**: Simple products and product bundles/sets
- ✅ **Hierarchical Categories**: Parent/child category structure
- ✅ **Brand Management**: Associate products with manufacturers
- ✅ **Advanced Search**: PostgreSQL trigram-based fuzzy search
- ✅ **Image Processing**: Automatic WebP conversion with multiple sizes (async background processing)
- ✅ **SEO Optimization**: Auto-generated slugs, meta tags, sitemaps

### **B2B Features**
- ✅ **Price Configuration**: Toggle pricing visibility (B2B vs e-commerce modes)
- ✅ **Quote Workflow**: Partners request orders, you provide quotes
- ✅ **Order Management**: Status tracking from request to delivery
- ✅ **Partner Accounts**: Manage B2B customers with company info
- ✅ **Communication**: Order comments with payment links and documents

### **Admin Dashboard** (Web)
- ✅ **Product Management**: CRUD with drag-drop image upload
- ✅ **Category Management**: Hierarchical structure with drag-drop reordering
- ✅ **Bundle Builder**: Create product sets with components
- ✅ **Content Management**: Blog posts and static pages with rich text editor
- ✅ **User Management**: Role-based access control (Admin, Manager, User)
- ✅ **Order Dashboard**: Review requests, send quotes, track fulfillment

### **Technical Features**
- ✅ **Modular Monolith**: Clean separation with isolated database schemas
- ✅ **CQRS Pattern**: MediatR for commands and queries
- ✅ **Result Pattern**: Explicit error handling without exceptions
- ✅ **JWT Authentication**: Secure admin authentication with refresh tokens
- ✅ **Background Jobs**: Image processing via Channels (non-blocking)
- ✅ **Docker Ready**: Complete containerization for development and production
- ✅ **Comprehensive Tests**: Architecture, unit, and integration tests

---

## 🏗️ **Architecture**

### **Modular Monolith Design**

```
┌─────────────────────────────────────────────────────────┐
│                    API Gateway (Nginx)                   │
└────────────┬────────────────────────────────┬───────────┘
             │                                 │
    ┌────────▼─────────┐              ┌───────▼──────────┐
    │   .NET 10 API    │              │   Next.js 15     │
    │   (Backend)      │              │   (Admin Panel)  │
    └────────┬─────────┘              └──────────────────┘
             │
    ┌────────▼────────────────────────────────────────────┐
    │              PostgreSQL 16                          │
    │  ┌──────────┬──────────┬──────────┐                │
    │  │ identity │ catalog  │ content  │  (schemas)     │
    │  └──────────┴──────────┴──────────┘                │
    └─────────────────────────────────────────────────────┘
```

### **Module Isolation**

Each module has:
- **Separate Schema**: `identity`, `catalog`, `content`
- **Own Migration History**: `__EFMigrationsHistory_<Module>`
- **Independent Boundaries**: Modules cannot reference each other
- **Clean Architecture**: Domain → Application → Infrastructure → API

### **Layers**

```
┌─────────────────┐
│   API Layer     │  Controllers (thin, delegate to MediatR)
├─────────────────┤
│ Infrastructure  │  DbContext, Services, Background Jobs
├─────────────────┤
│  Application    │  Commands, Queries, Validators (CQRS)
├─────────────────┤
│     Domain      │  Entities, Value Objects, Enums
└─────────────────┘
```

---

## 🛠️ **Tech Stack**

### **Backend**
- **.NET 10 LTS** - Latest long-term support release
- **C# 14** - Modern language features
- **PostgreSQL 16** - Robust relational database with trigram search
- **Entity Framework Core 10** - ORM with schema isolation
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Request validation pipeline
- **ImageSharp** - Image processing and optimization
- **JWT** - Authentication and authorization

### **Frontend (Admin Panel)**
- **Next.js 15** - React framework with App Router
- **React 19** - Latest React with Server Components
- **TypeScript** - Type safety
- **Tailwind CSS** - Utility-first styling
- **shadcn/ui** - Beautiful component library
- **TanStack Query v5** - Server state management
- **React Hook Form** - Form handling
- **TipTap** - Rich text editor

### **Infrastructure**
- **Docker & Docker Compose** - Containerization
- **Nginx** - Reverse proxy and static file serving
- **GitHub Actions** - CI/CD (coming soon)

### **Testing**
- **xUnit** - Test framework
- **NSubstitute** - Mocking library
- **FluentAssertions** - Assertion library
- **NetArchTest** - Architecture rules validation
- **Testcontainers** - Integration testing with real PostgreSQL

---

## 🚀 **Getting Started**

### **Prerequisites**

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (required for PostgreSQL)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) (for admin panel)
- [Visual Studio Code](https://code.visualstudio.com/) (recommended)

### **Quick Start (5 minutes)**

```bash
# 1. Clone the repository
git clone <your-repo-url>
cd Storefront

# 2. Start PostgreSQL
docker-compose up -d

# 3. Start Backend API
cd src/API/Storefront.Api
dotnet run

# Backend runs at: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger

# 4. Start Admin Panel (optional)
cd web
npm install
npm run dev

# Admin panel: http://localhost:3000
```

### **Default Admin Credentials**

```
Email: admin@storefront.com
Password: AdminPassword123!
```

---

## 📖 **Documentation**

### **Getting Started**
- [Installation Guide](docs/INSTALLATION.md) - Detailed setup instructions
- [Quick Reference](docs/QUICK_REFERENCE.md) - Common commands
- [Startup Guide](docs/STARTUP_GUIDE.md) - Troubleshooting startup issues

### **Development**
- [Architecture Guide](docs/ARCHITECTURE.md) - System design and principles
- [API Reference](docs/API_REFERENCE.md) - Complete endpoint documentation
- [Testing Guide](docs/TESTING.md) - Running and writing tests
- [Debugging Guide](docs/DEBUGGING.md) - Backend and frontend debugging

### **Features**
- [Product Bundles](docs/features/PRODUCT_BUNDLES.md) - Bundle/set products
- [Price Configuration](docs/features/PRICE_CONFIGURATION.md) - Toggle pricing
- [B2B Order System](docs/features/B2B_ORDERS.md) - Order management workflow
- [Image Processing](docs/features/IMAGE_PROCESSING.md) - Async image handling

### **Deployment**
- [Deployment Guide](docs/DEPLOYMENT.md) - Production deployment
- [Docker Guide](docs/DOCKER.md) - Container management

### **Frontend**
- [Admin Panel Guide](web/docs/ADMIN_PANEL.md) - Admin dashboard usage
- [Design System](web/docs/DESIGN_SYSTEM.md) - UI components and styling

---

## 📁 **Project Structure**

```
Storefront/
├── src/
│   ├── API/
│   │   └── Storefront.Api/              # Main API host
│   ├── Modules/
│   │   ├── Identity/                    # Auth & user management
│   │   ├── Catalog/                     # Products, categories, bundles
│   │   └── Content/                     # Blog, pages, SEO
│   ├── Shared/
│   │   └── Storefront.SharedKernel/     # Result pattern, common types
│   └── Infrastructure/
│       └── Storefront.Infrastructure/   # Cross-cutting concerns
├── tests/
│   ├── Storefront.UnitTests/            # Unit tests
│   ├── Storefront.IntegrationTests/     # API integration tests
│   └── Storefront.ArchitectureTests/    # Architecture validation
├── web/                                 # Next.js admin panel
├── docker/                              # Docker configs
├── scripts/                             # Database backup/restore
└── docs/                                # Documentation
```

---

## 🔌 **API Reference**

### **Authentication**
```http
POST /api/identity/auth/login      # Login with credentials
POST /api/identity/auth/refresh    # Refresh JWT token
```

### **Products**
```http
GET    /api/catalog/products                           # List/search products
GET    /api/catalog/products/{id}                      # Get product details
POST   /api/catalog/products                           # Create product
PUT    /api/catalog/products/{id}                      # Update product
DELETE /api/catalog/products/{id}                      # Delete product
POST   /api/catalog/products/{id}/images               # Upload image
```

### **Bundles**
```http
GET    /api/catalog/products/{id}/bundle               # Get bundle with components
POST   /api/catalog/products/{id}/components           # Add component to bundle
DELETE /api/catalog/products/{bundleId}/components/{componentId} # Remove component
```

### **Categories**
```http
GET    /api/catalog/categories         # List categories
POST   /api/catalog/categories         # Create category
PUT    /api/catalog/categories/{id}    # Update category
DELETE /api/catalog/categories/{id}    # Delete category
```

**Full API documentation:** [docs/API_REFERENCE.md](docs/API_REFERENCE.md)

---

## 🎨 **Configuration**

### **B2B Mode (Current - No Pricing)**

`appsettings.json`:
```json
{
  "CatalogSettings": {
    "PricingEnabled": false,
    "RequirePriceForProducts": false,
    "ShowPriceLabel": "Contact for Quote",
    "AllowPriceInquiry": true
  }
}
```

### **E-commerce Mode (With Pricing)**

```json
{
  "CatalogSettings": {
    "PricingEnabled": true,
    "RequirePriceForProducts": true,
    "ShowPriceLabel": null,
    "AllowPriceInquiry": false
  }
}
```

No code changes needed - just toggle the configuration!

---

## 🧪 **Testing**

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Storefront.UnitTests
dotnet test tests/Storefront.IntegrationTests
dotnet test tests/Storefront.ArchitectureTests

# With coverage
dotnet test /p:CollectCoverage=true
```

**Test Coverage:**
- ✅ Architecture validation (NetArchTest)
- ✅ Domain logic (unit tests)
- ✅ API endpoints (integration tests with Testcontainers)
- ✅ Result pattern and value objects

---

## 🐳 **Docker Deployment**

### **Development**
```bash
docker-compose up -d
```

### **Production**
```bash
docker-compose -f docker-compose.prod.yml up -d --build
```

**Includes:**
- Multi-stage optimized builds
- Nginx reverse proxy with caching
- Health checks for all services
- Automatic database initialization
- Volume persistence

---

## 📦 **Key Features Explained**

### **Product Bundles**

Create product sets that contain multiple items:

```json
{
  "name": "Living Room Set",
  "productType": "Bundle",
  "bundleItems": [
    { "componentProductId": "sofa-id", "quantity": 1 },
    { "componentProductId": "chair-id", "quantity": 2 }
  ]
}
```

Benefits:
- Sell products individually or as sets
- Calculate bundle pricing automatically or set fixed price
- Show savings to customers
- Track bundle inventory

[Learn more →](docs/features/PRODUCT_BUNDLES.md)

### **Price Configuration**

Toggle pricing on/off without code changes:

**B2B Mode** (Quote-based):
- Products shown without prices
- "Contact for Quote" button
- Quote workflow via order comments

**E-commerce Mode**:
- Prices displayed
- Add to cart functionality
- Standard checkout

[Learn more →](docs/features/PRICE_CONFIGURATION.md)

### **B2B Order System** (Coming Soon)

Complete order lifecycle management:

```
Request → Review → Quote → Confirm → 
Prepare → QC → Ship → Deliver
```

Features:
- Order status tracking
- Communication via comments
- Payment link sharing
- Document uploads
- Partner management

[Learn more →](docs/features/B2B_ORDERS.md)

---

## 🛣️ **Roadmap**

### **Phase 1: Catalog Foundation** ✅ **COMPLETE**
- [x] Product management (Simple + Bundles)
- [x] Category management (hierarchical)
- [x] Brand management
- [x] Price configuration toggle
- [x] Image processing system
- [x] Search functionality
- [x] Admin dashboard

### **Phase 2: Order Management** 🚧 **IN PROGRESS**
- [ ] Order request from partners
- [ ] Status workflow
- [ ] Quote management
- [ ] Comment system
- [ ] Document attachments
- [ ] Order tracking

### **Phase 3: Partner Portal** 📋 **PLANNED**
- [ ] Partner authentication
- [ ] Partner dashboard
- [ ] Order history
- [ ] Quote approval
- [ ] Communication hub

### **Phase 4: Mobile App** 📱 **PLANNED**
- [ ] Browse catalog (React Native / Flutter)
- [ ] Request orders
- [ ] Track order status
- [ ] Communication with admin
- [ ] Push notifications

### **Phase 5: Advanced Features** 🔮 **FUTURE**
- [ ] Product variants (fabric/color options)
- [ ] Bulk order import
- [ ] Payment gateway integration
- [ ] Shipping integrations
- [ ] Analytics dashboard

---

## 📊 **Performance & Scalability**

- **Async Image Processing**: Non-blocking upload handling with Channels
- **Database Indexes**: Optimized queries with GIN trigram indexes
- **Caching**: Nginx-level caching for static assets
- **Lazy Loading**: Server Components + Client Components optimization
- **Background Jobs**: CPU-intensive tasks run in BackgroundService
- **Schema Isolation**: Modules can scale independently

---

## 🔐 **Security**

- ✅ JWT authentication with refresh tokens
- ✅ Role-based authorization (RBAC)
- ✅ HTTP-only cookies for tokens
- ✅ CORS configured
- ✅ Input validation (FluentValidation)
- ✅ SQL injection prevention (EF Core parameterization)
- ✅ Security headers (Nginx)

---

## 🤝 **Contributing**

Contributions welcome! Please read our [Contributing Guide](CONTRIBUTING.md) first.

### **Development Setup**

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

### **Code Quality**

- Run tests: `dotnet test`
- Check architecture rules: `dotnet test tests/Storefront.ArchitectureTests`
- Follow existing patterns (CQRS, Result pattern)
- Add tests for new features

---

## 📄 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📞 **Support**

- **Documentation**: [docs/](docs/)
- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)

---

## 🙏 **Acknowledgments**

Built with:
- [.NET](https://dotnet.microsoft.com/)
- [PostgreSQL](https://www.postgresql.org/)
- [Next.js](https://nextjs.org/)
- [shadcn/ui](https://ui.shadcn.com/)
- [MediatR](https://github.com/jbogard/MediatR)
- [ImageSharp](https://sixlabors.com/products/imagesharp/)

---

## 🚀 **Quick Links**

- 📚 [Full Documentation](docs/)
- 🔌 [API Reference](docs/API_REFERENCE.md)
- 🎨 [Design System](web/docs/DESIGN_SYSTEM.md)
- 🐳 [Docker Guide](docs/DOCKER.md)
- 🧪 [Testing Guide](docs/TESTING.md)
- 🚀 [Deployment Guide](docs/DEPLOYMENT.md)

---

**Made with ❤️ for furniture manufacturers embracing digital transformation**
