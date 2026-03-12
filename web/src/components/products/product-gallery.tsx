"use client";

import Image from "next/image";
import { useState } from "react";
import { ProductImage } from "@/lib/api";
import { getImageUrl } from "@/lib/utils";
import { cn } from "@/lib/utils";

interface ProductGalleryProps {
  primaryImage?: ProductImage;
  images: ProductImage[];
  productName: string;
}

export function ProductGallery({ primaryImage, images, productName }: ProductGalleryProps) {
  const [activeImage, setActiveImage] = useState<ProductImage | undefined>(primaryImage);

  const displayImage = activeImage || primaryImage;
  const allImages = images.filter((img) => img.type !== "Thumbnail").length > 0
    ? images
    : images;

  const thumbnails = allImages.slice(0, 8);

  return (
    <div className="space-y-3">
      {/* Main image */}
      <div className="relative aspect-square overflow-hidden rounded-xl border bg-muted">
        <Image
          src={getImageUrl(displayImage?.url)}
          alt={productName}
          fill
          className="object-cover transition-opacity duration-200"
          priority
          unoptimized
        />
      </div>

      {/* Thumbnails */}
      {thumbnails.length > 1 && (
        <div className="grid grid-cols-5 gap-2">
          {thumbnails.map((img) => (
            <button
              key={img.id}
              onClick={() => setActiveImage(img)}
              className={cn(
                "relative aspect-square overflow-hidden rounded-lg border-2 bg-muted transition-all",
                activeImage?.id === img.id
                  ? "border-primary ring-1 ring-primary"
                  : "border-transparent hover:border-muted-foreground/30"
              )}
            >
              <Image
                src={getImageUrl(img.url)}
                alt={productName}
                fill
                className="object-cover"
                unoptimized
              />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
