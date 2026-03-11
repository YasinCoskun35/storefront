"use client";

import { useQuery } from "@tanstack/react-query";
import { formatPrice } from "@/lib/utils";
import { partnerPublicApi } from "@/lib/api/partners";

interface Props {
  price: number | null;
  compareAtPrice?: number | null;
}

export function PartnerPriceDisplay({ price, compareAtPrice }: Props) {
  const { data: profile } = useQuery({
    queryKey: ["partner-profile"],
    queryFn: partnerPublicApi.getProfile,
    staleTime: 60_000,
    // If the user isn't logged in as a partner, the request will 401 and
    // TanStack Query will set data to undefined — we just show the plain price.
    retry: false,
  });

  if (price === null || price === undefined) {
    return null;
  }

  const discountRate = profile?.discountRate ?? 0;
  const hasPartnerDiscount = discountRate > 0;
  const discountedPrice = hasPartnerDiscount
    ? Math.round(price * (1 - discountRate / 100) * 100) / 100
    : price;

  if (hasPartnerDiscount) {
    return (
      <div className="flex flex-col gap-0.5">
        <div className="flex items-baseline gap-2">
          <span className="text-2xl font-bold text-green-700">
            {formatPrice(discountedPrice)}
          </span>
          <span className="text-sm text-muted-foreground line-through">
            {formatPrice(price)}
          </span>
        </div>
        <span className="text-xs text-green-600 font-medium">
          Partner price (−{discountRate}% discount)
        </span>
      </div>
    );
  }

  return (
    <div className="flex items-baseline gap-2">
      <span className="text-2xl font-bold">{formatPrice(price)}</span>
      {compareAtPrice && compareAtPrice > price && (
        <span className="text-sm text-muted-foreground line-through">
          {formatPrice(compareAtPrice)}
        </span>
      )}
    </div>
  );
}
