import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone", // Enable for optimized Docker builds
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "images.unsplash.com",
        pathname: "/**",
      },
      {
        protocol: "http",
        hostname: "localhost",
        port: "8080",
        pathname: "/uploads/**",
      },
      {
        protocol: "http",
        hostname: "api",
        port: "8080",
        pathname: "/uploads/**",
      },
      {
        protocol: "http",
        hostname: "nginx",
        port: "",
        pathname: "/uploads/**",
      },
    ],
  },
  // Enable experimental features if needed
  experimental: {
    optimizePackageImports: ["lucide-react"],
  },
};

export default nextConfig;

