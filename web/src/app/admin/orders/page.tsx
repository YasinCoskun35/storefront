"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminOrdersApi, OrderSummary, OrderStatus } from "@/lib/api/orders";
import { OrderStatusBadge } from "@/components/orders/order-status-badge";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Package, Search, ChevronRight, CheckCircle, Clock } from "lucide-react";
import { toast } from "sonner";
import { useTranslations } from "next-intl";

export default function AdminOrdersPage() {
  const t = useTranslations("orders");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [searchTerm, setSearchTerm] = useState("");
  const queryClient = useQueryClient();

  const { data: ordersResponse, isLoading } = useQuery({
    queryKey: ["admin-orders", statusFilter],
    queryFn: () =>
      adminOrdersApi.getOrders({
        status: statusFilter || undefined,
        pageNumber: 1,
        pageSize: 50,
      }),
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ orderId, status }: { orderId: string; status: number }) =>
      adminOrdersApi.updateOrderStatus(orderId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-orders"] });
      toast.success("Order status updated");
    },
    onError: () => toast.error("Failed to update order status"),
  });

  const orders: OrderSummary[] = ordersResponse?.items || [];

  // Filter orders by search term
  const filteredOrders = orders.filter((order) =>
    order.orderNumber.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex items-center gap-3 mb-6">
        <Package className="w-8 h-8 text-purple-600" />
        <div>
          <h1 className="text-3xl font-bold">{t("adminTitle")}</h1>
          <p className="text-gray-600 mt-1">
            {t("adminTitleDesc")}
          </p>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-4 gap-4 mb-6">
        <Card className="p-4">
          <div className="text-sm text-gray-600">Total Orders</div>
          <div className="text-2xl font-bold">{ordersResponse?.totalCount || 0}</div>
        </Card>
        <Card className="p-4 bg-yellow-50">
          <div className="text-sm text-gray-600">Pending</div>
          <div className="text-2xl font-bold">
            {orders.filter((o) => o.status === "Pending").length}
          </div>
        </Card>
        <Card className="p-4 bg-blue-50">
          <div className="text-sm text-gray-600">Quote Sent</div>
          <div className="text-2xl font-bold">
            {orders.filter((o) => o.status === "QuoteSent").length}
          </div>
        </Card>
        <Card className="p-4 bg-green-50">
          <div className="text-sm text-gray-600">Confirmed</div>
          <div className="text-2xl font-bold">
            {orders.filter((o) => o.status === "Confirmed").length}
          </div>
        </Card>
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4 mb-6">
        <div className="flex-1 relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <Input
            type="search"
            placeholder={t("searchPlaceholder")}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>

        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="px-4 py-2 border rounded-md bg-white"
        >
          <option value="">{t("allStatuses")}</option>
          <option value="Pending">Pending</option>
          <option value="QuoteSent">Quote Sent</option>
          <option value="Confirmed">Confirmed</option>
          <option value="Preparing">Preparing</option>
          <option value="QualityCheck">Quality Check</option>
          <option value="ReadyToShip">Ready to Ship</option>
          <option value="Shipping">Shipping</option>
          <option value="Delivered">Delivered</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>

      {/* Orders List */}
      {isLoading ? (
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600" />
        </div>
      ) : filteredOrders.length === 0 ? (
        <Card className="p-12 text-center">
          <Package className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h2 className="text-xl font-medium text-gray-900 mb-2">
            {searchTerm || statusFilter ? t("noOrders") : t("noOrdersYet")}
          </h2>
          <p className="text-gray-600">
            {searchTerm || statusFilter
              ? t("tryAdjusting")
              : t("willAppear")}
          </p>
        </Card>
      ) : (
        <div className="space-y-4">
          {filteredOrders.map((order) => (
            <Card key={order.id} className="p-6 hover:shadow-md transition-shadow">
              <div className="flex items-center justify-between">
                <Link href={`/admin/orders/${order.id}`} className="flex-1 min-w-0">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="text-lg font-semibold">
                      {order.orderNumber}
                    </h3>
                    <OrderStatusBadge status={order.status} />
                    {order.hasUnreadComments && (
                      <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
                        New
                      </span>
                    )}
                  </div>

                  <div className="grid grid-cols-2 sm:grid-cols-5 gap-4 text-sm text-gray-600">
                    <div>
                      <span className="block text-xs text-gray-500">Created</span>
                      {new Date(order.createdAt).toLocaleDateString()}
                    </div>
                    <div>
                      <span className="block text-xs text-gray-500">Items</span>
                      {order.itemCount}
                    </div>
                    {order.totalAmount && (
                      <div>
                        <span className="block text-xs text-gray-500">Total</span>
                        {order.currency} {order.totalAmount.toFixed(2)}
                      </div>
                    )}
                    {order.requestedDeliveryDate && (
                      <div>
                        <span className="block text-xs text-gray-500">Requested Delivery</span>
                        {new Date(order.requestedDeliveryDate).toLocaleDateString()}
                      </div>
                    )}
                  </div>
                </Link>

                <div className="flex items-center gap-2 ml-4 shrink-0">
                  {order.status === "Pending" && (
                    <>
                      <Button
                        size="sm"
                        variant="outline"
                        className="text-green-700 border-green-300 hover:bg-green-50"
                        onClick={(e) => {
                          e.preventDefault();
                          updateStatusMutation.mutate({ orderId: order.id, status: OrderStatus.Confirmed });
                        }}
                        disabled={updateStatusMutation.isPending}
                      >
                        <CheckCircle className="w-3 h-3 mr-1" />
                        Approve
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        className="text-orange-700 border-orange-300 hover:bg-orange-50"
                        onClick={(e) => {
                          e.preventDefault();
                          updateStatusMutation.mutate({ orderId: order.id, status: OrderStatus.PendingPayment });
                        }}
                        disabled={updateStatusMutation.isPending}
                      >
                        <Clock className="w-3 h-3 mr-1" />
                        Pend Payment
                      </Button>
                    </>
                  )}
                  <Link href={`/admin/orders/${order.id}`}>
                    <ChevronRight className="w-5 h-5 text-gray-400" />
                  </Link>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Pagination */}
      {ordersResponse && ordersResponse.totalPages > 1 && (
        <div className="mt-6 flex items-center justify-center gap-2">
          <Button variant="outline" disabled>
            Previous
          </Button>
          <span className="text-sm text-gray-600">
            Page {ordersResponse.pageNumber} of {ordersResponse.totalPages}
          </span>
          <Button variant="outline" disabled>
            Next
          </Button>
        </div>
      )}
    </div>
  );
}
