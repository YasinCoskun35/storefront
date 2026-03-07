"use client";

import { use } from "react";
import { useQuery } from "@tanstack/react-query";
import { catalogApi } from "@/lib/api";
import { CategoryForm } from "@/components/admin/category-form";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import { Loader2 } from "lucide-react";

export default function EditCategoryPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data: categories, isLoading } = useQuery({
    queryKey: ["admin-categories", "all"],
    queryFn: () => catalogApi.getCategories({ all: true }),
  });

  const category = categories?.find((c) => c.id === id);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!category) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Link href="/admin/categories">
            <button className="flex h-10 w-10 items-center justify-center rounded-lg border bg-background hover:bg-muted">
              <ArrowLeft className="h-5 w-5" />
            </button>
          </Link>
          <h1 className="font-display text-3xl font-bold text-secondary">Category Not Found</h1>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/admin/categories">
          <button className="flex h-10 w-10 items-center justify-center rounded-lg border bg-background hover:bg-muted">
            <ArrowLeft className="h-5 w-5" />
          </button>
        </Link>
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary">Edit Category</h1>
          <p className="text-muted-foreground">Update &quot;{category.name}&quot;</p>
        </div>
      </div>

      <CategoryForm categoryId={id} initialData={category} />
    </div>
  );
}
