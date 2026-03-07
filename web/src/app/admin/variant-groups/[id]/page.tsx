"use client";

import { use, useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminVariantsApi, VariantOption } from "@/lib/api/variants";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { ArrowLeft, Plus, Trash2, Pencil } from "lucide-react";
import Link from "next/link";
import { toast } from "sonner";

export default function VariantGroupDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const queryClient = useQueryClient();
  const [optionOpen, setOptionOpen] = useState(false);
  const [editingOption, setEditingOption] = useState<VariantOption | null>(null);
  const [optionForm, setOptionForm] = useState({
    name: "", code: "", hexColor: "", imageUrl: "", priceAdjustment: "", isAvailable: true, displayOrder: 0,
  });

  const { data: group, isLoading } = useQuery({
    queryKey: ["admin-variant-group", id],
    queryFn: () => adminVariantsApi.getById(id),
  });

  const addOptionMutation = useMutation({
    mutationFn: (data: typeof optionForm) => adminVariantsApi.addOption(id, {
      name: data.name,
      code: data.code,
      hexColor: data.hexColor || undefined,
      imageUrl: data.imageUrl || undefined,
      priceAdjustment: data.priceAdjustment ? parseFloat(data.priceAdjustment) : undefined,
      isAvailable: data.isAvailable,
      displayOrder: data.displayOrder,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-variant-group", id] });
      toast.success("Option added");
      setOptionOpen(false);
      resetOptionForm();
    },
    onError: (err: any) => toast.error(err.response?.data?.message ?? "Failed"),
  });

  const updateOptionMutation = useMutation({
    mutationFn: ({ optionId, data }: { optionId: string; data: typeof optionForm }) =>
      adminVariantsApi.updateOption(id, optionId, {
        name: data.name,
        code: data.code,
        hexColor: data.hexColor || undefined,
        imageUrl: data.imageUrl || undefined,
        priceAdjustment: data.priceAdjustment ? parseFloat(data.priceAdjustment) : undefined,
        isAvailable: data.isAvailable,
        displayOrder: data.displayOrder,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-variant-group", id] });
      toast.success("Option updated");
      setOptionOpen(false);
      setEditingOption(null);
      resetOptionForm();
    },
    onError: (err: any) => toast.error(err.response?.data?.message ?? "Failed"),
  });

  const deleteOptionMutation = useMutation({
    mutationFn: (optionId: string) => adminVariantsApi.deleteOption(id, optionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-variant-group", id] });
      toast.success("Option deleted");
    },
    onError: (err: any) => toast.error(err.response?.data?.message ?? "Failed"),
  });

  function resetOptionForm() {
    setOptionForm({ name: "", code: "", hexColor: "", imageUrl: "", priceAdjustment: "", isAvailable: true, displayOrder: 0 });
  }

  function openEdit(option: VariantOption) {
    setEditingOption(option);
    setOptionForm({
      name: option.name,
      code: option.code,
      hexColor: option.hexColor ?? "",
      imageUrl: option.imageUrl ?? "",
      priceAdjustment: option.priceAdjustment?.toString() ?? "",
      isAvailable: option.isAvailable,
      displayOrder: option.displayOrder,
    });
    setOptionOpen(true);
  }

  function handleSaveOption() {
    if (editingOption) {
      updateOptionMutation.mutate({ optionId: editingOption.id, data: optionForm });
    } else {
      addOptionMutation.mutate(optionForm);
    }
  }

  if (isLoading) return <div>Loading...</div>;
  if (!group) return <div>Variant group not found</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/admin/variant-groups">
          <Button variant="ghost" size="icon"><ArrowLeft className="h-4 w-4" /></Button>
        </Link>
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary">{group.name}</h1>
          <p className="text-muted-foreground">{group.displayType} · {group.options.length} options</p>
        </div>
      </div>

      <div className="rounded-md border p-4 space-y-2">
        <div className="flex gap-2">
          <Badge variant="outline">{group.displayType}</Badge>
          {group.isRequired && <Badge variant="outline">Required</Badge>}
          {group.allowMultiple && <Badge variant="outline">Multi-select</Badge>}
          {group.isActive ? <Badge className="bg-green-100 text-green-800">Active</Badge> : <Badge variant="secondary">Inactive</Badge>}
        </div>
        {group.description && <p className="text-sm text-muted-foreground">{group.description}</p>}
      </div>

      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-lg font-semibold">Options</h2>
          <Dialog open={optionOpen} onOpenChange={(v) => { setOptionOpen(v); if (!v) { setEditingOption(null); resetOptionForm(); } }}>
            <DialogTrigger asChild>
              <Button size="sm"><Plus className="h-4 w-4 mr-2" />Add Option</Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>{editingOption ? "Edit Option" : "Add Option"}</DialogTitle>
              </DialogHeader>
              <div className="space-y-4 pt-2">
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <Label>Name *</Label>
                    <Input value={optionForm.name} onChange={(e) => setOptionForm((f) => ({ ...f, name: e.target.value }))} placeholder="Royal Blue" />
                  </div>
                  <div>
                    <Label>Code *</Label>
                    <Input value={optionForm.code} onChange={(e) => setOptionForm((f) => ({ ...f, code: e.target.value }))} placeholder="RB-001" />
                  </div>
                </div>
                {group.displayType === "Swatch" && (
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <Label>Hex Color</Label>
                      <div className="flex gap-2">
                        <Input type="color" value={optionForm.hexColor || "#000000"} onChange={(e) => setOptionForm((f) => ({ ...f, hexColor: e.target.value }))} className="w-12 p-1 h-9" />
                        <Input value={optionForm.hexColor} onChange={(e) => setOptionForm((f) => ({ ...f, hexColor: e.target.value }))} placeholder="#1E3A8A" />
                      </div>
                    </div>
                    <div>
                      <Label>Image URL</Label>
                      <Input value={optionForm.imageUrl} onChange={(e) => setOptionForm((f) => ({ ...f, imageUrl: e.target.value }))} placeholder="/uploads/..." />
                    </div>
                  </div>
                )}
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <Label>Price Adjustment</Label>
                    <Input type="number" value={optionForm.priceAdjustment} onChange={(e) => setOptionForm((f) => ({ ...f, priceAdjustment: e.target.value }))} placeholder="0.00" />
                  </div>
                  <div>
                    <Label>Display Order</Label>
                    <Input type="number" value={optionForm.displayOrder} onChange={(e) => setOptionForm((f) => ({ ...f, displayOrder: parseInt(e.target.value) || 0 }))} />
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Switch checked={optionForm.isAvailable} onCheckedChange={(v) => setOptionForm((f) => ({ ...f, isAvailable: v }))} />
                  <Label>Available</Label>
                </div>
                <Button
                  className="w-full"
                  onClick={handleSaveOption}
                  disabled={!optionForm.name || !optionForm.code || addOptionMutation.isPending || updateOptionMutation.isPending}
                >
                  {editingOption ? "Save Changes" : "Add Option"}
                </Button>
              </div>
            </DialogContent>
          </Dialog>
        </div>

        <div className="rounded-md border">
          <table className="w-full">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="text-left p-3 font-medium">Option</th>
                <th className="text-left p-3 font-medium">Code</th>
                <th className="text-left p-3 font-medium">Preview</th>
                <th className="text-left p-3 font-medium">Status</th>
                <th className="text-right p-3 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {group.options.length === 0 && (
                <tr><td colSpan={5} className="text-center p-6 text-muted-foreground">No options yet</td></tr>
              )}
              {group.options.map((opt) => (
                <tr key={opt.id} className="border-b hover:bg-muted/30">
                  <td className="p-3 font-medium">{opt.name}</td>
                  <td className="p-3 text-muted-foreground text-sm font-mono">{opt.code}</td>
                  <td className="p-3">
                    {opt.hexColor && (
                      <div className="w-6 h-6 rounded-full border" style={{ backgroundColor: opt.hexColor }} title={opt.hexColor} />
                    )}
                  </td>
                  <td className="p-3">
                    {opt.isAvailable
                      ? <Badge className="bg-green-100 text-green-800">Available</Badge>
                      : <Badge variant="secondary">Unavailable</Badge>
                    }
                  </td>
                  <td className="p-3 text-right">
                    <div className="flex items-center justify-end gap-2">
                      <Button variant="ghost" size="icon" onClick={() => openEdit(opt)}><Pencil className="h-4 w-4" /></Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => { if (window.confirm(`Delete "${opt.name}"?`)) deleteOptionMutation.mutate(opt.id); }}
                      >
                        <Trash2 className="h-4 w-4 text-destructive" />
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
