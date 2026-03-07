"use client";

import Image from "next/image";
import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { ShoppingCart, Eye } from "lucide-react";
import { getImageUrl, formatPrice } from "@/lib/utils";
import { Product } from "@/lib/api";
import { useQuery } from "@tanstack/react-query";
import { settingsApi } from "@/lib/api/settings";
import { useEffect, useState } from "react";

interface ProductCardProps {
  product: Product;
}

const stockConfig = {
  InStock: {
    label: "Stokta",
    className: "bg-success text-success-foreground hover:bg-success/90",
  },
  LowStock: {
    label: "Son Ürünler",
    className: "bg-warning text-warning-foreground hover:bg-warning/90",
  },
  OutOfStock: {
    label: "Tükendi",
    className: "bg-destructive text-destructive-foreground",
  },
};

const stockStatusMap: Record<number, keyof typeof stockConfig> = {
  0: "OutOfStock",
  1: "InStock",
  2: "LowStock",
};

export function ProductCard({ product }: ProductCardProps) {
  const [isPartner, setIsPartner] = useState(false);

  useEffect(() => {
    setIsPartner(!!localStorage.getItem("partner_access_token"));
  }, []);

  const { data: settings } = useQuery({
    queryKey: ["app-settings"],
    queryFn: settingsApi.getAll,
    staleTime: 5 * 60 * 1000,
  });

  const pricingEnabled =
    settings?.find((s) => s.key === "Features.Pricing.Enabled")?.value?.toLowerCase() === "true";

  const stockKey = stockStatusMap[product.stockStatus] ?? "InStock";
  const stock = stockConfig[stockKey];
  const outOfStock = stockKey === "OutOfStock";

  return (
    <Card className="group relative flex h-full flex-col overflow-hidden transition-all duration-200 hover:shadow-md hover:-translate-y-0.5">
      {/* Image */}
      <Link href={`/products/${product.id}`} className="relative block shrink-0">
        <div className="relative aspect-[4/3] overflow-hidden bg-muted">
          <Image
            src={getImageUrl(product.primaryImageUrl)}
            alt={product.name}
            fill
            className="object-cover transition-transform duration-300 group-hover:scale-105"
            sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
            unoptimized
          />

          {/* Stock Badge */}
          <div className="absolute right-2 top-2">
            <Badge className={stock.className}>{stock.label}</Badge>
          </div>

          {/* Category Badge */}
          {product.categoryName && (
            <div className="absolute left-2 top-2">
              <Badge
                variant="secondary"
                className="bg-background/80 backdrop-blur-sm text-xs"
              >
                {product.categoryName}
              </Badge>
            </div>
          )}
        </div>
      </Link>

      {/* Content */}
      <CardContent className="flex flex-1 flex-col gap-1 p-4">
        <Link href={`/products/${product.id}`}>
          <h3 className="font-semibold text-foreground line-clamp-2 leading-snug transition-colors hover:text-primary text-sm md:text-base">
            {product.name}
          </h3>
        </Link>
        {product.sku && (
          <p className="text-xs text-muted-foreground">SKU: {product.sku}</p>
        )}
      </CardContent>

      {/* Footer */}
      <CardFooter className="flex items-center justify-between gap-2 p-4 pt-0">
        {/* Price section */}
        <div className="flex flex-col">
          {pricingEnabled && product.price != null ? (
            <>
              <span className="text-lg font-bold text-primary leading-none">
                {formatPrice(product.price)}
              </span>
              {product.compareAtPrice != null &&
                product.compareAtPrice > product.price && (
                  <span className="text-xs text-muted-foreground line-through">
                    {formatPrice(product.compareAtPrice)}
                  </span>
                )}
            </>
          ) : (
            <span className="text-xs text-muted-foreground">
              Fiyat için iletişime geçin
            </span>
          )}
        </div>

        {/* CTA */}
        {isPartner ? (
          <Button
            size="sm"
            disabled={outOfStock}
            className="gap-1.5 shrink-0"
            asChild={outOfStock ? false : true}
          >
            {outOfStock ? (
              <span>
                <ShoppingCart className="h-3.5 w-3.5" />
                Tükendi
              </span>
            ) : (
              <Link href={`/products/${product.id}`}>
                <ShoppingCart className="h-3.5 w-3.5" />
                {pricingEnabled ? "Sepete Ekle" : "Teklif Al"}
              </Link>
            )}
          </Button>
        ) : (
          <Button size="sm" variant="outline" className="gap-1.5 shrink-0" asChild>
            <Link href={`/products/${product.id}`}>
              <Eye className="h-3.5 w-3.5" />
              İncele
            </Link>
          </Button>
        )}
      </CardFooter>
    </Card>
  );
}
