"use client";

import { useQuery } from "@tanstack/react-query";
import { ProductForm } from "@/components/admin/product-form";
import { ProductVariantGroups } from "@/components/admin/product-variant-groups";
import { catalogApi } from "@/lib/api";
import { ArrowLeft, Loader2 } from "lucide-react";
import Link from "next/link";
import { use } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface EditProductPageProps {
  params: Promise<{ id: string }>;
}

export default function EditProductPage({ params }: EditProductPageProps) {
  const { id } = use(params);

  const { data: product, isLoading, error } = useQuery({
    queryKey: ["product", id],
    queryFn: () => catalogApi.getProductById(id),
  });

  if (isLoading) {
    return (
      <div className="flex h-96 items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Link href="/admin/products">
            <button className="flex h-10 w-10 items-center justify-center rounded-lg border bg-background hover:bg-muted">
              <ArrowLeft className="h-5 w-5" />
            </button>
          </Link>
          <div>
            <h1 className="font-display text-3xl font-bold text-secondary">Product Not Found</h1>
            <p className="text-muted-foreground">The product you&apos;re looking for doesn&apos;t exist.</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/admin/products">
          <button className="flex h-10 w-10 items-center justify-center rounded-lg border bg-background hover:bg-muted">
            <ArrowLeft className="h-5 w-5" />
          </button>
        </Link>
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary">Edit Product</h1>
          <p className="text-muted-foreground">Update product information</p>
        </div>
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        <div className="xl:col-span-2">
          <ProductForm productId={id} initialData={product} />
        </div>

        <div>
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Variant Groups</CardTitle>
            </CardHeader>
            <CardContent>
              <ProductVariantGroups productId={id} />
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
