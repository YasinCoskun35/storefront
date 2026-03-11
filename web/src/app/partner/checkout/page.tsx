"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { partnerOrdersApi, CreateOrderRequest } from "@/lib/api/orders";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { ShoppingCart, ArrowLeft, Send } from "lucide-react";
import { toast } from "sonner";
import { useTranslations } from "next-intl";

export default function CheckoutPage() {
  const router = useRouter();
  const t = useTranslations("checkout");
  const queryClient = useQueryClient();

  const [formData, setFormData] = useState<CreateOrderRequest>({
    deliveryAddress: "",
    deliveryCity: "",
    deliveryState: "",
    deliveryPostalCode: "",
    deliveryCountry: "United States",
    deliveryNotes: "",
    requestedDeliveryDate: "",
    notes: "",
  });

  // Fetch cart
  const { data: cart, isLoading } = useQuery({
    queryKey: ["partner-cart"],
    queryFn: () => partnerOrdersApi.getCart(),
  });

  // Create order mutation
  const createOrderMutation = useMutation({
    mutationFn: (data: CreateOrderRequest) => partnerOrdersApi.createOrder(data),
    onSuccess: (response) => {
      toast.success(t("orderSuccess"));
      queryClient.invalidateQueries({ queryKey: ["partner-cart"] });
      queryClient.invalidateQueries({ queryKey: ["partner-orders"] });
      router.push(`/partner/orders/${response.orderId}`);
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || t("orderFailed"));
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!cart || cart.itemCount === 0) {
      toast.error(t("emptyCartTitle"));
      return;
    }

    createOrderMutation.mutate(formData);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
      </div>
    );
  }

  if (!cart || cart.itemCount === 0) {
    return (
      <div className="container max-w-4xl mx-auto px-4 py-8">
        <Card className="p-12 text-center">
          <ShoppingCart className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h2 className="text-xl font-medium text-gray-900 mb-2">
            {t("emptyCartTitle")}
          </h2>
          <p className="text-gray-600 mb-6">
            {t("emptyCartDesc")}
          </p>
          <Button onClick={() => router.push("/products")}>
            {t("browseProducts")}
          </Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="container max-w-6xl mx-auto px-4 py-8">
      <Button
        variant="ghost"
        onClick={() => router.back()}
        className="mb-6"
      >
        <ArrowLeft className="w-4 h-4 mr-2" />
        {t("backToCart")}
      </Button>

      <h1 className="text-3xl font-bold mb-8">{t("title")}</h1>

      <form onSubmit={handleSubmit} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left Column: Forms */}
        <div className="lg:col-span-2 space-y-6">
          {/* Delivery Address */}
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4">{t("deliveryAddress")}</h2>
            <div className="space-y-4">
              <div>
                <Label htmlFor="deliveryAddress">{t("streetAddress")}</Label>
                <Input
                  id="deliveryAddress"
                  value={formData.deliveryAddress}
                  onChange={(e) =>
                    setFormData({ ...formData, deliveryAddress: e.target.value })
                  }
                  required
                  placeholder="123 Main Street, Suite 500"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="deliveryCity">{t("city")}</Label>
                  <Input
                    id="deliveryCity"
                    value={formData.deliveryCity}
                    onChange={(e) =>
                      setFormData({ ...formData, deliveryCity: e.target.value })
                    }
                    required
                  />
                </div>
                <div>
                  <Label htmlFor="deliveryState">{t("state")}</Label>
                  <Input
                    id="deliveryState"
                    value={formData.deliveryState}
                    onChange={(e) =>
                      setFormData({ ...formData, deliveryState: e.target.value })
                    }
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="deliveryPostalCode">{t("postalCode")}</Label>
                  <Input
                    id="deliveryPostalCode"
                    value={formData.deliveryPostalCode}
                    onChange={(e) =>
                      setFormData({ ...formData, deliveryPostalCode: e.target.value })
                    }
                    required
                  />
                </div>
                <div>
                  <Label htmlFor="deliveryCountry">{t("country")}</Label>
                  <Input
                    id="deliveryCountry"
                    value={formData.deliveryCountry}
                    onChange={(e) =>
                      setFormData({ ...formData, deliveryCountry: e.target.value })
                    }
                    required
                  />
                </div>
              </div>

              <div>
                <Label htmlFor="deliveryNotes">{t("deliveryNotes")}</Label>
                <Textarea
                  id="deliveryNotes"
                  value={formData.deliveryNotes}
                  onChange={(e) =>
                    setFormData({ ...formData, deliveryNotes: e.target.value })
                  }
                  placeholder="Special delivery instructions..."
                  rows={2}
                />
              </div>
            </div>
          </Card>

          {/* Order Details */}
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4">{t("orderDetails")}</h2>
            <div className="space-y-4">
              <div>
                <Label htmlFor="requestedDeliveryDate">
                  {t("requestedDeliveryDate")}
                </Label>
                <Input
                  id="requestedDeliveryDate"
                  type="date"
                  value={formData.requestedDeliveryDate}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      requestedDeliveryDate: e.target.value,
                    })
                  }
                />
              </div>

              <div>
                <Label htmlFor="notes">{t("additionalNotes")}</Label>
                <Textarea
                  id="notes"
                  value={formData.notes}
                  onChange={(e) =>
                    setFormData({ ...formData, notes: e.target.value })
                  }
                  placeholder="Any special requests or information..."
                  rows={3}
                />
              </div>
            </div>
          </Card>
        </div>

        {/* Right Column: Order Summary */}
        <div className="lg:col-span-1">
          <Card className="p-6 sticky top-4">
            <h2 className="text-lg font-semibold mb-4">{t("orderSummary")}</h2>

            <div className="space-y-3 mb-6">
              {cart.items.map((item) => (
                <div key={item.id} className="flex justify-between text-sm">
                  <span className="text-gray-600">
                    {item.productName} × {item.quantity}
                  </span>
                </div>
              ))}
            </div>

            <div className="border-t pt-4 mb-6">
              <div className="flex justify-between items-center text-lg font-medium">
                <span>Total Items:</span>
                <span>{cart.itemCount}</span>
              </div>
              <p className="text-xs text-gray-500 mt-2">
                * {t("pricingNote")}
              </p>
            </div>

            <Button
              type="submit"
              size="lg"
              className="w-full"
              disabled={createOrderMutation.isPending}
            >
              {createOrderMutation.isPending ? (
                t("submitting")
              ) : (
                <>
                  <Send className="w-5 h-5 mr-2" />
                  {t("submitOrder")}
                </>
              )}
            </Button>

            <p className="text-xs text-gray-500 mt-4 text-center">
              {t("submitNote")}
            </p>
          </Card>
        </div>
      </form>
    </div>
  );
}
