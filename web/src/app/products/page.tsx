import { catalogApi } from "@/lib/api";
import { ProductCard } from "@/components/products/product-card";
import { ProductFilters } from "@/components/products/product-filters";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import { ChevronLeft, ChevronRight } from "lucide-react";

interface ProductsPageProps {
  searchParams: Promise<{
    q?: string;
    categoryId?: string;
    minPrice?: string;
    maxPrice?: string;
    pageNumber?: string;
  }>;
}

export default async function ProductsPage({ searchParams }: ProductsPageProps) {
  const params = await searchParams;
  const pageNumber = parseInt(params.pageNumber || "1");
  
  // Fetch products and categories in parallel
  const [productsResult, categories] = await Promise.all([
    catalogApi.searchProducts({
      searchTerm: params.q,
      categoryId: params.categoryId,
      minPrice: params.minPrice ? parseFloat(params.minPrice) : undefined,
      maxPrice: params.maxPrice ? parseFloat(params.maxPrice) : undefined,
      pageNumber,
      pageSize: 12,
    }),
    catalogApi.getCategories(),
  ]);

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2">Products</h1>
        {params.q && (
          <p className="text-muted-foreground">
            Showing results for "{params.q}"
          </p>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
        {/* Filters Sidebar */}
        <aside className="lg:col-span-1">
          <div className="sticky top-20">
            <ProductFilters
              categories={categories.map((c) => ({ id: c.id, name: c.name }))}
            />
          </div>
        </aside>

        {/* Products Grid */}
        <div className="lg:col-span-3">
          {productsResult.items.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-lg text-muted-foreground">
                No products found. Try adjusting your filters.
              </p>
            </div>
          ) : (
            <>
              <div className="mb-4 flex items-center justify-between">
                <p className="text-sm text-muted-foreground">
                  Showing {(pageNumber - 1) * 12 + 1}-
                  {Math.min(pageNumber * 12, productsResult.totalCount)} of{" "}
                  {productsResult.totalCount} products
                </p>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
                {productsResult.items.map((product) => (
                  <ProductCard key={product.id} product={product} />
                ))}
              </div>

              {/* Pagination */}
              {productsResult.totalPages > 1 && (
                <div className="mt-8 flex items-center justify-center gap-2">
                  {productsResult.hasPreviousPage && (
                    <Link
                      href={`/products?${new URLSearchParams({
                        ...params,
                        pageNumber: (pageNumber - 1).toString(),
                      })}`}
                    >
                      <Button variant="outline" size="icon">
                        <ChevronLeft className="h-4 w-4" />
                      </Button>
                    </Link>
                  )}

                  <span className="text-sm">
                    Page {pageNumber} of {productsResult.totalPages}
                  </span>

                  {productsResult.hasNextPage && (
                    <Link
                      href={`/products?${new URLSearchParams({
                        ...params,
                        pageNumber: (pageNumber + 1).toString(),
                      })}`}
                    >
                      <Button variant="outline" size="icon">
                        <ChevronRight className="h-4 w-4" />
                      </Button>
                    </Link>
                  )}
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}

