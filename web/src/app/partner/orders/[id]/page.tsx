"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { partnerOrdersApi, CommentType } from "@/lib/api/orders";
import { OrderStatusBadge } from "@/components/orders/order-status-badge";
import { OrderTimeline } from "@/components/orders/order-timeline";
import { CartItemCard } from "@/components/orders/cart-item-card";
import { CommentThread } from "@/components/orders/comment-thread";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, MapPin, Calendar, Package, Truck } from "lucide-react";
import { toast } from "sonner";

export default function PartnerOrderDetailsPage({
  params,
}: {
  params: { id: string };
}) {
  const router = useRouter();
  const queryClient = useQueryClient();

  const { data: order, isLoading } = useQuery({
    queryKey: ["partner-order", params.id],
    queryFn: () => partnerOrdersApi.getOrderDetails(params.id),
  });

  const addCommentMutation = useMutation({
    mutationFn: ({ content, type }: { content: string; type: CommentType }) =>
      partnerOrdersApi.addComment(params.id, {
        content,
        type,
      }),
    onSuccess: () => {
      toast.success("Comment added successfully");
      queryClient.invalidateQueries({ queryKey: ["partner-order", params.id] });
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || "Failed to add comment");
    },
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!order) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card className="p-12 text-center">
          <Package className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h2 className="text-xl font-medium text-gray-900 mb-2">
            Order not found
          </h2>
          <Button onClick={() => router.push("/partner/orders")}>
            Back to Orders
          </Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <Button
        variant="ghost"
        onClick={() => router.push("/partner/orders")}
        className="mb-6"
      >
        <ArrowLeft className="w-4 h-4 mr-2" />
        Back to Orders
      </Button>

      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold mb-2">{order.orderNumber}</h1>
          <div className="flex items-center gap-3">
            <OrderStatusBadge status={order.status} />
            <span className="text-sm text-gray-600">
              Created {new Date(order.createdAt).toLocaleDateString()}
            </span>
          </div>
        </div>
      </div>

      {/* Timeline */}
      <Card className="p-6 mb-6">
        <OrderTimeline
          currentStatus={order.status}
          createdAt={order.createdAt}
          submittedAt={order.submittedAt}
          confirmedAt={order.confirmedAt}
        />
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column */}
        <div className="lg:col-span-2 space-y-6">
          {/* Order Items */}
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <Package className="w-5 h-5" />
              Order Items ({order.items.length})
            </h2>
            <div className="space-y-3">
              {order.items.map((item) => (
                <CartItemCard key={item.id} item={item} readOnly />
              ))}
            </div>
          </Card>

          {/* Comments */}
          <Card className="p-6">
            <CommentThread
              comments={order.comments}
              onAddComment={(content, type) =>
                addCommentMutation.mutateAsync({ content, type })
              }
              isAdmin={false}
            />
          </Card>
        </div>

        {/* Right Column */}
        <div className="space-y-6">
          {/* Delivery Information */}
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <MapPin className="w-5 h-5" />
              Delivery Address
            </h2>
            <div className="text-sm space-y-1">
              <p>{order.deliveryAddress}</p>
              <p>
                {order.deliveryCity}, {order.deliveryState}{" "}
                {order.deliveryPostalCode}
              </p>
              <p>{order.deliveryCountry}</p>
            </div>
            {order.deliveryNotes && (
              <div className="mt-4 p-3 bg-gray-50 rounded text-sm">
                <span className="font-medium">Delivery Notes:</span>
                <p className="text-gray-700 mt-1">{order.deliveryNotes}</p>
              </div>
            )}
          </Card>

          {/* Dates */}
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <Calendar className="w-5 h-5" />
              Important Dates
            </h2>
            <div className="space-y-3 text-sm">
              {order.requestedDeliveryDate && (
                <div>
                  <span className="text-gray-600">Requested Delivery:</span>
                  <p className="font-medium">
                    {new Date(order.requestedDeliveryDate).toLocaleDateString()}
                  </p>
                </div>
              )}
              {order.expectedDeliveryDate && (
                <div>
                  <span className="text-gray-600">Expected Delivery:</span>
                  <p className="font-medium">
                    {new Date(order.expectedDeliveryDate).toLocaleDateString()}
                  </p>
                </div>
              )}
              {order.submittedAt && (
                <div>
                  <span className="text-gray-600">Order Submitted:</span>
                  <p className="font-medium">
                    {new Date(order.submittedAt).toLocaleDateString()}
                  </p>
                </div>
              )}
            </div>
          </Card>

          {/* Shipping Info */}
          {(order.trackingNumber || order.shippingProvider) && (
            <Card className="p-6">
              <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
                <Truck className="w-5 h-5" />
                Shipping
              </h2>
              <div className="space-y-3 text-sm">
                {order.shippingProvider && (
                  <div>
                    <span className="text-gray-600">Provider:</span>
                    <p className="font-medium">{order.shippingProvider}</p>
                  </div>
                )}
                {order.trackingNumber && (
                  <div>
                    <span className="text-gray-600">Tracking Number:</span>
                    <p className="font-medium font-mono">
                      {order.trackingNumber}
                    </p>
                  </div>
                )}
              </div>
            </Card>
          )}

          {/* Pricing */}
          {order.totalAmount && (
            <Card className="p-6 bg-blue-50">
              <h2 className="text-lg font-semibold mb-4">Order Total</h2>
              <div className="space-y-2 text-sm">
                {order.subTotal && (
                  <div className="flex justify-between">
                    <span>Subtotal:</span>
                    <span>{order.currency} {order.subTotal.toFixed(2)}</span>
                  </div>
                )}
                {order.taxAmount && (
                  <div className="flex justify-between">
                    <span>Tax:</span>
                    <span>{order.currency} {order.taxAmount.toFixed(2)}</span>
                  </div>
                )}
                {order.shippingCost && (
                  <div className="flex justify-between">
                    <span>Shipping:</span>
                    <span>{order.currency} {order.shippingCost.toFixed(2)}</span>
                  </div>
                )}
                {order.discount && (
                  <div className="flex justify-between text-green-600">
                    <span>Discount:</span>
                    <span>-{order.currency} {order.discount.toFixed(2)}</span>
                  </div>
                )}
                <div className="flex justify-between font-bold text-lg pt-2 border-t">
                  <span>Total:</span>
                  <span>{order.currency} {order.totalAmount.toFixed(2)}</span>
                </div>
              </div>
            </Card>
          )}

          {/* Notes */}
          {order.notes && (
            <Card className="p-6">
              <h2 className="text-lg font-semibold mb-3">Order Notes</h2>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">
                {order.notes}
              </p>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
