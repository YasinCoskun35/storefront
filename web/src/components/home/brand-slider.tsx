"use client";

import Link from "next/link";
import type { FeaturedBrandDto } from "@/lib/api";

interface BrandSliderProps {
  brands: FeaturedBrandDto[];
  title?: string;
}

export function BrandSlider({ brands, title = "Öne Çıkan Markalar" }: BrandSliderProps) {
  if (!brands.length) return null;

  return (
    <section className="py-8 border-t bg-muted/30">
      <div className="container mx-auto px-4">
        <h2 className="text-xl font-bold mb-6 text-center">{title}</h2>
        <div className="flex flex-wrap justify-center gap-6 md:gap-10">
          {brands.map((brand) => (
            <Link
              key={brand.id}
              href={brand.link}
              className="flex items-center justify-center min-w-[100px] h-12 px-4 rounded-lg border bg-background hover:border-primary hover:shadow-md transition-all text-sm font-medium"
            >
              {brand.name}
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
}
