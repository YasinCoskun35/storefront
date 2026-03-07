"use client";

import { useState, useEffect, useCallback } from "react";
import Link from "next/link";
import Image from "next/image";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import type { HeroSlideDto } from "@/lib/api";

interface HeroSliderProps {
  slides: HeroSlideDto[];
  autoPlayInterval?: number;
}

export function HeroSlider({ slides, autoPlayInterval = 5000 }: HeroSliderProps) {
  const [current, setCurrent] = useState(0);

  const next = useCallback(() => {
    setCurrent((prev) => (prev + 1) % slides.length);
  }, [slides.length]);

  const prev = useCallback(() => {
    setCurrent((p) => (p === 0 ? slides.length - 1 : p - 1));
  }, [slides.length]);

  useEffect(() => {
    if (slides.length <= 1) return;
    const id = setInterval(next, autoPlayInterval);
    return () => clearInterval(id);
  }, [next, autoPlayInterval, slides.length]);

  if (!slides.length) return null;

  return (
    <section className="relative w-full overflow-hidden bg-muted">
      <div className="relative aspect-[3/1] min-h-[200px] md:min-h-[320px]">
        {slides.map((slide, i) => (
          <div
            key={slide.id}
            className={cn(
              "absolute inset-0 transition-opacity duration-500",
              i === current ? "opacity-100 z-10" : "opacity-0 z-0"
            )}
          >
            <Link href={slide.link} className="block h-full w-full">
              <div className="relative h-full w-full">
                <Image
                  src={slide.imageUrl}
                  alt={slide.title}
                  fill
                  className="object-cover"
                  sizes="100vw"
                  priority={i === 0}
                />
                <div
                  className={cn(
                    "absolute inset-0 bg-gradient-to-r from-black/60 via-black/30 to-transparent flex flex-col justify-center px-8 md:px-16"
                  )}
                >
                  <h2 className="text-2xl md:text-4xl font-bold text-white drop-shadow-lg max-w-xl">
                    {slide.title}
                  </h2>
                  {slide.subtitle && (
                    <p className="mt-2 text-base md:text-xl text-white/90 max-w-md">
                      {slide.subtitle}
                    </p>
                  )}
                  <span className="mt-6 inline-flex h-11 items-center justify-center rounded-md bg-white px-8 text-sm font-medium text-gray-900 hover:bg-gray-100 transition-colors w-fit">
                    {slide.linkText}
                  </span>
                </div>
              </div>
            </Link>
          </div>
        ))}
      </div>

      {/* Arrows */}
      {slides.length > 1 && (
        <>
          <button
            type="button"
            onClick={prev}
            className="absolute left-4 top-1/2 -translate-y-1/2 z-20 rounded-full bg-black/40 p-2 text-white hover:bg-black/60 transition-colors"
            aria-label="Önceki"
          >
            <ChevronLeft className="h-6 w-6" />
          </button>
          <button
            type="button"
            onClick={next}
            className="absolute right-4 top-1/2 -translate-y-1/2 z-20 rounded-full bg-black/40 p-2 text-white hover:bg-black/60 transition-colors"
            aria-label="Sonraki"
          >
            <ChevronRight className="h-6 w-6" />
          </button>
        </>
      )}

      {/* Dots */}
      {slides.length > 1 && (
        <div className="absolute bottom-4 left-1/2 -translate-x-1/2 z-20 flex gap-2">
          {slides.map((_, i) => (
            <button
              key={i}
              type="button"
              onClick={() => setCurrent(i)}
              className={cn(
                "h-2 rounded-full transition-all",
                i === current ? "w-8 bg-white" : "w-2 bg-white/50 hover:bg-white/70"
              )}
              aria-label={`Slide ${i + 1}`}
            />
          ))}
        </div>
      )}
    </section>
  );
}
