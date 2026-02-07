# Documentation Index

Welcome to Storefront documentation! This guide helps you find what you need quickly.

---

## 🚀 Getting Started

Start here if you're new to the project:

1. **[README](../README.md)** - Project overview and features
2. **[Installation Guide](INSTALLATION.md)** - Setup instructions (5 minutes)
3. **[Quick Reference](QUICK_REFERENCE.md)** - Common commands cheat sheet
4. **[Startup Guide](STARTUP_GUIDE.md)** - Troubleshooting startup issues

---

## 📚 Core Documentation

### Architecture & Design
- **[Architecture Guide](ARCHITECTURE.md)** - System design, patterns, and principles
- **[API Reference](API_REFERENCE.md)** - Complete endpoint documentation

### Development
- **[Debugging Guide](DEBUGGING.md)** - Backend and frontend debugging
- **[Testing Guide](TESTING.md)** - Running and writing tests
- **[Contributing Guide](../CONTRIBUTING.md)** - Contribution guidelines

### Deployment
- **[Deployment Guide](DEPLOYMENT.md)** - Production deployment
- **[Docker Guide](DOCKER.md)** - Container management

---

## ✨ Features

Detailed guides for major features:

- **[Product Bundles](features/PRODUCT_BUNDLES.md)** - Create product sets/bundles
- **[Price Configuration](features/PRICE_CONFIGURATION.md)** - Toggle pricing on/off
- **[B2B Order System](features/B2B_ORDERS.md)** - Order management workflow (coming soon)

---

## 🎨 Frontend Documentation

- **[Admin Panel Guide](../web/docs/ADMIN_PANEL.md)** - Using the admin dashboard
- **[Design System](../web/docs/DESIGN_SYSTEM.md)** - UI components and styling
- **[Debugging Frontend](../web/docs/DEBUGGING.md)** - Next.js debugging

---

## 🔧 Troubleshooting

Common issues and solutions:

- **[Startup Issues](STARTUP_GUIDE.md)** - Database connection, port conflicts
- **[Build Issues](troubleshooting/BUILD_ISSUES.md)** - Package version mismatches
- **[Database Issues](troubleshooting/DATABASE_ISSUES.md)** - Table creation, migrations

---

## 📖 Reference

Quick lookups:

| Topic | Link |
|-------|------|
| Common Commands | [Quick Reference](QUICK_REFERENCE.md) |
| API Endpoints | [API Reference](API_REFERENCE.md) |
| Configuration | [Architecture Guide](ARCHITECTURE.md#configuration-management) |
| Testing | [Testing Guide](TESTING.md) |
| Project Structure | [README](../README.md#project-structure) |

---

## 🎯 By Role

### I'm a Developer
1. [Installation](INSTALLATION.md)
2. [Architecture](ARCHITECTURE.md)
3. [Debugging](DEBUGGING.md)
4. [Contributing](../CONTRIBUTING.md)

### I'm a DevOps Engineer
1. [Docker Guide](DOCKER.md)
2. [Deployment](DEPLOYMENT.md)
3. [Startup Guide](STARTUP_GUIDE.md)

### I'm a Designer/Frontend Developer
1. [Design System](../web/docs/DESIGN_SYSTEM.md)
2. [Admin Panel](../web/docs/ADMIN_PANEL.md)
3. [Frontend Debugging](../web/docs/DEBUGGING.md)

### I'm a Product Manager
1. [README](../README.md)
2. [Features Overview](../README.md#features)
3. [Roadmap](../README.md#roadmap)

---

## 📦 Documentation Structure

```
docs/
├── README.md                    # This file
├── INSTALLATION.md              # Setup guide
├── ARCHITECTURE.md              # System design
├── API_REFERENCE.md             # API docs
├── QUICK_REFERENCE.md           # Commands cheat sheet
├── STARTUP_GUIDE.md             # Troubleshooting
├── DEBUGGING.md                 # Debug setup
├── TESTING.md                   # Testing guide
├── DEPLOYMENT.md                # Production deploy
├── DOCKER.md                    # Docker guide
├── features/
│   ├── PRODUCT_BUNDLES.md       # Bundle products
│   ├── PRICE_CONFIGURATION.md   # Pricing toggle
│   └── B2B_ORDERS.md            # Order system
└── troubleshooting/
    ├── BUILD_ISSUES.md          # Build problems
    └── DATABASE_ISSUES.md       # DB problems
```

---

## 🔍 Search Tips

Use GitHub's search (press `/`) or your IDE:

**Common searches:**
- "docker" - Docker and containerization
- "test" - Testing guides
- "migration" - Database migrations
- "authentication" - Auth/JWT setup
- "CORS" - CORS issues
- "bundle" - Product bundle feature

---

## 🆘 Need Help?

Can't find what you need?

1. Check the [FAQ](#faq)
2. Search [Issues](../../issues)
3. Start a [Discussion](../../discussions)
4. Review [Contributing Guide](../CONTRIBUTING.md)

---

## FAQ

### How do I add a new endpoint?
See [Contributing Guide - Adding a New Endpoint](../CONTRIBUTING.md#adding-a-new-endpoint)

### How do I add a new module?
See [Architecture Guide - Extension Points](ARCHITECTURE.md#extension-points)

### How do I debug the backend?
See [Debugging Guide](DEBUGGING.md)

### How do I run tests?
See [Testing Guide](TESTING.md)

### How do I deploy to production?
See [Deployment Guide](DEPLOYMENT.md)

### Where is the database schema?
See [Architecture Guide - Database Isolation](ARCHITECTURE.md#database-isolation)

---

## 📝 Documentation Conventions

- **Code blocks** use syntax highlighting
- **File paths** use `code` formatting
- **Commands** are in code blocks with `bash` syntax
- **Warnings** use blockquotes (> ⚠️ Warning)
- **Tips** use blockquotes (> 💡 Tip)

---

**Last Updated:** 2026-02-08  
**Version:** 1.0.0
