"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useTranslations } from "next-intl";
import { cn } from "@/lib/utils";
import {
  LayoutDashboard,
  Package,
  FolderTree,
  FileText,
  Users,
  Settings,
  ShoppingBag,
  Handshake,
  Palette,
} from "lucide-react";
import { settingsApi } from "@/lib/api/settings";

const allMenuItems = [
  {
    key: "dashboard",
    href: "/admin/dashboard",
    icon: LayoutDashboard,
  },
  {
    key: "products",
    href: "/admin/products",
    icon: Package,
  },
  {
    key: "categories",
    href: "/admin/categories",
    icon: FolderTree,
  },
  {
    key: "partners",
    href: "/admin/partners",
    icon: Handshake,
  },
  {
    key: "orders",
    href: "/admin/orders",
    icon: ShoppingBag,
  },
  {
    key: "variantGroups",
    href: "/admin/variant-groups",
    icon: Palette,
  },
  {
    key: "blog",
    href: "/admin/blog",
    icon: FileText,
    featureKey: "Features.Blog.Enabled",
  },
  {
    key: "users",
    href: "/admin/users",
    icon: Users,
  },
  {
    key: "settings",
    href: "/admin/settings",
    icon: Settings,
  },
];

export function AdminSidebar() {
  const t = useTranslations("nav");
  const pathname = usePathname();
  const [menuItems, setMenuItems] = useState(allMenuItems);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadSettings = async () => {
      try {
        const settings = await settingsApi.getAll();
        
        // Filter menu items based on feature flags
        const filteredItems = allMenuItems.filter((item) => {
          if (!item.featureKey) return true; // No feature key = always show
          
          const setting = settings.find((s) => s.key === item.featureKey);
          return setting?.value?.toLowerCase() === "true";
        });
        
        setMenuItems(filteredItems);
      } catch (error) {
        console.error("Failed to load settings:", error);
        // On error, show all items
        setMenuItems(allMenuItems);
      } finally {
        setIsLoading(false);
      }
    };

    loadSettings();
  }, []);

  return (
    <div className="flex h-full w-64 flex-col border-r bg-muted/40">
      <div className="flex h-16 items-center border-b px-6">
        <Link href="/admin/dashboard" className="flex items-center space-x-2">
          <ShoppingBag className="h-6 w-6" />
          <span className="text-lg font-bold">{t("admin")}</span>
        </Link>
      </div>
      <nav className="flex-1 space-y-1 p-4">
        {isLoading ? (
          <div className="flex items-center justify-center py-8">
            <div className="h-5 w-5 animate-spin rounded-full border-2 border-primary border-t-transparent" />
          </div>
        ) : (
          menuItems.map((item) => {
            const Icon = item.icon;
            const isActive = pathname === item.href;

            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  "flex items-center space-x-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "hover:bg-accent hover:text-accent-foreground"
                )}
              >
                <Icon className="h-5 w-5" />
                <span>{t(item.key as any)}</span>
              </Link>
            );
          })
        )}
      </nav>
    </div>
  );
}

