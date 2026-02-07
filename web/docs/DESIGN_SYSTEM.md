# 🎨 Hardware Store Design System

## ✅ Setup Complete!

Your frontend now has a professional hardware store design system with:
- ✅ shadcn/ui components installed
- ✅ Custom color scheme (Orange + Charcoal)
- ✅ Custom fonts (Inter + Poppins)
- ✅ Semantic colors for stock status
- ✅ Ready-to-use components

---

## 🎨 Color Scheme

### Primary Colors

```css
Primary (Orange):    #FA8334 - Use for CTAs, highlights, links
Secondary (Dark):    #1A1F2E - Use for headers, important text
Background (White):  #FFFFFF - Main background
Muted (Light Gray):  #F5F6F7 - Cards, secondary backgrounds
```

### Semantic Colors (Stock Status)

```css
Success (Green):     #10B981 - In Stock
Warning (Yellow):    #F59E0B - Low Stock  
Destructive (Red):   #EF4444 - Out of Stock
Accent (Teal):       #00A1B3 - Info, new items
```

---

## 🔤 Typography

### Fonts
- **Inter** - Body text (default)
- **Poppins** - Headings and display text

### Usage

```tsx
// Heading with display font
<h1 className="font-display text-4xl font-bold text-secondary">
  Hardware Store
</h1>

// Body text (font-sans is default)
<p className="text-base text-foreground">
  Quality tools and equipment for professionals
</p>
```

---

## 🧩 Installed Components

All components are in `src/components/ui/`:

- ✅ `button` - Buttons with variants
- ✅ `card` - Content containers
- ✅ `badge` - Labels and tags
- ✅ `avatar` - User avatars
- ✅ `skeleton` - Loading states
- ✅ `dialog` - Modals
- ✅ `dropdown-menu` - Dropdowns
- ✅ `input` - Form inputs
- ✅ `label` - Form labels
- ✅ `select` - Select dropdowns
- ✅ `table` - Data tables
- ✅ `tabs` - Tab navigation
- ✅ `toast` - Notifications
- ✅ `separator` - Dividers

---

## 📦 Component Examples

### Product Card

```tsx
import { ProductCard } from "@/components/products/product-card";

<ProductCard
  id="1"
  name="DeWalt 20V MAX Cordless Drill"
  price={199.99}
  image="/images/product.jpg"
  stockStatus="InStock"
  category="Power Tools"
/>
```

### Stock Status Badges

```tsx
import { Badge } from "@/components/ui/badge";

// In Stock
<Badge className="bg-success text-success-foreground">
  In Stock
</Badge>

// Low Stock
<Badge className="bg-warning text-warning-foreground">
  Low Stock
</Badge>

// Out of Stock
<Badge className="bg-destructive text-destructive-foreground">
  Out of Stock
</Badge>
```

### Buttons

```tsx
import { Button } from "@/components/ui/button";

// Primary button
<Button>Add to Cart</Button>

// Secondary button
<Button variant="secondary">View Details</Button>

// Outline button
<Button variant="outline">Learn More</Button>

// With icon
<Button className="gap-2">
  <ShoppingCart className="h-4 w-4" />
  Add to Cart
</Button>
```

### Cards

```tsx
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from "@/components/ui/card";

<Card>
  <CardHeader>
    <CardTitle>Product Category</CardTitle>
    <CardDescription>Browse our selection</CardDescription>
  </CardHeader>
  <CardContent>
    <p>Content goes here...</p>
  </CardContent>
  <CardFooter>
    <Button>View All</Button>
  </CardFooter>
</Card>
```

### Forms

```tsx
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";

<div className="space-y-4">
  <div className="space-y-2">
    <Label htmlFor="name">Product Name</Label>
    <Input id="name" placeholder="Enter product name" />
  </div>
  
  <div className="space-y-2">
    <Label htmlFor="price">Price</Label>
    <Input id="price" type="number" placeholder="0.00" />
  </div>
  
  <Button>Save Product</Button>
</div>
```

### Toasts (Notifications)

```tsx
import { useToast } from "@/hooks/use-toast";

function Component() {
  const { toast } = useToast();
  
  return (
    <Button
      onClick={() => {
        toast({
          title: "Product Added",
          description: "The product has been added to your cart.",
        });
      }}
    >
      Add to Cart
    </Button>
  );
}
```

---

## 🎯 Common Patterns

### Product Grid Layout

```tsx
<div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
  {products.map((product) => (
    <ProductCard key={product.id} {...product} />
  ))}
</div>
```

