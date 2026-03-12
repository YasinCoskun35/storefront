"use client";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { adminVariantsApi, ProductVariantGroup } from "@/lib/api/variants";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ChevronDown, ChevronUp, Layers, Loader2, Plus, Trash2 } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

interface ProductVariantGroupsProps {
  productId: string;
}

export function ProductVariantGroups({ productId }: ProductVariantGroupsProps) {
  const queryClient = useQueryClient();
  const [showAssignDialog, setShowAssignDialog] = useState(false);
  const [selectedGroupId, setSelectedGroupId] = useState("");
  const [isRequired, setIsRequired] = useState(true);

  const { data: assigned, isLoading: loadingAssigned } = useQuery({
    queryKey: ["product-variant-groups", productId],
    queryFn: () => adminVariantsApi.getProductVariantGroups(productId),
    enabled: !!productId,
  });

  const { data: allGroups, isLoading: loadingGroups } = useQuery({
    queryKey: ["admin-variant-groups"],
    queryFn: () => adminVariantsApi.getAll({ isActive: true }),
  });

  const assignMutation = useMutation({
    mutationFn: () => adminVariantsApi.assignToProduct(productId, { variantGroupId: selectedGroupId, isRequired }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["product-variant-groups", productId] });
      setShowAssignDialog(false);
      setSelectedGroupId("");
      toast.success("Variant group assigned");
    },
    onError: (err: any) => toast.error(err.response?.data?.message || "Failed to assign"),
  });

  const removeMutation = useMutation({
    mutationFn: (groupId: string) => adminVariantsApi.removeFromProduct(productId, groupId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["product-variant-groups", productId] });
      toast.success("Variant group removed");
    },
    onError: () => toast.error("Failed to remove"),
  });

  const assignedGroupIds = new Set(assigned?.map((a) => a.variantGroupId) ?? []);
  const availableGroups = allGroups?.filter((g) => !assignedGroupIds.has(g.id)) ?? [];

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="font-semibold text-sm">Variant Groups</h3>
          <p className="text-xs text-muted-foreground">
            Assign variant groups (fabric, finish, size). Partners select options when ordering.
          </p>
        </div>
        <Button
          type="button"
          size="sm"
          variant="outline"
          onClick={() => setShowAssignDialog(true)}
          disabled={availableGroups.length === 0}
        >
          <Plus className="h-4 w-4 mr-1" />
          Assign Group
        </Button>
      </div>

      {loadingAssigned ? (
        <div className="flex justify-center py-6">
          <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
        </div>
      ) : assigned?.length === 0 ? (
        <div className="border border-dashed rounded-lg py-8 text-center text-muted-foreground">
          <Layers className="h-8 w-8 mx-auto mb-2 opacity-30" />
          <p className="text-sm">No variant groups assigned.</p>
          <p className="text-xs">Partners won&apos;t need to select variants for this product.</p>
        </div>
      ) : (
        <div className="space-y-2">
          {assigned?.map((assignment) => (
            <AssignedGroupRow
              key={assignment.id}
              assignment={assignment}
              onRemove={() => removeMutation.mutate(assignment.variantGroupId)}
              isRemoving={removeMutation.isPending}
            />
          ))}
        </div>
      )}

      <Dialog open={showAssignDialog} onOpenChange={setShowAssignDialog}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Assign Variant Group</DialogTitle>
            <DialogDescription>Select a variant group to assign to this product</DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Variant Group</Label>
              {loadingGroups ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <Select value={selectedGroupId} onValueChange={setSelectedGroupId}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select a group..." />
                  </SelectTrigger>
                  <SelectContent>
                    {availableGroups.map((group) => (
                      <SelectItem key={group.id} value={group.id}>
                        <span className="font-medium">{group.name}</span>
                        <span className="text-muted-foreground ml-2 text-xs">
                          {group?.displayType} · {group?.options?.length} options
                        </span>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            </div>

            <div className="flex items-center justify-between py-2 px-3 bg-muted/50 rounded-lg">
              <div>
                <Label htmlFor="variantIsRequired">Required selection</Label>
                <p className="text-xs text-muted-foreground">
                  Partner must pick an option to add to cart
                </p>
              </div>
              <Switch
                id="variantIsRequired"
                checked={isRequired}
                onCheckedChange={(v: boolean) => setIsRequired(v)}
              />
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => setShowAssignDialog(false)}>
              Cancel
            </Button>
            <Button
              type="button"
              disabled={!selectedGroupId || assignMutation.isPending}
              onClick={() => assignMutation.mutate()}
            >
              {assignMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Assign
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function AssignedGroupRow({
  assignment,
  onRemove,
  isRemoving,
}: {
  assignment: ProductVariantGroup;
  onRemove: () => void;
  isRemoving: boolean;
}) {
  const [expanded, setExpanded] = useState(false);
  const { variantGroup } = assignment;

  return (
    <Card className="overflow-hidden">
      <CardHeader className="p-3">
        <div className="flex items-center justify-between">
          <div
            className="flex items-center gap-3 cursor-pointer flex-1"
            onClick={() => setExpanded(!expanded)}
          >
            <Layers className="h-4 w-4 text-muted-foreground flex-shrink-0" />
            <div className="flex-1">
              <div className="font-medium text-sm">{variantGroup.name}</div>
              <div className="flex items-center gap-2 mt-0.5">
                <span className="text-xs text-muted-foreground">{variantGroup.displayType}</span>
                <span className="text-xs text-muted-foreground">·</span>
                <span className="text-xs text-muted-foreground">
                  {variantGroup.options.length} options
                </span>
                {assignment.isRequired && (
                  <Badge variant="outline" className="text-xs py-0 px-1">Required</Badge>
                )}
              </div>
            </div>
            {expanded ? (
              <ChevronUp className="h-4 w-4 text-muted-foreground" />
            ) : (
              <ChevronDown className="h-4 w-4 text-muted-foreground" />
            )}
          </div>
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={onRemove}
            disabled={isRemoving}
            className="ml-2"
          >
            <Trash2 className="h-4 w-4 text-destructive" />
          </Button>
        </div>
      </CardHeader>

      {expanded && (
        <CardContent className="pt-0 pb-3 px-3">
          <div className="grid grid-cols-6 gap-2">
            {variantGroup.options.slice(0, 12).map((opt) => (
              <div key={opt.id} className="text-center" title={`${opt.name} (${opt.code})`}>
                {opt.hexColor ? (
                  <div
                    className="h-8 w-8 rounded-full border-2 border-white shadow-sm mx-auto"
                    style={{ backgroundColor: opt.hexColor }}
                  />
                ) : (
                  <div className="h-8 w-8 rounded-full bg-muted border mx-auto flex items-center justify-center text-xs text-muted-foreground">
                    {opt.code.slice(0, 2)}
                  </div>
                )}
                <div className="text-xs text-muted-foreground truncate mt-0.5">{opt.name}</div>
              </div>
            ))}
            {variantGroup.options.length > 12 && (
              <div className="text-center">
                <div className="h-8 w-8 rounded-full bg-muted flex items-center justify-center mx-auto text-xs text-muted-foreground">
                  +{variantGroup.options.length - 12}
                </div>
              </div>
            )}
          </div>
        </CardContent>
      )}
    </Card>
  );
}
