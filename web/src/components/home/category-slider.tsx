"use client";

import Link from "next/link";
import Image from "next/image";
import { ChevronRight } from "lucide-react";
import type { CategorySlideDto } from "@/lib/api";

interface CategorySliderProps {
  categories: CategorySlideDto[];
  title?: string;
}

export function CategorySlider({ categories, title = "Kategoriler" }: CategorySliderProps) {
  if (!categories.length) return null;

  return (
    <section className="py-8">
      <div className="container mx-auto px-4">
        <h2 className="text-2xl font-bold mb-6">{title}</h2>
        <div className="flex gap-4 overflow-x-auto pb-2 scroll-smooth scrollbar-hide -mx-4 px-4">
          {categories.map((cat) => (
            <Link
              key={cat.id}
              href={cat.link}
              className="group shrink-0 w-[180px] md:w-[220px] rounded-xl overflow-hidden border bg-card hover:shadow-lg transition-all"
            >
              <div className="relative aspect-[4/3] bg-muted">
                <Image
                  src={cat.imageUrl}
                  alt={cat.name}
                  fill
                  className="object-cover group-hover:scale-105 transition-transform duration-300"
                  sizes="220px"
                />
              </div>
              <div className="p-3">
                <h3 className="font-semibold text-sm line-clamp-2 group-hover:text-primary transition-colors">
                  {cat.name}
                </h3>
                <p className="text-xs text-muted-foreground mt-0.5">
                  {cat.productCount} ürün
                </p>
                <span className="inline-flex items-center text-xs text-primary font-medium mt-1 group-hover:underline">
                  İncele
                  <ChevronRight className="h-3 w-3 ml-0.5" />
                </span>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
}
