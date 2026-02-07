import Link from "next/link";
import { Button } from "@/components/ui/button";
import { ArrowRight } from "lucide-react";

export default function HomePage() {
  return (
    <div className="container mx-auto px-4 py-16">
      {/* Hero Section */}
      <section className="text-center space-y-6 py-12">
        <h1 className="text-4xl md:text-6xl font-bold tracking-tight">
          Quality Hardware & Tools
        </h1>
        <p className="text-xl text-muted-foreground max-w-2xl mx-auto">
          Discover our extensive collection of professional-grade tools and
          equipment for your next project.
        </p>
        <div className="flex gap-4 justify-center">
          <Link href="/products">
            <Button size="lg">
              Browse Products
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          </Link>
          <Link href="/about">
            <Button size="lg" variant="outline">
              Learn More
            </Button>
          </Link>
        </div>
      </section>

      {/* Categories Preview */}
      <section className="py-12">
        <h2 className="text-3xl font-bold mb-8">Shop by Category</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <Link
            href="/products?category=power-tools"
            className="group relative overflow-hidden rounded-lg border bg-card p-6 hover:shadow-lg transition-shadow"
          >
            <h3 className="text-xl font-semibold mb-2">Power Tools</h3>
            <p className="text-muted-foreground">
              Electric and battery-powered tools for professionals
            </p>
          </Link>
          <Link
            href="/products?category=hand-tools"
            className="group relative overflow-hidden rounded-lg border bg-card p-6 hover:shadow-lg transition-shadow"
          >
            <h3 className="text-xl font-semibold mb-2">Hand Tools</h3>
            <p className="text-muted-foreground">
              Quality manual tools for precision work
            </p>
          </Link>
          <Link
            href="/products?category=safety"
            className="group relative overflow-hidden rounded-lg border bg-card p-6 hover:shadow-lg transition-shadow"
          >
            <h3 className="text-xl font-semibold mb-2">Safety Equipment</h3>
            <p className="text-muted-foreground">
              Protective gear and safety accessories
            </p>
          </Link>
        </div>
      </section>
    </div>
  );
}

