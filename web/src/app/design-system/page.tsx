import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { ProductCard } from "@/components/products/product-card";
import { 
  Wrench, 
  Hammer, 
  Drill, 
  Settings,
  Package,
  Truck,
  Shield,
  Star
} from "lucide-react";

export default function DesignSystemPage() {
  return (
    <div className="container mx-auto px-4 py-12">
      {/* Header */}
      <div className="mb-12 text-center">
        <h1 className="font-display text-5xl font-bold text-secondary mb-4">
          Hardware Store Design System
        </h1>
        <p className="text-xl text-muted-foreground">
          Professional components and color scheme for your storefront
        </p>
      </div>

      {/* Color Palette */}
      <section className="mb-16">
        <h2 className="font-display text-3xl font-bold text-secondary mb-6">
          Color Palette
        </h2>
        
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {/* Primary - Orange */}
          <Card>
            <CardHeader>
              <CardTitle>Primary - Orange</CardTitle>
              <CardDescription>For CTAs and interactive elements</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <div className="h-20 w-full rounded-md bg-primary flex items-center justify-center">
                  <span className="font-mono text-primary-foreground">#FA8334</span>
                </div>
                <p className="text-sm text-muted-foreground">
                  Use for buttons, links, and highlights
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Secondary - Charcoal */}
          <Card>
            <CardHeader>
              <CardTitle>Secondary - Charcoal</CardTitle>
              <CardDescription>For headers and emphasis</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <div className="h-20 w-full rounded-md bg-secondary flex items-center justify-center">
                  <span className="font-mono text-secondary-foreground">#1A1F2E</span>
                </div>
                <p className="text-sm text-muted-foreground">
                  Use for headings and important text
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Success - Green */}
          <Card>
            <CardHeader>
              <CardTitle>Success - Green</CardTitle>
              <CardDescription>For positive states</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <div className="h-20 w-full rounded-md bg-success flex items-center justify-center">
                  <span className="font-mono text-success-foreground">In Stock</span>
                </div>
                <p className="text-sm text-muted-foreground">
                  Use for available products
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Warning - Yellow */}
          <Card>
            <CardHeader>
              <CardTitle>Warning - Yellow</CardTitle>
              <CardDescription>For attention states</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <div className="h-20 w-full rounded-md bg-warning flex items-center justify-center">
                  <span className="font-mono text-warning-foreground">Low Stock</span>
                </div>
                <p className="text-sm text-muted-foreground">
                  Use for warnings and alerts
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Destructive - Red */}
          <Card>
            <CardHeader>
              <CardTitle>Destructive - Red</CardTitle>
              <CardDescription>For errors and unavailable</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <div className="h-20 w-full rounded-md bg-destructive flex items-center justify-center">
                  <span className="font-mono text-destructive-foreground">Out of Stock</span>
                </div>
                <p className="text-sm text-muted-foreground">
                  Use for errors and unavailable items
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Accent - Teal */}
          <Card>
            <CardHeader>
              <CardTitle>Accent - Teal</CardTitle>
              <CardDescription>For informational elements</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                <div className="h-20 w-full rounded-md bg-accent flex items-center justify-center">
                  <span className="font-mono text-accent-foreground">#00A1B3</span>
                </div>
                <p className="text-sm text-muted-foreground">
                  Use for info messages and badges
                </p>
              </div>
            </CardContent>
          </Card>
        </div>
      </section>

      <Separator className="my-12" />

      {/* Typography */}
      <section className="mb-16">
        <h2 className="font-display text-3xl font-bold text-secondary mb-6">
          Typography
        </h2>
        
        <Card>
          <CardContent className="space-y-6 pt-6">
            <div>
              <h1 className="font-display text-5xl font-bold text-secondary mb-2">
                Heading 1 - Display Font
              </h1>
              <p className="text-sm text-muted-foreground font-mono">
                font-display text-5xl font-bold (Poppins)
              </p>
            </div>

            <div>
              <h2 className="font-display text-4xl font-semibold text-secondary mb-2">
                Heading 2 - Display Font
              </h2>
              <p className="text-sm text-muted-foreground font-mono">
                font-display text-4xl font-semibold (Poppins)
              </p>
            </div>

            <div>
              <h3 className="font-display text-3xl font-semibold text-secondary mb-2">
                Heading 3 - Display Font
              </h3>
              <p className="text-sm text-muted-foreground font-mono">
                font-display text-3xl font-semibold (Poppins)
              </p>
            </div>

            <div>
              <p className="text-lg text-foreground mb-2">
                Body Text Large - Regular content with good readability for product descriptions and detailed information.
              </p>
              <p className="text-sm text-muted-foreground font-mono">
                text-lg (Inter - 18px)
              </p>
            </div>

            <div>
              <p className="text-base text-foreground mb-2">
                Body Text Regular - Default body text for general content. Clean and professional appearance.
              </p>
              <p className="text-sm text-muted-foreground font-mono">
                text-base (Inter - 16px)
              </p>
            </div>

            <div>
              <p className="text-sm text-muted-foreground mb-2">
                Small Text - For secondary information, captions, and less important details.
              </p>
              <p className="text-xs text-muted-foreground font-mono">
                text-sm (Inter - 14px)
              </p>
            </div>
          </CardContent>
        </Card>
      </section>

      <Separator className="my-12" />

      {/* Buttons */}
      <section className="mb-16">
        <h2 className="font-display text-3xl font-bold text-secondary mb-6">
          Buttons
        </h2>
        
        <Card>
          <CardContent className="space-y-6 pt-6">
            <div className="space-y-3">
              <h3 className="font-display text-xl font-semibold">Variants</h3>
              <div className="flex flex-wrap gap-3">
                <Button>Primary Button</Button>
                <Button variant="secondary">Secondary</Button>
                <Button variant="outline">Outline</Button>
                <Button variant="ghost">Ghost</Button>
                <Button variant="destructive">Destructive</Button>
              </div>
            </div>

            <div className="space-y-3">
              <h3 className="font-display text-xl font-semibold">Sizes</h3>
              <div className="flex flex-wrap items-center gap-3">
                <Button size="sm">Small</Button>
                <Button size="default">Default</Button>
                <Button size="lg">Large</Button>
              </div>
            </div>

            <div className="space-y-3">
              <h3 className="font-display text-xl font-semibold">With Icons</h3>
              <div className="flex flex-wrap gap-3">
                <Button className="gap-2">
                  <ShoppingCart className="h-4 w-4" />
                  Add to Cart
                </Button>
                <Button variant="outline" className="gap-2">
                  <Package className="h-4 w-4" />
                  View Products
                </Button>
                <Button variant="secondary" className="gap-2">
                  <Truck className="h-4 w-4" />
                  Check Delivery
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      </section>

      <Separator className="my-12" />

      {/* Badges */}
      <section className="mb-16">
        <h2 className="font-display text-3xl font-bold text-secondary mb-6">
          Badges
        </h2>
        
        <Card>
          <CardContent className="space-y-6 pt-6">
            <div className="flex flex-wrap gap-3">
              <Badge>Default Badge</Badge>
              <Badge variant="secondary">Secondary</Badge>
              <Badge variant="outline">Outline</Badge>
              <Badge variant="destructive">Destructive</Badge>
            </div>

            <div className="flex flex-wrap gap-3">
              <Badge className="bg-success text-success-foreground">In Stock</Badge>
              <Badge className="bg-warning text-warning-foreground">Low Stock</Badge>
              <Badge className="bg-destructive text-destructive-foreground">Out of Stock</Badge>
              <Badge className="bg-accent text-accent-foreground">New Arrival</Badge>
            </div>
          </CardContent>
        </Card>
      </section>

      <Separator className="my-12" />

      {/* Icons */}
      <section className="mb-16">
        <h2 className="font-display text-3xl font-bold text-secondary mb-6">
          Hardware Icons (Lucide)
        </h2>
        
        <Card>
          <CardContent className="pt-6">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
              <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-muted/50">
                <Wrench className="h-12 w-12 text-primary" />
                <span className="text-sm font-medium">Wrench</span>
              </div>
              <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-muted/50">
                <Hammer className="h-12 w-12 text-primary" />
                <span className="text-sm font-medium">Hammer</span>
              </div>
              <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-muted/50">
                <Drill className="h-12 w-12 text-primary" />
                <span className="text-sm font-medium">Drill</span>
              </div>
              <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-muted/50">
                <Settings className="h-12 w-12 text-primary" />
                <span className="text-sm font-medium">Settings</span>
              </div>
              <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-muted/50">
                <Package className="h-12 w-12 text-primary" />
                <span className="text-sm font-medium">Package</span>
              </div>
              <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-muted/50">
                <Truck className="h-12 w-12 text-primary" />
                <span className="text-sm font-medium">Truck</span>
              </div>
              <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-muted/50">
                <Shield className="h-12 w-12 text-primary" />
                <span className="text-sm font-medium">Shield</span>
              </div>
              <div className="flex flex-col items-center gap-3 p-4 rounded-lg border bg-muted/50">
                <Star className="h-12 w-12 text-primary" />
                <span className="text-sm font-medium">Star</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </section>

      <Separator className="my-12" />

      {/* Forms */}
      <section className="mb-16">
        <h2 className="font-display text-3xl font-bold text-secondary mb-6">
          Form Elements
        </h2>
        
        <Card>
          <CardContent className="space-y-6 pt-6">
            <div className="space-y-2">
              <Label htmlFor="name">Product Name</Label>
              <Input id="name" placeholder="Enter product name" />
            </div>

            <div className="space-y-2">
              <Label htmlFor="price">Price</Label>
              <Input id="price" type="number" placeholder="0.00" />
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Description</Label>
              <textarea
                id="description"
                className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                placeholder="Enter product description"
              />
            </div>

            <Button>Submit Form</Button>
          </CardContent>
        </Card>
      </section>

      <Separator className="my-12" />

      {/* Product Cards Example */}
      <section className="mb-16">
        <h2 className="font-display text-3xl font-bold text-secondary mb-6">
          Product Cards
        </h2>
        
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          <ProductCard
            id="1"
            name="DeWalt 20V MAX Cordless Drill Driver Kit"
            price={199.99}
            image="/api/placeholder/400/400"
            stockStatus="InStock"
            category="Power Tools"
          />
          <ProductCard
            id="2"
            name="Milwaukee 18V Lithium-Ion Hammer Drill"
            price={149.99}
            image="/api/placeholder/400/400"
            stockStatus="LowStock"
            category="Power Tools"
          />
          <ProductCard
            id="3"
            name="Bosch 12V Max Brushless Impact Driver"
            price={129.99}
            image="/api/placeholder/400/400"
            stockStatus="OutOfStock"
            category="Power Tools"
          />
        </div>
      </section>

      {/* Usage Guide */}
      <section className="mb-16">
        <Card className="border-primary/50 bg-primary/5">
          <CardHeader>
            <CardTitle className="font-display text-2xl">Quick Usage Guide</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <h3 className="font-display text-lg font-semibold mb-2">Using Colors</h3>
              <pre className="bg-secondary text-secondary-foreground p-4 rounded-md overflow-x-auto">
{`// Primary color for CTAs
<Button>Click Me</Button>

// Stock status badges
<Badge className="bg-success text-success-foreground">In Stock</Badge>
<Badge className="bg-warning text-warning-foreground">Low Stock</Badge>
<Badge className="bg-destructive text-destructive-foreground">Out of Stock</Badge>`}
              </pre>
            </div>

            <div>
              <h3 className="font-display text-lg font-semibold mb-2">Using Fonts</h3>
              <pre className="bg-secondary text-secondary-foreground p-4 rounded-md overflow-x-auto">
{`// Display font for headings
<h1 className="font-display text-4xl font-bold">Hardware Store</h1>

// Regular font for body text
<p className="text-base">Quality tools and equipment...</p>`}
              </pre>
            </div>
          </CardContent>
        </Card>
      </section>
    </div>
  );
}

function ShoppingCart({ className }: { className?: string }) {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
    >
      <circle cx="8" cy="21" r="1" />
      <circle cx="19" cy="21" r="1" />
      <path d="M2.05 2.05h2l2.66 12.42a2 2 0 0 0 2 1.58h9.78a2 2 0 0 0 1.95-1.57l1.65-7.43H5.12" />
    </svg>
  );
}

