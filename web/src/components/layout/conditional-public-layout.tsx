"use client";

import { usePathname } from "next/navigation";
import { useState, useEffect } from "react";
import { Header } from "@/components/layout/header";
import { Footer } from "@/components/layout/footer";
import { settingsApi } from "@/lib/api/settings";

export function ConditionalPublicLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const [storefrontEnabled, setStorefrontEnabled] = useState<boolean | null>(null);

  const isAdminRoute = pathname.startsWith("/admin");
  const isPartnerRoute = pathname.startsWith("/partner");
  const isLoginRoute = pathname === "/login";
  const isAppRoute = isAdminRoute || isPartnerRoute || isLoginRoute;

  useEffect(() => {
    if (isAppRoute) return;

    const load = async () => {
      try {
        const enabled = await settingsApi.isFeatureEnabled("Features.PublicStorefront.Enabled");
        setStorefrontEnabled(enabled);
      } catch {
        setStorefrontEnabled(false);
      }
    };
    load();
  }, [isAppRoute]);

  if (isAppRoute) {
    return <>{children}</>;
  }

  if (storefrontEnabled === null) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
      </div>
    );
  }

  if (!storefrontEnabled) {
    return <StorefrontDisabledRedirect />;
  }

  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1">{children}</main>
      <Footer />
    </div>
  );
}

function StorefrontDisabledRedirect() {
  const [countdown, setCountdown] = useState(3);

  useEffect(() => {
    const timer = setInterval(() => {
      setCountdown((c) => {
        if (c <= 1) {
          window.location.href = "/partner/login";
          return 0;
        }
        return c - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, []);

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-background px-4">
      <div className="max-w-md text-center space-y-6">
        <div className="mx-auto w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center">
          <svg className="w-8 h-8 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
          </svg>
        </div>
        <div>
          <h1 className="text-2xl font-bold">Partner Access Only</h1>
          <p className="text-muted-foreground mt-2">
            This platform is available to registered business partners.
          </p>
        </div>
        <p className="text-sm text-muted-foreground">
          Redirecting to partner login in {countdown}...
        </p>
        <div className="flex gap-3 justify-center">
          <a
            href="/partner/login"
            className="inline-flex items-center justify-center rounded-md bg-primary px-6 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 transition-colors"
          >
            Partner Login
          </a>
          <a
            href="/login"
            className="inline-flex items-center justify-center rounded-md border border-input bg-background px-6 py-2 text-sm font-medium hover:bg-accent hover:text-accent-foreground transition-colors"
          >
            Admin Login
          </a>
        </div>
      </div>
    </div>
  );
}
