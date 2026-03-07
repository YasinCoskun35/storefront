"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { adminVariantsApi, VariantGroup } from "@/lib/api/variants";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { Switch } from "@/components/ui/switch";
import { Plus, Pencil, Trash2, Layers } from "lucide-react";
import Link from "next/link";
import { toast } from "sonner";

export default function AdminVariantGroupsPage() {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState({
    name: "",
    description: "",
    displayType: "Swatch",
    isRequired: true,
    allowMultiple: false,
    displayOrder: 0,
  });

  const { data: groups, isLoading } = useQuery({
    queryKey: ["admin-variant-groups"],
    queryFn: () => adminVariantsApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: adminVariantsApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-variant-groups"] });
      toast.success("Variant group created");
      setOpen(false);
      setForm({ name: "", description: "", displayType: "Swatch", isRequired: true, allowMultiple: false, displayOrder: 0 });
    },
    onError: (err: any) => toast.error(err.response?.data?.message ?? "Failed to create"),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => adminVariantsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-variant-groups"] });
      toast.success("Variant group deleted");
    },
    onError: (err: any) => toast.error(err.response?.data?.message ?? "Failed to delete"),
  });

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary">Variant Groups</h1>
          <p className="text-muted-foreground">Manage product variant groups (fabric, finish, size, etc.)</p>
        </div>
        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              New Variant Group
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create Variant Group</DialogTitle>
            </DialogHeader>
            <div className="space-y-4 pt-2">
              <div>
                <Label>Name *</Label>
                <Input
                  value={form.name}
                  onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                  placeholder="e.g. Fabric, Leg Finish"
                />
              </div>
              <div>
                <Label>Description</Label>
                <Input
                  value={form.description}
                  onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                  placeholder="Optional description"
                />
              </div>
              <div>
                <Label>Display Type</Label>
                <Select value={form.displayType} onValueChange={(v) => setForm((f) => ({ ...f, displayType: v }))}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Swatch">Swatch (color circles)</SelectItem>
                    <SelectItem value="Dropdown">Dropdown</SelectItem>
                    <SelectItem value="RadioButtons">Radio Buttons</SelectItem>
                    <SelectItem value="ImageGrid">Image Grid</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="flex items-center gap-6">
                <div className="flex items-center gap-2">
                  <Switch
                    checked={form.isRequired}
                    onCheckedChange={(v) => setForm((f) => ({ ...f, isRequired: v }))}
                  />
                  <Label>Required</Label>
                </div>
                <div className="flex items-center gap-2">
                  <Switch
                    checked={form.allowMultiple}
                    onCheckedChange={(v) => setForm((f) => ({ ...f, allowMultiple: v }))}
                  />
                  <Label>Allow Multiple</Label>
                </div>
              </div>
              <div>
                <Label>Display Order</Label>
                <Input
                  type="number"
                  value={form.displayOrder}
                  onChange={(e) => setForm((f) => ({ ...f, displayOrder: parseInt(e.target.value) || 0 }))}
                />
              </div>
              <Button
                className="w-full"
                onClick={() => createMutation.mutate(form)}
                disabled={!form.name || createMutation.isPending}
              >
                Create
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="text-left p-3 font-medium">Name</th>
              <th className="text-left p-3 font-medium">Display Type</th>
              <th className="text-left p-3 font-medium">Options</th>
              <th className="text-left p-3 font-medium">Status</th>
              <th className="text-right p-3 font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {(groups ?? []).length === 0 && (
              <tr>
                <td colSpan={5} className="text-center p-8 text-muted-foreground">
                  <Layers className="h-8 w-8 mx-auto mb-2 opacity-50" />
                  No variant groups yet. Create one to get started.
                </td>
              </tr>
            )}
            {(groups ?? []).map((g: VariantGroup) => (
              <tr key={g.id} className="border-b hover:bg-muted/30">
                <td className="p-3">
                  <div className="font-medium">{g.name}</div>
                  {g.description && <div className="text-sm text-muted-foreground">{g.description}</div>}
                </td>
                <td className="p-3">
                  <Badge variant="outline">{g.displayType}</Badge>
                </td>
                <td className="p-3 text-muted-foreground">{g.optionCount ?? 0} options</td>
                <td className="p-3">
                  {g.isActive
                    ? <Badge className="bg-green-100 text-green-800">Active</Badge>
                    : <Badge variant="secondary">Inactive</Badge>
                  }
                </td>
                <td className="p-3 text-right">
                  <div className="flex items-center justify-end gap-2">
                    <Link href={`/admin/variant-groups/${g.id}`}>
                      <Button variant="ghost" size="icon">
                        <Pencil className="h-4 w-4" />
                      </Button>
                    </Link>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => {
                        if (window.confirm(`Delete "${g.name}"?`)) deleteMutation.mutate(g.id);
                      }}
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
  );
}
