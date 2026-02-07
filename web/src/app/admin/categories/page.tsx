"use client";

import { useQuery } from "@tanstack/react-query";
import { catalogApi } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Plus, Pencil, Trash2 } from "lucide-react";
import Link from "next/link";

export default function AdminCategoriesPage() {
  const { data: categories, isLoading } = useQuery({
    queryKey: ["admin-categories"],
    queryFn: () => catalogApi.getCategories(),
  });

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary">Categories</h1>
          <p className="text-muted-foreground">Manage product categories</p>
        </div>
        <Link href="/admin/categories/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Add Category
          </Button>
        </Link>
      </div>

      <div className="rounded-md border bg-card">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="px-4 py-3 text-left text-sm font-medium">Name</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Slug</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Products</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Status</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {categories?.map((category) => (
              <tr key={category.id} className="border-b hover:bg-muted/50">
                <td className="px-4 py-3 text-sm font-medium">{category.name}</td>
                <td className="px-4 py-3 text-sm text-muted-foreground font-mono">
                  {category.slug}
                </td>
                <td className="px-4 py-3 text-sm">{category.productCount}</td>
                <td className="px-4 py-3 text-sm">
                  <Badge 
                    className={category.isActive 
                      ? "bg-success text-success-foreground" 
                      : "bg-muted text-muted-foreground"
                    }
                  >
                    {category.isActive ? "Active" : "Inactive"}
                  </Badge>
                </td>
                <td className="px-4 py-3 text-sm">
                  <div className="flex items-center gap-2">
                    <Button variant="ghost" size="sm">
                      <Pencil className="h-4 w-4" />
                    </Button>
                    <Button variant="ghost" size="sm">
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

