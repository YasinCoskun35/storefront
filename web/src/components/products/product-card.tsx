import Image from "next/image";
import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { ShoppingCart } from "lucide-react";

interface ProductCardProps {
  id: string;
  name: string;
  price: number;
  image: string;
  stockStatus: "InStock" | "LowStock" | "OutOfStock";
  category?: string;
}

const stockConfig = {
  InStock: {
    label: "In Stock",
    variant: "default" as const,
    className: "bg-success text-success-foreground hover:bg-success/90",
  },
  LowStock: {
    label: "Low Stock",
    variant: "secondary" as const,
    className: "bg-warning text-warning-foreground hover:bg-warning/90",
  },
  OutOfStock: {
    label: "Out of Stock",
    variant: "destructive" as const,
    className: "bg-destructive text-destructive-foreground",
  },
};

export function ProductCard({
  id,
  name,
  price,
  image,
  stockStatus,
  category,
}: ProductCardProps) {
  const stock = stockConfig[stockStatus];

  return (
    <Card className="group relative flex h-full flex-col overflow-hidden transition-shadow hover:shadow-lg">
      {/* Image */}
      <Link href={`/products/${id}`} className="relative block">
        <div className="relative aspect-square overflow-hidden bg-muted">
          <Image
            src={image || "/placeholder.jpg"}
            alt={name}
            fill
            className="object-cover transition-transform duration-300 group-hover:scale-105"
            sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
          />
          
          {/* Stock Badge */}
          <div className="absolute right-2 top-2">
            <Badge className={stock.className}>{stock.label}</Badge>
          </div>

          {/* Category Badge */}
          {category && (
            <div className="absolute left-2 top-2">
              <Badge variant="secondary" className="bg-background/80 backdrop-blur-sm">
                {category}
              </Badge>
            </div>
          )}
        </div>
      </Link>

      {/* Content */}
      <CardContent className="flex-1 p-4">
        <Link href={`/products/${id}`}>
          <h3 className="font-display text-lg font-semibold text-foreground line-clamp-2 transition-colors hover:text-primary">
            {name}
          </h3>
        </Link>
      </CardContent>

      {/* Footer */}
      <CardFooter className="flex items-center justify-between gap-2 p-4 pt-0">
        <div className="flex flex-col">
          <span className="text-2xl font-bold text-primary">
            ${price.toFixed(2)}
          </span>
        </div>
        
        <Button 
          size="sm" 
          disabled={stockStatus === "OutOfStock"}
          className="gap-2"
        >
          <ShoppingCart className="h-4 w-4" />
          View
        </Button>
      </CardFooter>
    </Card>
  );
}
