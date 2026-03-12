"use client";

import { createPortal } from "react-dom";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  Search,
  ShoppingBag,
  ShoppingCart,
  ChevronDown,
  X,
  Package,
  Menu,
  Heart,
  User,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { useState, useEffect, useRef } from "react";
import { useQuery } from "@tanstack/react-query";
import { settingsApi } from "@/lib/api/settings";
import { catalogApi } from "@/lib/api";
import { partnerOrdersApi } from "@/lib/api/orders";
import { getImageUrl, cn } from "@/lib/utils";
import Image from "next/image";
export function Header() {
  const router = useRouter();
  const [searchQuery, setSearchQuery] = useState("");
  const [isPartner, setIsPartner] = useState(false);
  const [hoveredCategoryId, setHoveredCategoryId] = useState<string | null>(null);
  const [cartOpen, setCartOpen] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [mounted, setMounted] = useState(false);
  const cartRef = useRef<HTMLDivElement>(null);
  const navbarRef = useRef<HTMLDivElement>(null);
  const closeTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    setIsPartner(!!localStorage.getItem("partner_access_token"));
  }, []);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (cartRef.current && !cartRef.current.contains(e.target as Node)) {
        setCartOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  const { data: settings } = useQuery({
    queryKey: ["app-settings"],
    queryFn: settingsApi.getAll,
    staleTime: 5 * 60 * 1000,
  });

  const showBlog =
    settings?.find((s) => s.key === "Features.Blog.Enabled")?.value?.toLowerCase() === "true";

  const { data: categoryTree } = useQuery({
    queryKey: ["categories-tree"],
    queryFn: catalogApi.getCategoriesTree,
    staleTime: 10 * 60 * 1000,
  });

  const navbarCategories = (categoryTree ?? []).filter((c) => c.showInNavbar === true);

  const { data: cart } = useQuery({
    queryKey: ["cart"],
    queryFn: partnerOrdersApi.getCart,
    enabled: isPartner,
    staleTime: 30 * 1000,
  });

  const cartCount = cart?.itemCount ?? 0;

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      router.push(`/products?q=${encodeURIComponent(searchQuery)}`);
      setMobileMenuOpen(false);
    }
  };

  return (
    <header className="sticky top-0 z-[100] w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      {/* Top row: Logo (left) | Search (center) | Cart & Favorites (right) */}
      <div className="container mx-auto flex h-14 items-center gap-4 px-4">
        {/* Logo (left) */}
        <Link
          href="/"
          className="flex items-center gap-2 font-bold text-lg shrink-0"
          onClick={() => setMobileMenuOpen(false)}
        >
          <ShoppingBag className="h-6 w-6 text-primary" />
          <span>Storefront</span>
        </Link>

        {/* Search (center) */}
        <form onSubmit={handleSearch} className="hidden md:flex flex-1 justify-center min-w-0 max-w-md">
          <div className="relative w-full">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              type="search"
              placeholder="Buraya yazınız..."
              className="w-full pl-9 h-9 text-sm bg-muted/50"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>
        </form>

        {/* Cart & Favorites (right) */}
        <div className="flex items-center gap-2 ml-auto shrink-0">
          {!isPartner && (
            <Button size="sm" variant="outline" asChild className="hidden md:flex gap-1.5">
              <Link href="/partner/login">
                <User className="h-4 w-4" />
                Giriş Yap
              </Link>
            </Button>
          )}
          <Button size="sm" variant="outline" asChild className="hidden md:flex gap-1.5">
            <Link href="/partner/login">
              <Heart className="h-4 w-4" />
              Favorilerim
            </Link>
          </Button>
          {isPartner ? (
            <div ref={cartRef} className="relative">
              <Button
                size="sm"
                variant="outline"
                className="relative gap-1.5"
                onClick={() => setCartOpen((v) => !v)}
                aria-label="Sepetim"
              >
                <ShoppingCart className="h-4 w-4" />
                Sepetim
                {cartCount > 0 && (
                  <Badge className="absolute -right-1 -top-1 h-4 w-4 rounded-full p-0 text-[10px] flex items-center justify-center bg-primary text-primary-foreground">
                    {cartCount > 9 ? "9+" : cartCount}
                  </Badge>
                )}
              </Button>

              {cartOpen && (
                <div className="absolute right-0 top-full mt-1 w-80 rounded-xl border bg-popover shadow-lg">
                  <div className="flex items-center justify-between border-b p-4">
                    <p className="font-semibold text-sm">Sepetim</p>
                    <div className="flex items-center gap-2">
                      {cartCount > 0 && (
                        <Badge variant="secondary" className="text-xs">
                          {cartCount} ürün
                        </Badge>
                      )}
                      <button
                        onClick={() => setCartOpen(false)}
                        className="text-muted-foreground hover:text-foreground"
                      >
                        <X className="h-4 w-4" />
                      </button>
                    </div>
                  </div>

                  {cart?.items && cart.items.length > 0 ? (
                    <>
                      <ul className="max-h-72 overflow-y-auto divide-y">
                        {cart.items.slice(0, 5).map((item) => (
                          <li key={item.id} className="flex gap-3 p-3">
                            <div className="relative h-12 w-12 shrink-0 overflow-hidden rounded-md bg-muted">
                              {item.productImageUrl ? (
                                <Image
                                  src={getImageUrl(item.productImageUrl)}
                                  alt={item.productName}
                                  fill
                                  className="object-cover"
                                  unoptimized
                                />
                              ) : (
                                <Package className="h-full w-full p-2 text-muted-foreground" />
                              )}
                            </div>
                            <div className="flex-1 min-w-0">
                              <p className="text-xs font-medium line-clamp-2 leading-snug">
                                {item.productName}
                              </p>
                              <p className="text-xs text-muted-foreground mt-0.5">
                                Adet: {item.quantity}
                              </p>
                            </div>
                          </li>
                        ))}
                        {cart.items.length > 5 && (
                          <li className="p-3 text-center text-xs text-muted-foreground">
                            +{cart.items.length - 5} ürün daha
                          </li>
                        )}
                      </ul>
                      <div className="p-3 border-t space-y-2">
                        <Button className="w-full h-9 text-sm" asChild onClick={() => setCartOpen(false)}>
                          <Link href="/partner/cart">Sepete Git</Link>
                        </Button>
                        <Button
                          variant="outline"
                          className="w-full h-9 text-sm"
                          asChild
                          onClick={() => setCartOpen(false)}
                        >
                          <Link href="/partner/checkout">Sipariş Oluştur</Link>
                        </Button>
                      </div>
                    </>
                  ) : (
                    <div className="p-8 text-center">
                      <ShoppingCart className="h-8 w-8 text-muted-foreground mx-auto mb-2" />
                      <p className="text-sm text-muted-foreground">Sepetiniz boş</p>
                      <Button
                        size="sm"
                        variant="outline"
                        className="mt-3"
                        asChild
                        onClick={() => setCartOpen(false)}
                      >
                        <Link href="/products">Ürünlere Git</Link>
                      </Button>
                    </div>
                  )}
                </div>
              )}
            </div>
          ) : (
            <Button size="sm" variant="outline" asChild className="gap-1.5">
              <Link href="/partner/login">
                <ShoppingCart className="h-4 w-4" />
                Sepetim
              </Link>
            </Button>
          )}

          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            onClick={() => setMobileMenuOpen((v) => !v)}
            aria-label="Menü"
          >
            {mobileMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </Button>
        </div>
      </div>

      {/* Bottom row: Main categories (navbar) */}
      {navbarCategories.length > 0 && (
        <div
          ref={navbarRef}
          className="border-t bg-muted/30"
          onMouseLeave={() => {
            closeTimeoutRef.current = setTimeout(() => setHoveredCategoryId(null), 50);
          }}
          onMouseEnter={() => {
            if (closeTimeoutRef.current) {
              clearTimeout(closeTimeoutRef.current);
              closeTimeoutRef.current = null;
            }
          }}
        >
          <nav className="container mx-auto px-4">
            <div className="flex items-center justify-center gap-1 overflow-x-auto py-2">
              {navbarCategories.map((cat) => {
                const triggerClass = cn(
                  "flex items-center gap-1 px-4 py-2 rounded-md text-sm font-medium transition-colors",
                  hoveredCategoryId === cat.id
                    ? "bg-primary/10 text-primary"
                    : "hover:bg-muted hover:text-foreground"
                );
                return (
                <div
                  key={cat.id}
                  className="relative shrink-0"
                  onMouseEnter={() => setHoveredCategoryId(cat.id)}
                >
                  <span
                    className={cn(triggerClass, "cursor-pointer")}
                    role="button"
                    tabIndex={0}
                    onClick={(e) => {
                      e.preventDefault();
                      setHoveredCategoryId((prev) => (prev === cat.id ? null : cat.id));
                    }}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        setHoveredCategoryId((prev) => (prev === cat.id ? null : cat.id));
                      }
                    }}
                  >
                    {cat.name}
                    <ChevronDown className="h-3.5 w-3.5" />
                  </span>

                </div>
                );
              })}
              <Link
                href="/products"
                className="flex items-center px-4 py-2 rounded-md text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-muted shrink-0"
              >
                Tüm Ürünler
              </Link>
              {showBlog && (
                <Link
                  href="/blog"
                  className="flex items-center px-4 py-2 rounded-md text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-muted shrink-0"
                >
                  Blog
                </Link>
              )}
            </div>
          </nav>

          {/* Mega menu portal - renders outside overflow to prevent clipping */}
          {mounted && hoveredCategoryId && typeof document !== "undefined" && (() => {
            const cat = navbarCategories.find((c) => c.id === hoveredCategoryId);
            if (!cat) return null;
            const hasChildren = (cat.children?.length ?? 0) > 0;
            const menuEl = (
              <div
                className="fixed left-1/2 -translate-x-1/2 w-[min(90vw,800px)] min-h-[120px] rounded-b-xl border border-t-0 bg-popover p-6 shadow-xl z-[9999]"
                style={{
                  top: navbarRef.current
                    ? navbarRef.current.getBoundingClientRect().bottom
                    : 112,
                }}
                onMouseEnter={() => {
                  if (closeTimeoutRef.current) {
                    clearTimeout(closeTimeoutRef.current);
                    closeTimeoutRef.current = null;
                  }
                }}
                onMouseLeave={() => {
                  closeTimeoutRef.current = setTimeout(() => setHoveredCategoryId(null), 50);
                }}
              >
                <div className="mb-4 flex items-center justify-between">
                  <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
                    {hasChildren ? "Alt Kategoriler" : cat.name}
                  </p>
                  <Link
                    href={`/products?categoryId=${cat.id}`}
                    className="text-xs text-primary hover:underline"
                    onClick={() => setHoveredCategoryId(null)}
                  >
                    Tümünü Gör →
                  </Link>
                </div>
                {hasChildren ? (
                  <div className="grid grid-cols-4 gap-x-8 gap-y-5">
                    {cat.children!.slice(0, 12).map((child) => (
                      <Link
                        key={child.id}
                        href={`/products?categoryId=${child.id}`}
                        className="block font-medium text-foreground hover:text-primary transition-colors text-sm"
                        onClick={() => setHoveredCategoryId(null)}
                      >
                        {child.name}
                      </Link>
                    ))}
                  </div>
                ) : (
                  <Link
                    href={`/products?categoryId=${cat.id}`}
                    className="block font-medium text-foreground hover:text-primary transition-colors text-sm"
                    onClick={() => setHoveredCategoryId(null)}
                  >
                    {cat.name} ürünlerini görüntüle
                  </Link>
                )}
              </div>
            );
            return createPortal(menuEl, document.body);
          })()}
        </div>
      )}

      {/* Mobile: show categories + blog if no navbar categories */}
      {navbarCategories.length === 0 && (
        <div className="border-t md:hidden">
          <div className="container mx-auto px-4 py-2 flex gap-2 overflow-x-auto">
            <Link href="/products" className="shrink-0 text-sm font-medium">
              Tüm Ürünler
            </Link>
            {showBlog && (
              <Link href="/blog" className="shrink-0 text-sm font-medium text-muted-foreground">
                Blog
              </Link>
            )}
          </div>
        </div>
      )}

      {/* Mobile Menu */}
      {mobileMenuOpen && (
        <div className="border-t md:hidden">
          <div className="container mx-auto px-4 py-4 space-y-3">
            <form onSubmit={handleSearch} className="flex gap-2">
              <div className="relative flex-1">
                <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  type="search"
                  placeholder="Ürün ara..."
                  className="pl-8 h-9"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                />
              </div>
              <Button type="submit" size="sm">
                Ara
              </Button>
            </form>

            <nav className="flex flex-col gap-1 text-sm font-medium">
              <Link
                href="/products"
                className="px-3 py-2 rounded-md hover:bg-muted"
                onClick={() => setMobileMenuOpen(false)}
              >
                Tüm Ürünler
              </Link>
              {navbarCategories.map((cat) => (
                <Link
                  key={cat.id}
                  href={`/products?categoryId=${cat.id}`}
                  className="px-3 py-2 rounded-md hover:bg-muted"
                  onClick={() => setMobileMenuOpen(false)}
                >
                  {cat.name}
                </Link>
              ))}
              {showBlog && (
                <Link
                  href="/blog"
                  className="px-3 py-2 rounded-md hover:bg-muted"
                  onClick={() => setMobileMenuOpen(false)}
                >
                  Blog
                </Link>
              )}
              {isPartner && (
                <Link
                  href="/partner/cart"
                  className="px-3 py-2 rounded-md hover:bg-muted flex items-center justify-between"
                  onClick={() => setMobileMenuOpen(false)}
                >
                  <span>Sepetim</span>
                  {cartCount > 0 && <Badge variant="secondary">{cartCount}</Badge>}
                </Link>
              )}
              {!isPartner && (
                <Link
                  href="/partner/login"
                  className="px-3 py-2 rounded-md hover:bg-muted text-primary font-semibold"
                  onClick={() => setMobileMenuOpen(false)}
                >
                  Giriş Yap
                </Link>
              )}
            </nav>
          </div>
        </div>
      )}
    </header>
  );
}
