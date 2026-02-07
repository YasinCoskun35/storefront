# Admin Panel Guide

Complete guide to using the Storefront admin dashboard.

---

## Overview

The admin panel is a Next.js 15 application providing a complete backoffice for managing:
- Products (Simple & Bundles)
- Categories
- Brands
- Blog posts
- Static pages
- Users & roles

**Access:** `http://localhost:3000/admin`  
**Login:** `admin@storefront.com` / `AdminPassword123!`

---

## Features

### Product Management

#### List Products

**URL:** `/admin/products`

**Features:**
- ✅ Search products by name/SKU
- ✅ Filter by category, brand, status
- ✅ Sort by name, price, date
- ✅ Pagination (20 items per page)
- ✅ Quick actions (Edit, Delete, Duplicate)
- ✅ Bulk actions (Delete multiple)

#### Create Product

**URL:** `/admin/products/new`

**Form Fields:**
- Basic Info: Name, SKU, Description
- Pricing: Price, Compare At Price (optional if pricing disabled)
- Product Type: Simple or Bundle
- Category & Brand
- Stock: Status, Quantity
- Dimensions: Length, Width, Height, Weight
- Images: Drag & drop upload (max 20MB)
- SEO: Auto-generated slug, meta tags
- Status: Active/Inactive, Featured

**Bundle Products:**
1. Select "Bundle" product type
2. Add components:
   - Search and select product
   - Set quantity
   - Optional: Override price
   - Set display order
3. Set bundle price (or leave empty for auto-calculation)

#### Edit Product

**URL:** `/admin/products/[id]`

Same form as create, with:
- Pre-filled data
- Image gallery (reorder, delete)
- Delete product button

---

### Category Management

#### List Categories

**URL:** `/admin/categories`

**Features:**
- ✅ Hierarchical tree view (parent/child)
- ✅ Drag & drop to reorder
- ✅ Expand/collapse subcategories
- ✅ Product count per category
- ✅ Quick actions (Edit, Delete, Add Child)

#### Create Category

**URL:** `/admin/categories/new`

**Form Fields:**
- Name
- Description
- Slug (auto-generated)
- Parent Category (optional)
- Display Order
- Status (Active/Inactive)

---

### Brand Management

**URL:** `/admin/brands`

**Features:**
- List all brands
- Create/edit brands
- Logo upload
- Product count

---

### Content Management

#### Blog Posts

**URL:** `/admin/blog`

**Features:**
- Rich text editor (TipTap)
- Featured image upload
- SEO metadata
- Publish/draft status
- Scheduled publishing

**Editor Features:**
- Bold, italic, underline
- Headings (H1-H6)
- Lists (ordered, unordered)
- Links
- Images
- Code blocks
- Tables

#### Static Pages

**URL:** `/admin/pages`

Manage pages like:
- About Us
- Contact
- Privacy Policy
- Terms of Service

---

### User Management

**URL:** `/admin/users`

**Features:**
- List all users
- Create new users
- Assign roles (Admin, Manager, User)
- Activate/deactivate accounts
- Password reset

---

## Navigation

### Main Menu

```
Dashboard
├── Products
│   ├── All Products
│   ├── Add Product
│   ├── Categories
│   └── Brands
├── Content
│   ├── Blog Posts
│   └── Pages
├── Users
│   ├── All Users
│   └── Roles
└── Settings
    ├── General
    ├── Catalog
    └── SEO
```

---

## Quick Actions

### Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+K` | Search |
| `Ctrl+N` | New Product |
| `Ctrl+S` | Save Form |
| `Esc` | Close Modal |

### Bulk Operations

Select multiple items with checkboxes:
- Delete selected
- Change status
- Export CSV

---

## Settings

### General Settings

**URL:** `/admin/settings`

- Site name
- Site URL
- Admin email
- Timezone
- Date format

### Catalog Settings

**URL:** `/admin/settings/catalog`

- Pricing enabled/disabled
- Currency
- Weight/dimension units
- Stock tracking
- Low stock threshold

### SEO Settings

**URL:** `/admin/settings/seo`

- Default meta description
- Default keywords
- Social media tags
- Sitemap settings

---

## Image Management

### Product Images

**Upload:**
1. Drag & drop or click to browse
2. Select images (JPEG, PNG, GIF, WebP)
3. Images upload automatically
4. Processing happens in background

**Processing:**
- Generates 3 WebP sizes:
  - Thumbnail: 200px width
  - Medium: 600px width
  - Large: 1200px width
- Original preserved

**Management:**
- Reorder: Drag images
- Set primary: Click star icon
- Delete: Click trash icon

### Image Guidelines

