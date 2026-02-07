"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { catalogApi, Product } from "@/lib/api";
import { DataTable } from "@/components/admin/data-table";
import { Button } from "@/components/ui/button";
import { formatPrice, getImageUrl } from "@/lib/utils";
import { Plus, Pencil, Trash2 } from "lucide-react";
import Image from "next/image";
import Link from "next/link";

export default function AdminProductsPage() {
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ["admin-products", page],
    queryFn: () => catalogApi.searchProducts({ pageNumber: page, pageSize: 10 }),
  });

  const columns = [
    {
      header: "Image",
      accessor: (row: Product) => (
        <div className="relative h-12 w-12 rounded overflow-hidden">
          <Image
            src={getImageUrl(row.primaryImageUrl)}
            alt={row.name}
            fill
            className="object-cover"
          />
        </div>
      ),
    },
    {
      header: "Name",
      accessor: "name" as keyof Product,
      cell: (value: string) => <span className="font-medium">{value}</span>,
    },
    {
      header: "SKU",
      accessor: "sku" as keyof Product,
    },
    {
      header: "Price",
      accessor: "price" as keyof Product,
      cell: (value: number) => formatPrice(value),
    },
    {
      header: "Stock",
      accessor: "stockStatus" as keyof Product,
      cell: (value: string, row: Product) => (
        <span
          className={`px-2 py-1 rounded text-xs ${
            value === "InStock"
              ? "bg-green-100 text-green-800"
              : "bg-red-100 text-red-800"
          }`}
        >
          {value} ({row.quantity})
        </span>
      ),
    },
    {
      header: "Actions",
      accessor: (row: Product) => (
        <div className="flex items-center gap-2">
          <Link href={`/admin/products/${row.id}`}>
            <Button variant="ghost" size="sm">
              <Pencil className="h-4 w-4" />
            </Button>
          </Link>
          <Button variant="ghost" size="sm">
            <Trash2 className="h-4 w-4 text-destructive" />
          </Button>
        </div>
      ),
    },
  ];

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Products</h1>
          <p className="text-muted-foreground">
            Manage your product catalog
          </p>
        </div>
        <Link href="/admin/products/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Add Product
          </Button>
        </Link>
      </div>

      <DataTable
        columns={columns}
        data={data?.items || []}
        currentPage={page}
        totalPages={data?.totalPages || 1}
        onPageChange={setPage}
      />
    </div>
  );
}

