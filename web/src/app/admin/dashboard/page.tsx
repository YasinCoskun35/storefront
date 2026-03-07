"use client";

import { useQuery } from "@tanstack/react-query";
import { catalogApi } from "@/lib/api";
import { adminOrdersApi } from "@/lib/api/orders";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
  Package,
  ShoppingCart,
  Users,
  Clock,
  CheckCircle,
  TrendingUp,
  ArrowRight,
} from "lucide-react";
import Link from "next/link";

export default function AdminDashboardPage() {
  const { data: productsData } = useQuery({
    queryKey: ["admin-products-stats"],
    queryFn: () => catalogApi.searchProducts({ pageSize: 1 }),
  });

  const { data: orderStats } = useQuery({
    queryKey: ["admin-order-stats"],
    queryFn: () => adminOrdersApi.getStats(),
  });

  const { data: recentOrders } = useQuery({
    queryKey: ["admin-recent-orders"],
    queryFn: () => adminOrdersApi.getOrders({ pageNumber: 1, pageSize: 5 }),
  });

  const stats = [
    {
      title: "Total Products",
      value: productsData?.totalCount ?? 0,
      icon: Package,
      description: "In catalog",
      href: "/admin/products",
      color: "text-blue-600",
      bg: "bg-blue-50",
    },
    {
      title: "Total Orders",
      value: orderStats?.totalOrders ?? 0,
      icon: ShoppingCart,
      description: "All time",
      href: "/admin/orders",
      color: "text-purple-600",
      bg: "bg-purple-50",
    },
    {
      title: "Pending Orders",
      value: orderStats?.pendingOrders ?? 0,
      icon: Clock,
      description: "Awaiting quote",
      href: "/admin/orders?status=Pending",
      color: "text-yellow-600",
      bg: "bg-yellow-50",
      alert: true,
    },
    {
      title: "Active Partners",
      value: orderStats?.totalPartners ?? 0,
      icon: Users,
      description: "With orders",
      href: "/admin/partners",
      color: "text-green-600",
      bg: "bg-green-50",
    },
  ];

  return (
    <div className="space-y-8">
      <div>
        <h1 className="font-display text-3xl font-bold text-secondary">Dashboard</h1>
        <p className="text-muted-foreground mt-1">Overview of your store activity</p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <Link key={stat.title} href={stat.href}>
              <Card className="hover:shadow-md transition-shadow cursor-pointer">
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium text-muted-foreground">
                    {stat.title}
                  </CardTitle>
                  <div className={`p-2 rounded-lg ${stat.bg}`}>
                    <Icon className={`h-4 w-4 ${stat.color}`} />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-3xl font-bold text-secondary">{stat.value}</div>
                  <p className="text-xs text-muted-foreground mt-1 flex items-center gap-1">
                    {stat.description}
                    {stat.alert && stat.value > 0 && (
                      <Badge variant="destructive" className="text-xs py-0 px-1">
                        Action needed
                      </Badge>
                    )}
                  </p>
                </CardContent>
              </Card>
            </Link>
          );
        })}
      </div>

      {/* Active / Completed breakdown */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-base">Order Status</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-sm text-muted-foreground">Active (In Progress)</span>
              <span className="font-semibold">{orderStats?.activeOrders ?? 0}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-muted-foreground">Pending Quotes</span>
              <span className="font-semibold text-yellow-600">{orderStats?.pendingOrders ?? 0}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-muted-foreground">Completed</span>
              <span className="font-semibold text-green-600">{orderStats?.completedOrders ?? 0}</span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-base">Quick Actions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {[
              { href: "/admin/products/new", label: "Add New Product", desc: "Create a new product listing" },
              { href: "/admin/orders", label: "View Pending Orders", desc: "Process and quote orders" },
              { href: "/admin/partners", label: "Manage Partners", desc: "View and manage business partners" },
              { href: "/admin/color-charts", label: "Color Charts", desc: "Manage material/color options" },
            ].map((action) => (
              <Link
                key={action.href}
                href={action.href}
                className="flex items-center justify-between p-3 rounded-lg hover:bg-accent transition-colors group"
              >
                <div>
                  <div className="font-medium text-sm">{action.label}</div>
                  <div className="text-xs text-muted-foreground">{action.desc}</div>
                </div>
                <ArrowRight className="h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors" />
              </Link>
            ))}
          </CardContent>
        </Card>
      </div>

      {/* Recent Orders */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-base">Recent Orders</CardTitle>
          <Link href="/admin/orders" className="text-sm text-primary hover:underline flex items-center gap-1">
            View all <ArrowRight className="h-3 w-3" />
          </Link>
        </CardHeader>
        <CardContent>
          {recentOrders?.orders?.length > 0 ? (
            <div className="space-y-2">
              {recentOrders.orders.slice(0, 5).map((order: any) => (
                <Link
                  key={order.id}
                  href={`/admin/orders/${order.id}`}
                  className="flex items-center justify-between p-3 rounded-lg hover:bg-accent transition-colors"
                >
                  <div>
                    <span className="font-mono text-sm font-medium">{order.orderNumber}</span>
                    <span className="text-sm text-muted-foreground ml-3">
                      {new Date(order.createdAt).toLocaleDateString()}
                    </span>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-sm text-muted-foreground">{order.itemCount} item{order.itemCount !== 1 ? "s" : ""}</span>
                    <Badge
                      className={
                        order.status === "Pending" ? "bg-yellow-100 text-yellow-800" :
                        order.status === "Delivered" ? "bg-green-100 text-green-800" :
                        order.status === "Cancelled" ? "bg-red-100 text-red-800" :
                        "bg-blue-100 text-blue-800"
                      }
                    >
                      {order.status}
                    </Badge>
                  </div>
                </Link>
              ))}
            </div>
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              <CheckCircle className="h-12 w-12 mx-auto mb-3 opacity-20" />
              <p>No orders yet</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
