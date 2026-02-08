"use client";

import { CartItem } from "@/lib/api/orders";
import { Minus, Plus, Trash2 } from "lucide-react";
import Image from "next/image";
import { Button } from "@/components/ui/button";

interface CartItemCardProps {
  item: CartItem;
  onUpdateQuantity?: (itemId: string, quantity: number) => void;
  onRemove?: (itemId: string) => void;
  readOnly?: boolean;
}

export function CartItemCard({
  item,
  onUpdateQuantity,
  onRemove,
  readOnly = false,
}: CartItemCardProps) {
  return (
    <div className="flex gap-4 p-4 bg-white border rounded-lg">
      {item.productImageUrl && (
        <div className="relative w-24 h-24 flex-shrink-0">
          <Image
            src={item.productImageUrl}
            alt={item.productName}
            fill
            className="object-cover rounded"
          />
        </div>
      )}
      
      <div className="flex-1 min-w-0">
        <div className="flex items-start justify-between">
          <div>
            <h3 className="text-sm font-medium text-gray-900">
              {item.productName}
            </h3>
            <p className="text-sm text-gray-500">SKU: {item.productSKU}</p>
          </div>
          
          {!readOnly && onRemove && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onRemove(item.id)}
              className="text-red-600 hover:text-red-700 hover:bg-red-50"
            >
              <Trash2 className="w-4 h-4" />
            </Button>
          )}
        </div>

        {item.colorOptionName && (
          <div className="mt-2 flex items-center gap-2">
            <span className="text-xs text-gray-500">Color:</span>
            <div className="flex items-center gap-2">
              {item.colorOptionCode && (
                <span className="text-xs font-medium text-gray-700">
                  {item.colorOptionCode}
                </span>
              )}
              <span className="text-xs text-gray-700">{item.colorOptionName}</span>
            </div>
          </div>
        )}

        {item.customizationNotes && (
          <div className="mt-2">
            <span className="text-xs text-gray-500">Notes:</span>
            <p className="text-xs text-gray-700 mt-1">{item.customizationNotes}</p>
          </div>
        )}

        <div className="mt-3 flex items-center justify-between">
          {!readOnly && onUpdateQuantity ? (
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => onUpdateQuantity(item.id, item.quantity - 1)}
                disabled={item.quantity <= 1}
              >
                <Minus className="w-3 h-3" />
              </Button>
              <span className="text-sm font-medium w-12 text-center">
                {item.quantity}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => onUpdateQuantity(item.id, item.quantity + 1)}
              >
                <Plus className="w-3 h-3" />
              </Button>
            </div>
          ) : (
            <span className="text-sm text-gray-600">Quantity: {item.quantity}</span>
          )}
        </div>
      </div>
    </div>
  );
}
