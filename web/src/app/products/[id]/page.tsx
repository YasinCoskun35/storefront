import { Metadata } from "next";
import { catalogApi } from "@/lib/api";
import { formatPrice, getImageUrl } from "@/lib/utils";
import { AddToCartSection } from "@/components/products/add-to-cart-section";
import { ProductGallery } from "@/components/products/product-gallery";
import { Badge } from "@/components/ui/badge";
import { notFound } from "next/navigation";
import Link from "next/link";
import { ChevronRight } from "lucide-react";

interface ProductDetailPageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({ params }: ProductDetailPageProps): Promise<Metadata> {
  const { id } = await params;
  try {
    const product = await catalogApi.getProductById(id);
    return {
      title: `${product.name} - Storefront`,
      description: product.shortDescription || product.description || product.name,
      openGraph: {
        title: product.name,
        description: product.shortDescription || product.description || product.name,
        images: product.primaryImageUrl ? [getImageUrl(product.primaryImageUrl)] : [],
      },
    };
  } catch {
    return { title: "Ürün Bulunamadı - Storefront" };
  }
}

const stockConfig: Record<number, { label: string; className: string }> = {
  1: { label: "Stokta", className: "bg-success text-success-foreground" },
  2: { label: "Son Ürünler", className: "bg-warning text-warning-foreground" },
  0: { label: "Tükendi", className: "bg-destructive text-destructive-foreground" },
};

export default async function ProductDetailPage({ params }: ProductDetailPageProps) {
  const { id } = await params;

  let product;
  try {
    product = await catalogApi.getProductById(id);
  } catch {
    notFound();
  }

  const primaryImage = product.images.find((img) => img.isPrimary) || product.images[0];
  const stock = stockConfig[product.stockStatus] ?? stockConfig[1];

  const specs = [
    product.weight != null && { label: "Ağırlık", value: `${product.weight} ${product.weightUnit || "kg"}` },
    product.length != null && { label: "Uzunluk", value: `${product.length} ${product.dimensionUnit || "cm"}` },
    product.width != null && { label: "Genişlik", value: `${product.width} ${product.dimensionUnit || "cm"}` },
    product.height != null && { label: "Yükseklik", value: `${product.height} ${product.dimensionUnit || "cm"}` },
  ].filter(Boolean) as { label: string; value: string }[];

  return (
    <div className="container mx-auto px-4 py-8 max-w-6xl">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-1 text-sm text-muted-foreground mb-6 flex-wrap">
        <Link href="/" className="hover:text-foreground transition-colors">Ana Sayfa</Link>
        <ChevronRight className="h-3.5 w-3.5 shrink-0" />
        <Link href="/products" className="hover:text-foreground transition-colors">Ürünler</Link>
        {product.categoryName && (
          <>
            <ChevronRight className="h-3.5 w-3.5 shrink-0" />
            <Link
              href={`/products?categoryId=${product.categoryId}`}
              className="hover:text-foreground transition-colors"
            >
              {product.categoryName}
            </Link>
          </>
        )}
        <ChevronRight className="h-3.5 w-3.5 shrink-0" />
        <span className="text-foreground font-medium line-clamp-1">{product.name}</span>
      </nav>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-10 xl:gap-16">
        {/* Gallery — client component for thumbnail selection */}
        <ProductGallery
          primaryImage={primaryImage}
          images={product.images}
          productName={product.name}
        />

        {/* Product Info */}
        <div className="space-y-5">
          {/* Brand + Name */}
          <div>
            {product.brandName && (
              <p className="text-sm font-medium text-muted-foreground mb-1">{product.brandName}</p>
            )}
            <h1 className="text-2xl font-bold leading-tight md:text-3xl">{product.name}</h1>
            <p className="text-xs text-muted-foreground mt-1">SKU: {product.sku}</p>
          </div>

          {/* Price */}
          {product.price != null && (
            <div className="flex items-baseline gap-3">
              <span className="text-3xl font-bold text-primary">
                {formatPrice(product.price)}
              </span>
              {product.compareAtPrice != null && product.compareAtPrice > product.price && (
                <span className="text-lg text-muted-foreground line-through">
                  {formatPrice(product.compareAtPrice)}
                </span>
              )}
            </div>
          )}

          {/* Stock */}
          <div>
            <Badge className={stock.className}>
              {stock.label}
              {product.stockStatus === 1 && product.quantity > 0
                ? ` (${product.quantity} adet)`
                : ""}
            </Badge>
          </div>

          {/* Short description */}
          {product.shortDescription && (
            <p className="text-muted-foreground text-sm leading-relaxed">
              {product.shortDescription}
            </p>
          )}

          {/* Add to Cart — client component handles partner auth + variant selection */}
          <AddToCartSection
            productId={product.id}
            productName={product.name}
            productSKU={product.sku}
            productImageUrl={product.primaryImageUrl}
          />

          {/* Full description */}
          {product.description && (
            <div className="pt-4 border-t">
              <h2 className="text-sm font-semibold mb-2">Ürün Açıklaması</h2>
              <div
                className="prose prose-sm max-w-none text-muted-foreground"
                dangerouslySetInnerHTML={{ __html: product.description }}
              />
            </div>
          )}

          {/* Specifications */}
          {specs.length > 0 && (
            <div className="pt-4 border-t">
              <h2 className="text-sm font-semibold mb-3">Teknik Özellikler</h2>
              <dl className="divide-y rounded-lg border text-sm overflow-hidden">
                {specs.map(({ label, value }) => (
                  <div key={label} className="flex px-4 py-2.5">
                    <dt className="w-32 text-muted-foreground shrink-0">{label}</dt>
                    <dd className="font-medium">{value}</dd>
                  </div>
                ))}
              </dl>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
