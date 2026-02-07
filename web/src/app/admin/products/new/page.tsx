"use client";

import { ProductForm } from "@/components/admin/product-form";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";

export default function NewProductPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/admin/products">
          <button className="flex h-10 w-10 items-center justify-center rounded-lg border bg-background hover:bg-muted">
            <ArrowLeft className="h-5 w-5" />
          </button>
        </Link>
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary">Add New Product</h1>
          <p className="text-muted-foreground">Create a new product in your catalog</p>
        </div>
      </div>

      {/* Product Form */}
      <ProductForm />
    </div>
  );
}



