"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { partnerOrdersApi, OrderStatus } from "@/lib/api/orders";
import { OrderStatusBadge } from "@/components/orders/order-status-badge";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Package, Search, ChevronRight, ShoppingCart } from "lucide-react";
import { useRouter } from "next/navigation";

export default function PartnerOrdersPage() {
  const router = useRouter();
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [searchTerm, setSearchTerm] = useState("");

  const { data: ordersResponse, isLoading } = useQuery({
    queryKey: ["partner-orders", statusFilter],
    queryFn: () =>
      partnerOrdersApi.getOrders({
        status: statusFilter || undefined,
        pageNumber: 1,
        pageSize: 50,
      }),
  });

  const orders = ordersResponse?.items || [];

  // Filter orders by search term
  const filteredOrders = orders.filter((order) =>
    order.orderNumber.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <Package className="w-8 h-8 text-blue-600" />
          <div>
            <h1 className="text-3xl font-bold">My Orders</h1>
            <p className="text-gray-600 mt-1">
              Track and manage your order requests
            </p>
          </div>
        </div>

        <Button onClick={() => router.push("/partner/cart")}>
          <ShoppingCart className="w-4 h-4 mr-2" />
          View Cart
        </Button>
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4 mb-6">
        <div className="flex-1 relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <Input
            type="search"
            placeholder="Search by order number..."
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
          <option value="">All Statuses</option>
          <option value="Pending">Pending</option>
          <option value="QuoteSent">Quote Sent</option>
          <option value="Confirmed">Confirmed</option>
          <option value="Preparing">Preparing</option>
          <option value="Shipping">Shipping</option>
          <option value="Delivered">Delivered</option>
        </select>
      </div>

      {/* Orders List */}
      {isLoading ? (
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
      ) : filteredOrders.length === 0 ? (
        <Card className="p-12 text-center">
          <Package className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h2 className="text-xl font-medium text-gray-900 mb-2">
            {searchTerm || statusFilter ? "No orders found" : "No orders yet"}
          </h2>
          <p className="text-gray-600 mb-6">
            {searchTerm || statusFilter
              ? "Try adjusting your search or filters"
              : "Create your first order to get started"}
          </p>
          {!searchTerm && !statusFilter && (
            <Button onClick={() => router.push("/products")}>
              Browse Products
            </Button>
          )}
        </Card>
      ) : (
        <div className="space-y-4">
          {filteredOrders.map((order) => (
            <Link key={order.id} href={`/partner/orders/${order.id}`}>
              <Card className="p-6 hover:shadow-md transition-shadow cursor-pointer">
                <div className="flex items-center justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <h3 className="text-lg font-semibold">
                        {order.orderNumber}
                      </h3>
                      <OrderStatusBadge status={order.status} />
                    </div>

                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-sm text-gray-600">
                      <div>
                        <span className="block text-xs text-gray-500">
                          Created
                        </span>
                        {new Date(order.createdAt).toLocaleDateString()}
                      </div>

                      <div>
                        <span className="block text-xs text-gray-500">
                          Items
                        </span>
                        {order.itemCount}
                      </div>

                      {order.totalAmount && (
                        <div>
                          <span className="block text-xs text-gray-500">
                            Total
                          </span>
                          {order.currency} {order.totalAmount.toFixed(2)}
                        </div>
                      )}

                      {order.requestedDeliveryDate && (
                        <div>
                          <span className="block text-xs text-gray-500">
                            Requested Delivery
                          </span>
                          {new Date(
                            order.requestedDeliveryDate
                          ).toLocaleDateString()}
                        </div>
                      )}
                    </div>
                  </div>

                  <ChevronRight className="w-5 h-5 text-gray-400" />
                </div>
              </Card>
            </Link>
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
