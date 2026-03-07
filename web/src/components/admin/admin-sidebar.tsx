"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
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
    title: "Dashboard",
    href: "/admin/dashboard",
    icon: LayoutDashboard,
  },
  {
    title: "Products",
    href: "/admin/products",
    icon: Package,
  },
  {
    title: "Categories",
    href: "/admin/categories",
    icon: FolderTree,
  },
  {
    title: "Partners",
    href: "/admin/partners",
    icon: Handshake,
  },
  {
    title: "Orders",
    href: "/admin/orders",
    icon: ShoppingBag,
  },
  {
    title: "Variant Groups",
    href: "/admin/variant-groups",
    icon: Palette,
  },
  {
    title: "Blog",
    href: "/admin/blog",
    icon: FileText,
    featureKey: "Features.Blog.Enabled",
  },
  {
    title: "Users",
    href: "/admin/users",
    icon: Users,
  },
  {
    title: "Settings",
    href: "/admin/settings",
    icon: Settings,
  },
];

export function AdminSidebar() {
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
          <span className="text-lg font-bold">Admin</span>
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
                <span>{item.title}</span>
              </Link>
            );
          })
        )}
      </nav>
    </div>
  );
}

