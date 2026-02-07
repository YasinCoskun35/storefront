import { Metadata } from "next";
import Image from "next/image";
import { catalogApi } from "@/lib/api";
import { formatPrice, getImageUrl } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { ShoppingCart } from "lucide-react";
import { notFound } from "next/navigation";

interface ProductDetailPageProps {
  params: Promise<{
    id: string;
  }>;
}

export async function generateMetadata({
  params,
}: ProductDetailPageProps): Promise<Metadata> {
  const { id } = await params;
  
  try {
    const product = await catalogApi.getProductById(id);

    return {
      title: `${product.name} - Storefront`,
      description:
        product.shortDescription || product.description || product.name,
      openGraph: {
        title: product.name,
        description:
          product.shortDescription || product.description || product.name,
        images: product.primaryImageUrl
          ? [getImageUrl(product.primaryImageUrl)]
          : [],
      },
    };
  } catch (error) {
    return {
      title: "Product Not Found - Storefront",
    };
  }
}

export default async function ProductDetailPage({
  params,
}: ProductDetailPageProps) {
  const { id } = await params;
  
  let product;
  try {
    product = await catalogApi.getProductById(id);
  } catch (error) {
    notFound();
  }

  const primaryImage =
    product.images.find((img) => img.isPrimary) || product.images[0];
  const thumbnails = product.images
    .filter((img) => img.type === "Thumbnail")
    .sort((a, b) => a.displayOrder - b.displayOrder);

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Image Gallery */}
        <div className="space-y-4">
          <div className="relative aspect-square overflow-hidden rounded-lg bg-muted">
            <Image
              src={getImageUrl(primaryImage?.url)}
              alt={product.name}
              fill
              className="object-cover"
              priority
            />
          </div>
          {thumbnails.length > 0 && (
            <div className="grid grid-cols-4 gap-4">
              {thumbnails.map((img) => (
                <div
                  key={img.id}
                  className="relative aspect-square overflow-hidden rounded-lg bg-muted border"
                >
                  <Image
                    src={getImageUrl(img.url)}
                    alt={product.name}
                    fill
                    className="object-cover"
                  />
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Product Info */}
        <div className="space-y-6">
          <div>
            <p className="text-sm text-muted-foreground mb-2">
              {product.brandName || "Hardware"}
            </p>
            <h1 className="text-4xl font-bold mb-2">{product.name}</h1>
            <p className="text-sm text-muted-foreground">SKU: {product.sku}</p>
          </div>

          <div className="flex items-baseline gap-4">
            <span className="text-4xl font-bold">
              {formatPrice(product.price)}
            </span>
            {product.compareAtPrice && product.compareAtPrice > product.price && (
              <span className="text-xl text-muted-foreground line-through">
                {formatPrice(product.compareAtPrice)}
              </span>
            )}
          </div>

          <div
            className={`inline-flex items-center px-4 py-2 rounded text-sm font-medium ${
              product.stockStatus === "InStock"
                ? "bg-green-100 text-green-800"
                : "bg-red-100 text-red-800"
            }`}
          >
            {product.stockStatus === "InStock"
              ? `In Stock (${product.quantity} available)`
              : "Out of Stock"}
          </div>

          <Button size="lg" className="w-full" disabled={product.stockStatus !== "InStock"}>
            <ShoppingCart className="mr-2 h-5 w-5" />
            Contact to Purchase
          </Button>

          {product.shortDescription && (
            <div>
              <h2 className="text-lg font-semibold mb-2">Overview</h2>
              <p className="text-muted-foreground">{product.shortDescription}</p>
            </div>
          )}

          {product.description && (
            <div>
              <h2 className="text-lg font-semibold mb-2">Description</h2>
              <div
                className="prose prose-sm max-w-none"
                dangerouslySetInnerHTML={{ __html: product.description }}
              />
            </div>
          )}

          {/* Specifications */}
          {(product.weight || product.length || product.width || product.height) && (
            <div>
              <h2 className="text-lg font-semibold mb-4">Specifications</h2>
              <table className="w-full text-sm">
                <tbody>
                  {product.weight && (
                    <tr className="border-b">
                      <td className="py-2 font-medium">Weight</td>
                      <td className="py-2 text-muted-foreground">
                        {product.weight} {product.weightUnit || "kg"}
                      </td>
                    </tr>
                  )}
                  {product.length && (
                    <tr className="border-b">
                      <td className="py-2 font-medium">Length</td>
                      <td className="py-2 text-muted-foreground">
                        {product.length} {product.dimensionUnit || "cm"}
                      </td>
                    </tr>
                  )}
                  {product.width && (
                    <tr className="border-b">
                      <td className="py-2 font-medium">Width</td>
                      <td className="py-2 text-muted-foreground">
                        {product.width} {product.dimensionUnit || "cm"}
                      </td>
                    </tr>
                  )}
                  {product.height && (
                    <tr className="border-b">
                      <td className="py-2 font-medium">Height</td>
                      <td className="py-2 text-muted-foreground">
                        {product.height} {product.dimensionUnit || "cm"}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

