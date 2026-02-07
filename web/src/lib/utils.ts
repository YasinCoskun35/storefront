import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

export function getImageUrl(url: string | undefined): string {
  if (!url) return "/placeholder-product.jpg";
  
  // If URL starts with /, it's a relative URL from the API
  if (url.startsWith("/")) {
    return `http://localhost:8080${url}`;
  }
  
  return url;
}

