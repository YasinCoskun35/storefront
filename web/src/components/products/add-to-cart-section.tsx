"use client";

import { useState, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { publicVariantsApi, SelectedVariantItem, ProductVariantGroup, VariantOption } from "@/lib/api/variants";
import { partnerOrdersApi } from "@/lib/api/orders";
import { Button } from "@/components/ui/button";
import { ShoppingCart, Check, ChevronDown, ChevronUp, Loader2 } from "lucide-react";
import { toast } from "sonner";
import Link from "next/link";
import Image from "next/image";
import { getImageUrl } from "@/lib/utils";

interface AddToCartSectionProps {
  productId: string;
  productName: string;
  productSKU: string;
  productImageUrl?: string;
}

export function AddToCartSection({
  productId,
  productName,
  productSKU,
  productImageUrl,
}: AddToCartSectionProps) {
  const queryClient = useQueryClient();
  const [isPartner, setIsPartner] = useState(false);
  const [quantity, setQuantity] = useState(1);
  const [selectedVariants, setSelectedVariants] = useState<Record<string, SelectedVariantItem>>({});
  const [expandedGroup, setExpandedGroup] = useState<string | null>(null);
  const [notes, setNotes] = useState("");

  useEffect(() => {
    const token = localStorage.getItem("partner_access_token");
    if (token) {
      localStorage.setItem("accessToken", token);
      setIsPartner(true);
    }
  }, []);

  const { data: variantGroups, isLoading: loadingVariants } = useQuery({
    queryKey: ["product-variant-groups-public", productId],
    queryFn: () => publicVariantsApi.getProductVariantGroups(productId),
    staleTime: 60_000,
  });

  const addToCartMutation = useMutation({
    mutationFn: async () => {
      const required = variantGroups?.filter((pvg) => pvg.isRequired) ?? [];
      for (const pvg of required) {
        if (!selectedVariants[pvg.variantGroupId]) {
          throw new Error(`Please select a ${pvg.variantGroup.name}`);
        }
      }

      const selectedList = Object.values(selectedVariants);
      const variantsJson = selectedList.length > 0 ? JSON.stringify(selectedList) : undefined;

      return partnerOrdersApi.addToCart({
        productId,
        productName,
        productSKU,
        productImageUrl,
        quantity,
        selectedVariants: variantsJson,
        customizationNotes: notes || undefined,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["partner-cart"] });
      toast.success("Added to cart", {
        action: { label: "View Cart", onClick: () => (window.location.href = "/partner/cart") },
      });
    },
    onError: (err: any) => {
      toast.error(err.message || err.response?.data?.message || "Failed to add to cart");
    },
  });

  if (!isPartner) {
    return (
      <div className="border rounded-lg p-4 bg-muted/30 text-center space-y-2">
        <p className="text-sm font-medium">Partner login required to add items to cart</p>
        <p className="text-xs text-muted-foreground">
          Contact us or{" "}
          <Link href="/partner/login" className="text-primary hover:underline">
            sign in as a partner
          </Link>{" "}
          to place an order request.
        </p>
      </div>
    );
  }

  if (loadingVariants) {
    return (
      <div className="flex items-center gap-2 text-muted-foreground text-sm py-4">
        <Loader2 className="h-4 w-4 animate-spin" />
        Loading options...
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {variantGroups && variantGroups.length > 0 && (
        <div className="space-y-3">
          {variantGroups.map((pvg) => (
            <VariantGroupSelector
              key={pvg.id}
              pvg={pvg}
              selected={selectedVariants[pvg.variantGroupId]}
              expanded={expandedGroup === pvg.variantGroupId}
              onToggleExpand={() =>
                setExpandedGroup(expandedGroup === pvg.variantGroupId ? null : pvg.variantGroupId)
              }
              onSelect={(item) => {
                setSelectedVariants((prev) => ({ ...prev, [pvg.variantGroupId]: item }));
                setExpandedGroup(null);
              }}
            />
          ))}
        </div>
      )}

      {/* Quantity */}
      <div className="flex items-center gap-3">
        <span className="text-sm font-medium w-20">Quantity</span>
        <div className="flex items-center border rounded-lg">
          <button
            type="button"
            className="px-3 py-2 hover:bg-muted transition-colors"
            onClick={() => setQuantity((q) => Math.max(1, q - 1))}
          >
            −
          </button>
          <span className="px-4 py-2 min-w-[3rem] text-center font-medium">{quantity}</span>
          <button
            type="button"
            className="px-3 py-2 hover:bg-muted transition-colors"
            onClick={() => setQuantity((q) => q + 1)}
          >
            +
          </button>
        </div>
      </div>

      {/* Notes */}
      <div>
        <label className="text-sm font-medium block mb-1">
          Customization Notes <span className="text-muted-foreground font-normal">(optional)</span>
        </label>
        <textarea
          className="w-full text-sm border rounded-lg px-3 py-2 resize-none focus:outline-none focus:ring-2 focus:ring-ring"
          rows={2}
          placeholder="Specific requirements, measurements, etc."
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
        />
      </div>

      <Button
        size="lg"
        className="w-full"
        disabled={addToCartMutation.isPending}
        onClick={() => addToCartMutation.mutate()}
      >
        {addToCartMutation.isPending ? (
          <Loader2 className="mr-2 h-5 w-5 animate-spin" />
        ) : (
          <ShoppingCart className="mr-2 h-5 w-5" />
        )}
        Add to Cart
      </Button>

      {variantGroups && variantGroups.some((pvg) => pvg.isRequired) && (
        <p className="text-xs text-muted-foreground">
          <span className="text-destructive">*</span> Required selections
        </p>
      )}

      <Link href="/partner/cart" className="block text-center text-sm text-primary hover:underline">
        View Cart →
      </Link>
    </div>
  );
}

function VariantGroupSelector({
  pvg,
  selected,
  expanded,
  onToggleExpand,
  onSelect,
}: {
  pvg: ProductVariantGroup;
  selected?: SelectedVariantItem;
  expanded: boolean;
  onToggleExpand: () => void;
  onSelect: (item: SelectedVariantItem) => void;
}) {
  const { variantGroup } = pvg;
  const availableOptions = variantGroup.options.filter((o) => o.isAvailable);

  function buildItem(option: VariantOption): SelectedVariantItem {
    return {
      groupId: variantGroup.id,
      groupName: variantGroup.name,
      optionId: option.id,
      optionName: option.name,
      optionCode: option.code,
      hexColor: option.hexColor,
    };
  }

  if (variantGroup.displayType === "Dropdown") {
    return (
      <div>
        <label className="text-sm font-medium block mb-1">
          {variantGroup.name}
          {pvg.isRequired && <span className="text-destructive ml-1 text-xs">*</span>}
        </label>
        <select
          className="w-full text-sm border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-ring"
          value={selected?.optionId ?? ""}
          onChange={(e) => {
            const opt = availableOptions.find((o) => o.id === e.target.value);
            if (opt) onSelect(buildItem(opt));
          }}
        >
          <option value="">Select {variantGroup.name}...</option>
          {availableOptions.map((opt) => (
            <option key={opt.id} value={opt.id}>
              {opt.name} {opt.code ? `(${opt.code})` : ""}
            </option>
          ))}
        </select>
      </div>
    );
  }

  if (variantGroup.displayType === "RadioButtons") {
    return (
      <div>
        <div className="text-sm font-medium mb-2">
          {variantGroup.name}
          {pvg.isRequired && <span className="text-destructive ml-1 text-xs">*</span>}
        </div>
        <div className="space-y-1">
          {availableOptions.map((opt) => (
            <label key={opt.id} className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                name={`variant-${variantGroup.id}`}
                value={opt.id}
                checked={selected?.optionId === opt.id}
                onChange={() => onSelect(buildItem(opt))}
                className="text-primary"
              />
              <span className="text-sm">{opt.name}</span>
              {opt.code && <span className="text-xs text-muted-foreground">({opt.code})</span>}
            </label>
          ))}
        </div>
      </div>
    );
  }

  // Swatch (default) and ImageGrid
  return (
    <div className="border rounded-lg overflow-hidden">
      <button
        type="button"
        className="w-full flex items-center justify-between px-4 py-3 hover:bg-muted/50 transition-colors"
        onClick={onToggleExpand}
      >
        <div className="flex items-center gap-3">
          {selected ? (
            selected.hexColor ? (
              <div
                className="h-6 w-6 rounded-full border-2 border-primary shadow-sm flex-shrink-0"
                style={{ backgroundColor: selected.hexColor }}
              />
            ) : (
              <div className="h-6 w-6 rounded-full border-2 border-primary bg-muted flex-shrink-0 flex items-center justify-center">
                <Check className="h-3 w-3 text-primary" />
              </div>
            )
          ) : (
            <div className="h-6 w-6 rounded-full border-2 border-dashed border-muted-foreground flex-shrink-0" />
          )}
          <div className="text-left">
            <div className="font-medium text-sm">
              {variantGroup.name}
              {pvg.isRequired && <span className="text-destructive ml-1 text-xs">*</span>}
            </div>
            {selected ? (
              <div className="text-xs text-muted-foreground">
                {selected.optionName} — {selected.optionCode}
              </div>
            ) : (
              <div className="text-xs text-muted-foreground">
                {availableOptions.length} options available
              </div>
            )}
          </div>
        </div>
        {expanded ? (
          <ChevronUp className="h-4 w-4 text-muted-foreground" />
        ) : (
          <ChevronDown className="h-4 w-4 text-muted-foreground" />
        )}
      </button>

      {expanded && (
        <div className="px-4 pb-4 border-t bg-muted/20">
          <div className="grid grid-cols-5 gap-2 pt-3">
            {availableOptions.map((option) => {
              const isSelected = selected?.optionId === option.id;
              return (
                <button
                  key={option.id}
                  type="button"
                  title={`${option.name} (${option.code})`}
                  onClick={() => onSelect(buildItem(option))}
                  className="group flex flex-col items-center gap-1"
                >
                  {option.hexColor ? (
                    <div
                      className={`h-10 w-10 rounded-full border-2 transition-all ${
                        isSelected
                          ? "border-primary scale-110 shadow-md"
                          : "border-transparent hover:border-muted-foreground"
                      }`}
                      style={{ backgroundColor: option.hexColor }}
                    >
                      {isSelected && <Check className="h-5 w-5 text-white m-auto drop-shadow" />}
                    </div>
                  ) : option.imageUrl ? (
                    <div
                      className={`h-10 w-10 rounded-lg border-2 overflow-hidden transition-all ${
                        isSelected
                          ? "border-primary scale-110 shadow-md"
                          : "border-transparent hover:border-muted-foreground"
                      }`}
                    >
                      <Image
                        src={getImageUrl(option.imageUrl)}
                        alt={option.name}
                        width={40}
                        height={40}
                        className="object-cover"
                        unoptimized
                      />
                    </div>
                  ) : (
                    <div
                      className={`h-10 w-10 rounded-full border-2 bg-muted flex items-center justify-center transition-all ${
                        isSelected
                          ? "border-primary scale-110 shadow-md"
                          : "border-transparent hover:border-muted-foreground"
                      }`}
                    >
                      {isSelected ? (
                        <Check className="h-4 w-4 text-primary" />
                      ) : (
                        <span className="text-xs text-muted-foreground">{option.code.slice(0, 2)}</span>
                      )}
                    </div>
                  )}
                  <span className="text-xs text-muted-foreground truncate w-full text-center">
                    {option.name}
                  </span>
                </button>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
