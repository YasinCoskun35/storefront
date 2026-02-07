"use client";

import { useQuery } from "@tanstack/react-query";
import { catalogApi, contentApi } from "@/lib/api";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Package, AlertTriangle, FileText, TrendingUp } from "lucide-react";

export default function AdminDashboardPage() {
  // Fetch stats
  const { data: productsData } = useQuery({
    queryKey: ["admin-products-stats"],
    queryFn: () => catalogApi.searchProducts({ pageSize: 1 }),
  });

  const { data: blogData } = useQuery({
    queryKey: ["admin-blog-stats"],
    queryFn: () => contentApi.getBlogPosts({ pageSize: 1 }),
  });

  // Calculate low stock items (mock for now)
  const lowStockCount = 5; // Would need a separate endpoint

  const stats = [
    {
      title: "Total Products",
      value: productsData?.totalCount || 0,
      icon: Package,
      description: "Products in catalog",
    },
    {
      title: "Low Stock Items",
      value: lowStockCount,
      icon: AlertTriangle,
      description: "Products below threshold",
      alert: true,
    },
    {
      title: "Blog Posts",
      value: blogData?.totalCount || 0,
      icon: FileText,
      description: "Published articles",
    },
    {
      title: "Views This Month",
      value: "1,234",
      icon: TrendingUp,
      description: "Product views",
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Overview of your store</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <Card key={stat.title}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  {stat.title}
                </CardTitle>
                <Icon
                  className={`h-4 w-4 ${
                    stat.alert ? "text-destructive" : "text-muted-foreground"
                  }`}
                />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stat.value}</div>
                <p className="text-xs text-muted-foreground mt-1">
                  {stat.description}
                </p>
              </CardContent>
            </Card>
          );
        })}
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Quick Actions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            <a
              href="/admin/products/new"
              className="block p-3 rounded-lg hover:bg-accent transition-colors"
            >
              <div className="font-medium">Add New Product</div>
              <div className="text-sm text-muted-foreground">
                Create a new product listing
              </div>
            </a>
            <a
              href="/admin/blog/new"
              className="block p-3 rounded-lg hover:bg-accent transition-colors"
            >
              <div className="font-medium">Write Blog Post</div>
              <div className="text-sm text-muted-foreground">
                Publish a new article
              </div>
            </a>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Recent Activity</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">
              No recent activity to display
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