- **Min resolution:** 800x800px
- **Max file size:** 20MB
- **Formats:** JPEG, PNG, GIF, WebP
- **Aspect ratio:** Square (1:1) recommended
- **Background:** White or transparent

---

## Form Validation

All forms have real-time validation:

**Product Form:**
- Name: Required, max 500 chars
- SKU: Required, unique, max 100 chars
- Price: Required (if pricing enabled), > 0
- Category: Required
- Stock quantity: >= 0

**Error Display:**
- Red outline on invalid fields
- Error message below field
- Toast notification on submit

---

## Data Import/Export

### Export Products

**URL:** `/admin/products/export`

**Formats:**
- CSV
- Excel (XLSX)
- JSON

**Fields:**
- All product data
- Images (URLs)
- Category/brand names

### Import Products

**URL:** `/admin/products/import`

**Steps:**
1. Download template
2. Fill in data
3. Upload file
4. Review mapping
5. Confirm import

**Validation:**
- SKU uniqueness
- Category/brand existence
- Image URL validity

---

## Search

### Global Search

Press `Ctrl+K` or click search icon.

**Search across:**
- Products (name, SKU, description)
- Categories
- Brands
- Blog posts
- Pages

**Features:**
- Fuzzy matching
- Keyboard navigation
- Recent searches

### Advanced Product Search

**URL:** `/admin/products?advanced=true`

**Filters:**
- Text: Name, SKU, description
- Category: Select multiple
- Brand: Select multiple
- Price range
- Stock status
- Date added range
- Active/inactive
- Featured/not featured

---

## Troubleshooting

### Login Issues

**Problem:** Can't login  
**Solution:**
1. Check API is running: `http://localhost:8080/swagger`
2. Verify credentials: `admin@storefront.com` / `AdminPassword123!`
3. Check CORS settings in API
4. Clear browser cache

### Image Upload Fails

**Problem:** Images not uploading  
**Solution:**
1. Check file size (< 20MB)
2. Check file format (JPEG, PNG, GIF, WebP)
3. Check API is running
4. Check `/uploads` folder permissions

### Form Won't Save

**Problem:** Save button doesn't work  
**Solution:**
1. Check for validation errors (red fields)
2. Check browser console for errors
3. Check API connection
4. Try refreshing page

### Slow Performance

**Problem:** Admin panel is slow  
**Solution:**
1. Clear browser cache
2. Check network tab (slow API?)
3. Reduce image sizes
4. Update to latest version

---

## API Integration

Admin panel communicates with backend API:

**Base URL:** `http://localhost:8080/api`

**Authentication:**
- Login → JWT token
- Token stored in HTTP-only cookie
- Automatic refresh on expiry

**API Proxy:**
Next.js API routes act as proxy:
```
/api/admin/* → http://localhost:8080/api/*
```

**Benefits:**
- Avoids CORS issues
- Secure token handling
- Request/response transformation

---

## Customization

### Theme

**File:** `web/tailwind.config.ts`

```typescript
export default {
  theme: {
    extend: {
      colors: {
        primary: '#0070f3',    // Change brand color
        secondary: '#7928ca',
      },
    },
  },
};
```

### Logo

Replace: `web/public/logo.svg`

### Components

Admin uses **shadcn/ui** components.

**Add new component:**
```bash
npx shadcn-ui@latest add [component-name]
```

---

## Best Practices

### Product Management

- ✅ Use descriptive SKUs (e.g., `SOFA-3S-BLK-001`)
- ✅ Add multiple images (6-8 recommended)
- ✅ Fill all SEO fields
- ✅ Use categories consistently
- ✅ Enable products only when ready

### Category Structure

- ✅ Keep hierarchy shallow (max 3 levels)
- ✅ Use descriptive names
- ✅ Don't duplicate categories
- ✅ Use display order for sorting

### Content

- ✅ Write unique meta descriptions
- ✅ Use H1-H6 hierarchy
- ✅ Optimize images before upload
- ✅ Schedule posts in advance

---

## Keyboard Shortcuts Reference

| Action | Shortcut |
|--------|----------|
| Global search | `Ctrl+K` or `Cmd+K` |
| New product | `Ctrl+N` or `Cmd+N` |
| Save form | `Ctrl+S` or `Cmd+S` |
| Close modal | `Esc` |
| Navigate list | `↑` `↓` |
| Select item | `Enter` |
| Select multiple | `Shift+Click` |

---

## Mobile Admin (Coming Soon)

React Native app for managing products on-the-go:
- Quick product edits
- Order management
- Push notifications
- Camera product photos
- Inventory updates

---

**Need Help?** Check [Design System](DESIGN_SYSTEM.md) or [Debugging Guide](DEBUGGING.md)
