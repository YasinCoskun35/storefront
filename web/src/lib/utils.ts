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
  if (!url) {
    // Return a data URI for a simple gray placeholder SVG
    return "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='400'%3E%3Crect width='400' height='400' fill='%23e5e7eb'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' font-family='sans-serif' font-size='24' fill='%239ca3af'%3ENo Image%3C/text%3E%3C/svg%3E";
  }
  
  // If URL starts with /, it's a relative URL from the API
  if (url.startsWith("/")) {
    return `http://localhost:8080${url}`;
  }
  
  return url;
}

/**
 * Generates a URL-friendly slug from a string, with proper handling of Turkish characters
 * 
 * Turkish character mappings:
 * - ı → i
 * - ğ → g
 * - ü → u
 * - ş → s
 * - ö → o
 * - ç → c
 * - İ → i
 * 
 * @param text - The text to convert to a slug
 * @returns A URL-friendly slug
 * 
 * @example
 * generateSlug("Özel Ürün") // "ozel-urun"
 * generateSlug("Çok Güzel Şeyler") // "cok-guzel-seyler"
 * generateSlug("İstanbul'da Kahve") // "istanbulda-kahve"
 */
export function generateSlug(text: string): string {
  if (!text) return "";

  // Turkish character map
  const turkishMap: Record<string, string> = {
    'ı': 'i',
    'İ': 'i',
    'ğ': 'g',
    'Ğ': 'g',
    'ü': 'u',
    'Ü': 'u',
    'ş': 's',
    'Ş': 's',
    'ö': 'o',
    'Ö': 'o',
    'ç': 'c',
    'Ç': 'c',
  };

  return text
    .toLowerCase()
    // Replace Turkish characters
    .replace(/[ıİğĞüÜşŞöÖçÇ]/g, (char) => turkishMap[char] || char)
    // Replace any other special characters with their ASCII equivalents
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    // Replace spaces and special chars with hyphens
    .replace(/[^a-z0-9]+/g, '-')
    // Remove leading/trailing hyphens
    .replace(/^-+|-+$/g, '')
    // Replace multiple consecutive hyphens with single hyphen
    .replace(/-+/g, '-');
}