### Hero Section

```tsx
<section className="bg-gradient-to-r from-secondary to-secondary/90 py-20">
  <div className="container mx-auto px-4 text-center">
    <h1 className="font-display text-5xl font-bold text-white mb-6">
      Professional Tools & Hardware
    </h1>
    <p className="text-xl text-white/90 mb-8">
      Quality equipment for every project
    </p>
    <Button size="lg" className="gap-2">
      Browse Products
    </Button>
  </div>
</section>
```

### Category Card

```tsx
<Card className="group cursor-pointer transition-all hover:shadow-lg">
  <CardContent className="p-6 text-center">
    <div className="mb-4 inline-flex h-16 w-16 items-center justify-center rounded-full bg-primary/10">
      <Wrench className="h-8 w-8 text-primary" />
    </div>
    <h3 className="font-display text-xl font-semibold mb-2">
      Power Tools
    </h3>
    <p className="text-sm text-muted-foreground">
      Professional grade power tools
    </p>
  </CardContent>
</Card>
```

### Loading Skeleton

```tsx
import { Skeleton } from "@/components/ui/skeleton";

<Card>
  <CardContent className="p-4 space-y-3">
    <Skeleton className="h-48 w-full" />
    <Skeleton className="h-4 w-3/4" />
    <Skeleton className="h-4 w-1/2" />
  </CardContent>
</Card>
```

---

## 🚀 View the Design System

Visit the design system page to see all components in action:

```bash
# Start the dev server
cd /Users/yasincoskun/Projects/Storefront/web
npm run dev

# Open in browser
http://localhost:3000/design-system
```

---

## 📚 Resources

### Documentation
- **shadcn/ui**: https://ui.shadcn.com
- **Tailwind CSS**: https://tailwindcss.com
- **Lucide Icons**: https://lucide.dev

### Icons for Hardware Store
Already installed via `lucide-react`:
- `Wrench`, `Hammer`, `Drill`, `Screwdriver`
- `Package`, `Truck`, `Shield`, `Star`
- `ShoppingCart`, `Search`, `Menu`
- And 1000+ more!

### Adding More Components

```bash
# See all available components
npx shadcn@latest add

# Add specific component
npx shadcn@latest add [component-name]

# Examples:
npx shadcn@latest add alert
npx shadcn@latest add checkbox
npx shadcn@latest add radio-group
npx shadcn@latest add slider
npx shadcn@latest add switch
```

---

## 🎨 Customization

### Change Primary Color

Edit `src/app/globals.css`:

```css
:root {
  --primary: 25 95% 53%;  /* Change these HSL values */
}
```

### Change Fonts

Edit `src/app/layout.tsx`:

```typescript
import { Roboto, OpenSans } from "next/font/google";

const roboto = Roboto({ weight: ["400", "700"], subsets: ["latin"] });
```

### Add Custom Colors

Edit `tailwind.config.ts`:

```typescript
colors: {
  brand: {
    blue: "#0066CC",
    gold: "#FFD700",
  },
}
```

---

## ✨ Next Steps

### 1. Build Homepage
- Hero section with CTA
- Featured categories grid
- Popular products section
- Why choose us section

### 2. Build Product Catalog
- Filter sidebar
- Product grid with ProductCard
- Pagination
- Sort dropdown

### 3. Build Product Detail Page
- Image gallery
- Product info
- Specifications table
- Related products

### 4. Build Admin Dashboard
- Stats cards
- Data tables
- Forms for CRUD operations
- File upload for images

---

## 💡 Tips

### Responsive Design
```tsx
// Mobile-first approach
<div className="
  grid 
  grid-cols-1       /* 1 column on mobile */
  sm:grid-cols-2    /* 2 columns on tablet */
  lg:grid-cols-3    /* 3 columns on desktop */
  xl:grid-cols-4    /* 4 columns on large screens */
  gap-6
">
```

### Consistent Spacing
```tsx
// Use Tailwind spacing scale
p-4   /* 16px padding */
py-8  /* 32px vertical padding */
gap-6 /* 24px gap between items */
```

### Semantic HTML
```tsx
// Use proper HTML elements
<main>
  <section>
    <article>
      <header>
      <footer>
```

---

## 🆘 Need Help?

All components are fully documented at:
- **shadcn/ui docs**: https://ui.shadcn.com/docs/components
- **Tailwind docs**: https://tailwindcss.com/docs

Your design system is ready to use! Start building beautiful pages! 🎉



