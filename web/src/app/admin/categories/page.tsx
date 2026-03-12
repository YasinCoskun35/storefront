"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { catalogApi, Category } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Plus, Pencil, Trash2 } from "lucide-react";
import Link from "next/link";
import { toast } from "sonner";

export default function AdminCategoriesPage() {
  const queryClient = useQueryClient();

  const { data: categories, isLoading } = useQuery({
    queryKey: ["admin-categories", "all"],
    queryFn: () => catalogApi.getCategories({ all: true }),
  });

  const deleteMutation = useMutation({
    mutationFn: catalogApi.deleteCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-categories", "all"] });
      toast.success("Category deleted successfully");
    },
    onError: (error: any) => {
      const message = error.response?.data?.message || "Failed to delete category";
      toast.error(message);
    },
  });

  const handleDelete = (category: Category) => {
    if (category.productCount > 0) {
      toast.error(`Cannot delete category "${category.name}" because it has ${category.productCount} products. Please move or delete the products first.`);
      return;
    }
    
    if (window.confirm(`Are you sure you want to delete "${category.name}"?`)) {
      deleteMutation.mutate(category.id);
    }
  };

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
                    <Button variant="ghost" size="sm" asChild>
                      <Link href={`/admin/categories/${category.id}`}>
                        <Pencil className="h-4 w-4" />
                      </Link>
                    </Button>
                    <Button 
                      variant="ghost" 
                      size="sm"
                      onClick={() => handleDelete(category)}
                      disabled={deleteMutation.isPending}
                    >
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

