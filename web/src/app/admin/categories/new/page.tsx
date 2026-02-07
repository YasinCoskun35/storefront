"use client";

import { CategoryForm } from "@/components/admin/category-form";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";

export default function NewCategoryPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/admin/categories">
          <button className="flex h-10 w-10 items-center justify-center rounded-lg border bg-background hover:bg-muted">
            <ArrowLeft className="h-5 w-5" />
          </button>
        </Link>
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary">Add New Category</h1>
          <p className="text-muted-foreground">Create a new product category</p>
        </div>
      </div>

      {/* Category Form */}
      <CategoryForm />
    </div>
  );
}



