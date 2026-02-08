"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { partnerOrdersApi } from "@/lib/api/orders";
import { CartItemCard } from "@/components/orders/cart-item-card";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { ShoppingCart, ArrowRight } from "lucide-react";
import { toast } from "sonner";

export default function PartnerCartPage() {
  const router = useRouter();
  const queryClient = useQueryClient();

  // Fetch cart
  const { data: cart, isLoading } = useQuery({
    queryKey: ["partner-cart"],
    queryFn: () => partnerOrdersApi.getCart(),
  });

  const handleCheckout = () => {
    if (!cart || cart.itemCount === 0) {
      toast.error("Your cart is empty");
      return;
    }
    router.push("/partner/checkout");
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="container max-w-4xl mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-6">
        <ShoppingCart className="w-8 h-8 text-blue-600" />
        <div>
          <h1 className="text-3xl font-bold">Shopping Cart</h1>
          <p className="text-gray-600 mt-1">
            {cart?.itemCount || 0} {cart?.itemCount === 1 ? "item" : "items"}
          </p>
        </div>
      </div>

      {!cart || cart.itemCount === 0 ? (
        <Card className="p-12 text-center">
          <ShoppingCart className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h2 className="text-xl font-medium text-gray-900 mb-2">
            Your cart is empty
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
          {/* Cart Items */}
          <div className="space-y-4">
            {cart.items.map((item) => (
              <CartItemCard
                key={item.id}
                item={item}
                readOnly={false}
                onRemove={(itemId) => {
                  toast.info("Remove item feature coming soon");
                  // TODO: Implement remove item
                }}
                onUpdateQuantity={(itemId, quantity) => {
                  toast.info("Update quantity feature coming soon");
                  // TODO: Implement update quantity
                }}
              />
            ))}
          </div>

          {/* Summary Card */}
          <Card className="p-6 bg-gray-50">
            <div className="space-y-3">
              <div className="flex items-center justify-between text-lg font-medium">
                <span>Total Items:</span>
                <span>{cart.itemCount}</span>
              </div>
              
              <p className="text-sm text-gray-600">
                Pricing will be provided after order review
              </p>

              <Button 
                onClick={handleCheckout} 
                size="lg" 
                className="w-full"
              >
                Proceed to Checkout
                <ArrowRight className="w-5 h-5 ml-2" />
              </Button>
            </div>
          </Card>
        </div>
      )}
    </div>
  );
}
