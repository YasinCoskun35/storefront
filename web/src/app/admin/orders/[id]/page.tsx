"use client";

import { use, useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminOrdersApi, CommentType, OrderStatus, ORDER_STATUS_LABELS, OrderDetails } from "@/lib/api/orders";
import { settingsApi } from "@/lib/api/settings";
import { OrderStatusBadge } from "@/components/orders/order-status-badge";
import { OrderTimeline } from "@/components/orders/order-timeline";
import { CartItemCard } from "@/components/orders/cart-item-card";
import { CommentThread } from "@/components/orders/comment-thread";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ArrowLeft, MapPin, Calendar, Package, Truck, Edit, DollarSign } from "lucide-react";
import { toast } from "sonner";

export default function AdminOrderDetailsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const router = useRouter();
  const queryClient = useQueryClient();
  const [showStatusDialog, setShowStatusDialog] = useState(false);
  const [showPricingDialog, setShowPricingDialog] = useState(false);
  const [showShippingDialog, setShowShippingDialog] = useState(false);
  const [shippingForm, setShippingForm] = useState({
    trackingNumber: "",
    shippingProvider: "",
    expectedDeliveryDate: "",
    shippingNotes: "",
  });
  const [newStatus, setNewStatus] = useState<OrderStatus>(OrderStatus.Pending);
  const [statusNotes, setStatusNotes] = useState("");
  const [pricingForm, setPricingForm] = useState<{
    items: Record<string, { unitPrice: string; discount: string }>;
    shippingCost: string;
    taxAmount: string;
    discount: string;
    currency: string;
    notes: string;
  }>({
    items: {},
    shippingCost: "",
    taxAmount: "",
    discount: "",
    currency: "USD",
    notes: "",
  });

  const { data: order, isLoading } = useQuery({
    queryKey: ["admin-order", id],
    queryFn: () => adminOrdersApi.getOrderDetails(id),
  });

  const { data: pricingEnabled } = useQuery({
    queryKey: ["setting-pricing-enabled"],
    queryFn: () => settingsApi.isFeatureEnabled("Features.Pricing.Enabled"),
    staleTime: 60_000,
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ status, notes }: { status: number; notes?: string }) =>
      adminOrdersApi.updateOrderStatus(id, status, notes),
    onSuccess: () => {
      toast.success("Order status updated successfully");
      queryClient.invalidateQueries({ queryKey: ["admin-order", id] });
      queryClient.invalidateQueries({ queryKey: ["admin-orders"] });
      setShowStatusDialog(false);
      setStatusNotes("");
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || "Failed to update status");
    },
  });

  const addCommentMutation = useMutation({
    mutationFn: ({ content, type, isInternal }: { content: string; type: CommentType; isInternal: boolean }) =>
      adminOrdersApi.addComment(id, content, type, isInternal),
    onSuccess: () => {
      toast.success("Comment added successfully");
      queryClient.invalidateQueries({ queryKey: ["admin-order", id] });
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || "Failed to add comment");
    },
  });

  const setPricingMutation = useMutation({
    mutationFn: () => {
      const items = order!.items.map((item) => ({
        orderItemId: item.id,
        unitPrice: parseFloat(pricingForm.items[item.id]?.unitPrice || "0"),
        discount: parseFloat(pricingForm.items[item.id]?.discount || "0") || undefined,
      }));
      return adminOrdersApi.setOrderPricing(id, {
        items,
        shippingCost: pricingForm.shippingCost ? parseFloat(pricingForm.shippingCost) : undefined,
        taxAmount: pricingForm.taxAmount ? parseFloat(pricingForm.taxAmount) : undefined,
        discount: pricingForm.discount ? parseFloat(pricingForm.discount) : undefined,
        currency: pricingForm.currency || "USD",
        notes: pricingForm.notes || undefined,
      });
    },
    onSuccess: () => {
      toast.success("Order pricing updated successfully");
      queryClient.invalidateQueries({ queryKey: ["admin-order", id] });
      setShowPricingDialog(false);
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || "Failed to update pricing");
    },
  });

  const updateShippingMutation = useMutation({
    mutationFn: () =>
      adminOrdersApi.updateShipping(id, {
        trackingNumber: shippingForm.trackingNumber,
        shippingProvider: shippingForm.shippingProvider,
        expectedDeliveryDate: shippingForm.expectedDeliveryDate || undefined,
        shippingNotes: shippingForm.shippingNotes || undefined,
      }),
    onSuccess: () => {
      toast.success("Shipping info updated");
      queryClient.invalidateQueries({ queryKey: ["admin-order", id] });
      setShowShippingDialog(false);
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || "Failed to update shipping");
    },
  });

  const openPricingDialog = () => {
    // Pre-fill existing prices
    const items: Record<string, { unitPrice: string; discount: string }> = {};
    order?.items.forEach((item) => {
      items[item.id] = {
        unitPrice: item.unitPrice?.toString() || "",
        discount: item.discount?.toString() || "0",
      };
    });
    setPricingForm({
      items,
      shippingCost: order?.shippingCost?.toString() || "",
      taxAmount: order?.taxAmount?.toString() || "",
      discount: order?.discount?.toString() || "",
      currency: order?.currency || "USD",
      notes: "",
    });
    setShowPricingDialog(true);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600" />
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
          <Button onClick={() => router.push("/admin/orders")}>
            Back to Orders
          </Button>
        </Card>
      </div>
    );
  }

  const handleUpdateStatus = () => {
    updateStatusMutation.mutate({
      status: newStatus,
      notes: statusNotes || undefined,
    });
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <Button
        variant="ghost"
        onClick={() => router.push("/admin/orders")}
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
            <span className="text-sm text-gray-600">•</span>
            <span className="text-sm font-medium text-gray-900">
              {order.partnerCompanyName}
            </span>
          </div>
        </div>

        <div className="flex gap-2">
          {pricingEnabled && (
            <Button
              variant="outline"
              onClick={openPricingDialog}
            >
              <DollarSign className="w-4 h-4 mr-2" />
              Set Pricing
            </Button>
          )}
          <Button
            variant="outline"
            onClick={() => {
              setShippingForm({
                trackingNumber: (order as OrderDetails).trackingNumber || "",
                shippingProvider: (order as OrderDetails).shippingProvider || "",
                expectedDeliveryDate: "",
                shippingNotes: "",
              });
              setShowShippingDialog(true);
            }}
          >
            <Truck className="w-4 h-4 mr-2" />
            Shipping
          </Button>
          <Button onClick={() => setShowStatusDialog(true)}>
            <Edit className="w-4 h-4 mr-2" />
            Update Status
          </Button>
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
                <div key={item.id} className="border rounded-lg p-4">
                  <CartItemCard item={item} readOnly />
                  {(item.unitPrice || item.totalPrice) && (
                    <div className="mt-3 pt-3 border-t flex items-center justify-between text-sm">
                      <span className="text-gray-600">Price:</span>
                      <div className="text-right">
                        {item.unitPrice && (
                          <div>Unit: {order.currency} {item.unitPrice.toFixed(2)}</div>
                        )}
                        {item.totalPrice && (
                          <div className="font-medium">
                            Total: {order.currency} {item.totalPrice.toFixed(2)}
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          </Card>

          {/* Comments */}
          <Card className="p-6">
            <CommentThread
              comments={order.comments}
              onAddComment={async (content, type) => {
                // For admin, we need to handle internal notes
                await addCommentMutation.mutateAsync({
                  content,
                  type,
                  isInternal: false, // Set via checkbox in component
                });
              }}
              isAdmin={true}
            />
          </Card>
        </div>

        {/* Right Column */}
        <div className="space-y-6">
          {/* Partner Company */}
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-3">Partner Company</h2>
            <div className="text-sm">
              <p className="font-medium">{order.partnerCompanyName}</p>
              <p className="text-gray-600 text-xs mt-1">
                Company ID: {order.partnerCompanyId}
              </p>
            </div>
          </Card>

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
          <Card className="p-6">
            <h2 className="text-lg font-semibold mb-4 flex items-center gap-2">
              <Truck className="w-5 h-5" />
              Shipping
            </h2>
            {order.trackingNumber || order.shippingProvider ? (
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
            ) : (
              <p className="text-sm text-gray-500">
                Shipping information will be added when order is shipped
              </p>
            )}
          </Card>

          {/* Pricing — only visible when pricing feature is enabled */}
          {pricingEnabled && (
            order.totalAmount ? (
              <Card className="p-6 bg-purple-50">
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
            ) : (
              <Card className="p-6 bg-yellow-50">
                <h2 className="text-lg font-semibold mb-2">Pricing Pending</h2>
                <p className="text-sm text-gray-700 mb-4">
                  Set pricing for this order to send quote to partner
                </p>
                <Button
                  onClick={() => setShowPricingDialog(true)}
                  size="sm"
                  className="w-full"
                >
                  Set Pricing
                </Button>
              </Card>
            )
          )}

          {/* Internal Notes */}
          {order.notes && (
            <Card className="p-6">
              <h2 className="text-lg font-semibold mb-3">Partner Notes</h2>
              <p className="text-sm text-gray-700 whitespace-pre-wrap">
                {order.notes}
              </p>
            </Card>
          )}
        </div>
      </div>

      {/* Update Status Dialog */}
      <Dialog open={showStatusDialog} onOpenChange={setShowStatusDialog}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Update Order Status</DialogTitle>
            <DialogDescription>
              Change the status of order {order.orderNumber}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-4">
            <div>
              <Label htmlFor="newStatus">New Status</Label>
              <select
                id="newStatus"
                value={newStatus}
                onChange={(e) => setNewStatus(Number(e.target.value) as OrderStatus)}
                className="w-full px-3 py-2 border rounded-md mt-1"
              >
                {Object.entries(ORDER_STATUS_LABELS)
                  .filter(([key]) => Number(key) >= 1) // Exclude Draft
                  .map(([value, label]) => (
                    <option key={value} value={value}>
                      {label}
                    </option>
                  ))}
              </select>
            </div>

            <div>
              <Label htmlFor="statusNotes">Notes (optional)</Label>
              <Textarea
                id="statusNotes"
                value={statusNotes}
                onChange={(e) => setStatusNotes(e.target.value)}
                placeholder="Add any notes about this status change..."
                rows={3}
              />
            </div>
          </div>

          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setShowStatusDialog(false)}
            >
              Cancel
            </Button>
            <Button
              onClick={handleUpdateStatus}
              disabled={updateStatusMutation.isPending}
            >
              {updateStatusMutation.isPending ? "Updating..." : "Update Status"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Pricing Dialog */}
      <Dialog open={showPricingDialog} onOpenChange={setShowPricingDialog}>
        <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Set Order Pricing</DialogTitle>
            <DialogDescription>
              Set prices for each item and order-level costs for {order.orderNumber}
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-6 py-2">
            {/* Item Pricing */}
            <div>
              <h3 className="text-sm font-semibold mb-3">Item Prices</h3>
              <div className="space-y-3">
                {order.items.map((item) => (
                  <div key={item.id} className="flex items-center gap-3 p-3 border rounded-lg">
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-sm truncate">{item.productName}</p>
                      <p className="text-xs text-muted-foreground">
                        {item.productSKU} · Qty: {item.quantity}
                      </p>
                    </div>
                    <div className="flex gap-2">
                      <div className="w-28">
                        <Label className="text-xs text-muted-foreground">Unit Price</Label>
                        <Input
                          type="number"
                          min="0"
                          step="0.01"
                          placeholder="0.00"
                          value={pricingForm.items[item.id]?.unitPrice || ""}
                          onChange={(e) =>
                            setPricingForm((prev) => ({
                              ...prev,
                              items: {
                                ...prev.items,
                                [item.id]: {
                                  ...prev.items[item.id],
                                  unitPrice: e.target.value,
                                },
                              },
                            }))
                          }
                          className="h-8 text-sm"
                        />
                      </div>
                      <div className="w-24">
                        <Label className="text-xs text-muted-foreground">Discount</Label>
                        <Input
                          type="number"
                          min="0"
                          step="0.01"
                          placeholder="0.00"
                          value={pricingForm.items[item.id]?.discount || ""}
                          onChange={(e) =>
                            setPricingForm((prev) => ({
                              ...prev,
                              items: {
                                ...prev.items,
                                [item.id]: {
                                  ...prev.items[item.id],
                                  discount: e.target.value,
                                },
                              },
                            }))
                          }
                          className="h-8 text-sm"
                        />
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Order-Level Costs */}
            <div>
              <h3 className="text-sm font-semibold mb-3">Order Costs</h3>
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-1">
                  <Label className="text-xs">Currency</Label>
                  <Input
                    value={pricingForm.currency}
                    onChange={(e) => setPricingForm({ ...pricingForm, currency: e.target.value })}
                    placeholder="USD"
                  />
                </div>
                <div className="space-y-1">
                  <Label className="text-xs">Shipping Cost</Label>
                  <Input
                    type="number"
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                    value={pricingForm.shippingCost}
                    onChange={(e) => setPricingForm({ ...pricingForm, shippingCost: e.target.value })}
                  />
                </div>
                <div className="space-y-1">
                  <Label className="text-xs">Tax Amount</Label>
                  <Input
                    type="number"
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                    value={pricingForm.taxAmount}
                    onChange={(e) => setPricingForm({ ...pricingForm, taxAmount: e.target.value })}
                  />
                </div>
                <div className="space-y-1">
                  <Label className="text-xs">Order Discount</Label>
                  <Input
                    type="number"
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                    value={pricingForm.discount}
                    onChange={(e) => setPricingForm({ ...pricingForm, discount: e.target.value })}
                  />
                </div>
              </div>
              <div className="mt-3 space-y-1">
                <Label className="text-xs">Notes</Label>
                <Input
                  placeholder="Pricing notes (optional)"
                  value={pricingForm.notes}
                  onChange={(e) => setPricingForm({ ...pricingForm, notes: e.target.value })}
                />
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowPricingDialog(false)}>
              Cancel
            </Button>
            <Button onClick={() => setPricingMutation.mutate()} disabled={setPricingMutation.isPending}>
              {setPricingMutation.isPending && (
                <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
              )}
              Save Pricing
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Shipping Dialog */}
      <Dialog open={showShippingDialog} onOpenChange={setShowShippingDialog}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Update Shipping Info</DialogTitle>
            <DialogDescription>Add tracking number and carrier details</DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-1">
              <Label>Carrier / Shipping Provider</Label>
              <Input
                placeholder="e.g. DHL, FedEx, UPS"
                value={shippingForm.shippingProvider}
                onChange={(e) => setShippingForm({ ...shippingForm, shippingProvider: e.target.value })}
              />
            </div>
            <div className="space-y-1">
              <Label>Tracking Number</Label>
              <Input
                placeholder="Tracking number"
                value={shippingForm.trackingNumber}
                onChange={(e) => setShippingForm({ ...shippingForm, trackingNumber: e.target.value })}
              />
            </div>
            <div className="space-y-1">
              <Label>Expected Delivery Date</Label>
              <Input
                type="date"
                value={shippingForm.expectedDeliveryDate}
                onChange={(e) => setShippingForm({ ...shippingForm, expectedDeliveryDate: e.target.value })}
              />
            </div>
            <div className="space-y-1">
              <Label>Shipping Notes (visible to partner)</Label>
              <Textarea
                placeholder="Any notes about shipping..."
                rows={2}
                value={shippingForm.shippingNotes}
                onChange={(e) => setShippingForm({ ...shippingForm, shippingNotes: e.target.value })}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowShippingDialog(false)}>Cancel</Button>
            <Button
              disabled={!shippingForm.trackingNumber || !shippingForm.shippingProvider || updateShippingMutation.isPending}
              onClick={() => updateShippingMutation.mutate()}
            >
              {updateShippingMutation.isPending && (
                <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
              )}
              Save Shipping Info
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
