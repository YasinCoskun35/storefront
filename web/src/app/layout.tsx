import type { Metadata } from "next";
import { Inter, Poppins } from "next/font/google";
import "./globals.css";
import { ConditionalPublicLayout } from "@/components/layout/conditional-public-layout";
import { QueryProvider } from "@/components/providers/query-provider";
import { AuthProvider } from "@/lib/auth-context";
import { Toaster } from "sonner";

const inter = Inter({ 
  subsets: ["latin"],
  variable: "--font-sans",
  display: "swap",
});

const poppins = Poppins({ 
  weight: ["600", "700"],
  subsets: ["latin"],
  variable: "--font-display",
  display: "swap",
});

export const metadata: Metadata = {
  title: "Storefront - Profesyonel Mobilya Çözümleri",
  description: "B2B mobilya kataloğu — ürünleri keşfedin, sipariş oluşturun.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="tr" className={`${inter.variable} ${poppins.variable}`}>
      <body className="font-sans antialiased">
        <AuthProvider>
          <QueryProvider>
            <ConditionalPublicLayout>{children}</ConditionalPublicLayout>
            <Toaster />
          </QueryProvider>
        </AuthProvider>
      </body>
    </html>
  );
}
