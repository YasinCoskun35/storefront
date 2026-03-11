"use client";

import { useRouter } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { partnerOrdersApi } from "@/lib/api/orders";
import { partnerPublicApi } from "@/lib/api/partners";
import { CartItemCard } from "@/components/orders/cart-item-card";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { ShoppingCart, ArrowRight, Tag } from "lucide-react";
import { toast } from "sonner";
import { formatPrice } from "@/lib/utils";
import { useTranslations } from "next-intl";

export default function PartnerCartPage() {
  const router = useRouter();
  const t = useTranslations("cart");
  const queryClient = useQueryClient();

  const { data: profile } = useQuery({
    queryKey: ["partner-profile"],
    queryFn: partnerPublicApi.getProfile,
    staleTime: 60_000,
    retry: false,
  });
  const discountRate = profile?.discountRate ?? 0;

  const { data: cart, isLoading } = useQuery({
    queryKey: ["partner-cart"],
    queryFn: () => partnerOrdersApi.getCart(),
  });

  const removeMutation = useMutation({
    mutationFn: (itemId: string) => partnerOrdersApi.removeCartItem(itemId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["partner-cart"] });
      toast.success("Item removed from cart");
    },
    onError: () => toast.error("Failed to remove item"),
  });

  const updateQuantityMutation = useMutation({
    mutationFn: ({ itemId, quantity }: { itemId: string; quantity: number }) =>
      partnerOrdersApi.updateCartItemQuantity(itemId, quantity),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["partner-cart"] });
    },
    onError: () => toast.error("Failed to update quantity"),
  });

  const handleCheckout = () => {
    if (!cart || cart.itemCount === 0) {
      toast.error("Your cart is empty");
      return;
    }
    router.push("/partner/checkout");
  };

  // Compute pricing summary
  const pricedItems = cart?.items.filter((i) => i.unitPrice != null) ?? [];
  const hasPricing = pricedItems.length > 0;

  const subtotalOriginal = pricedItems.reduce(
    (sum, i) => sum + (i.unitPrice ?? 0) * i.quantity,
    0
  );
  const subtotalDiscounted =
    discountRate > 0
      ? pricedItems.reduce((sum, i) => {
          const discounted =
            Math.round((i.unitPrice ?? 0) * (1 - discountRate / 100) * 100) / 100;
          return sum + discounted * i.quantity;
        }, 0)
      : subtotalOriginal;
  const totalDiscount = subtotalOriginal - subtotalDiscounted;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
      </div>
    );
  }

  return (
    <div className="container max-w-4xl mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-6">
        <ShoppingCart className="w-8 h-8 text-blue-600" />
        <div>
          <h1 className="text-3xl font-bold">{t("title")}</h1>
          <p className="text-gray-600 mt-1">
            {cart?.itemCount || 0} {cart?.itemCount === 1 ? "item" : "items"}
          </p>
        </div>
      </div>

      {!cart || cart.itemCount === 0 ? (
        <Card className="p-12 text-center">
          <ShoppingCart className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h2 className="text-xl font-medium text-gray-900 mb-2">
            {t("empty")}
          </h2>
          <p className="text-gray-600 mb-6">
            Browse our products and add items to your cart
          </p>
          <Button onClick={() => router.push("/products")}>
            Browse Products
          </Button>
        </Card>
      ) : (
        <div className="space-y-6">
          {/* Discount Banner */}
          {discountRate > 0 && (
            <div className="flex items-center gap-2 bg-green-50 border border-green-200 rounded-lg px-4 py-3 text-green-800 text-sm">
              <Tag className="w-4 h-4 shrink-0" />
              <span>
                {t("discountBanner")} <strong>{discountRate}%</strong> {t("discountApplied")}
              </span>
            </div>
          )}

          {/* Cart Items */}
          <div className="space-y-4">
            {cart.items.map((item) => {
              const originalLineTotal =
                item.unitPrice != null ? item.unitPrice * item.quantity : null;
              const discountedUnitPrice =
                item.unitPrice != null && discountRate > 0
                  ? Math.round(item.unitPrice * (1 - discountRate / 100) * 100) / 100
                  : item.unitPrice ?? null;
              const discountedLineTotal =
                discountedUnitPrice != null
                  ? discountedUnitPrice * item.quantity
                  : null;

              return (
                <Card key={item.id} className="p-4">
                  <CartItemCard
                    item={item}
                    readOnly={false}
                    onRemove={(itemId) => {
                      if (window.confirm("Remove this item from cart?")) {
                        removeMutation.mutate(itemId);
                      }
                    }}
                    onUpdateQuantity={(itemId, quantity) => {
                      updateQuantityMutation.mutate({ itemId, quantity });
                    }}
                  />
                  {discountedLineTotal != null && (
                    <div className="mt-2 flex items-center justify-end gap-3 text-sm border-t pt-2">
                      {discountRate > 0 && originalLineTotal != null && (
                        <span className="text-muted-foreground line-through">
                          {formatPrice(originalLineTotal)}
                        </span>
                      )}
                      <span className="font-semibold text-green-700">
                        {formatPrice(discountedLineTotal)}
                      </span>
                    </div>
                  )}
                </Card>
              );
            })}
          </div>

          {/* Summary Card */}
          <Card className="p-6 bg-gray-50">
            <div className="space-y-3">
              {hasPricing ? (
                <>
                  {discountRate > 0 && (
                    <>
                      <div className="flex items-center justify-between text-sm text-muted-foreground">
                        <span>{t("subtotalBeforeDiscount")}</span>
                        <span className="line-through">{formatPrice(subtotalOriginal)}</span>
                      </div>
                      <div className="flex items-center justify-between text-sm text-green-700">
                        <span>{t("partnerDiscount")} ({discountRate}%):</span>
                        <span>−{formatPrice(totalDiscount)}</span>
                      </div>
                    </>
                  )}
                  <div className="flex items-center justify-between text-lg font-semibold border-t pt-2">
                    <span>{t("total")}</span>
                    <span>{formatPrice(subtotalDiscounted)}</span>
                  </div>
                </>
              ) : (
                <div className="flex items-center justify-between text-lg font-medium">
                  <span>Total Items:</span>
                  <span>{cart.itemCount}</span>
                </div>
              )}

              <Button onClick={handleCheckout} size="lg" className="w-full">
                {t("proceedToCheckout")}
                <ArrowRight className="w-5 h-5 ml-2" />
              </Button>
            </div>
          </Card>
        </div>
      )}
    </div>
  );
}
