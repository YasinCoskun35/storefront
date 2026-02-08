"use client";

import { ORDER_STATUS_COLORS, ORDER_STATUS_LABELS, OrderStatus } from "@/lib/api/orders";

interface OrderStatusBadgeProps {
  status: string | number;
  className?: string;
}

export function OrderStatusBadge({ status, className = "" }: OrderStatusBadgeProps) {
  // Convert string to enum if needed
  const statusValue = typeof status === "string" 
    ? OrderStatus[status as keyof typeof OrderStatus] 
    : status;

  const label = ORDER_STATUS_LABELS[statusValue as OrderStatus] || status;
  const colorClass = ORDER_STATUS_COLORS[statusValue as OrderStatus] || "bg-gray-100 text-gray-800";

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${colorClass} ${className}`}
    >
      {label}
    </span>
  );
}
