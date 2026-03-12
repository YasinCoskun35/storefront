"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { toast } from "sonner";
import { catalogApi, CreateCategoryDto, Category } from "@/lib/api";
import { Loader2 } from "lucide-react";

interface CategoryFormProps {
  categoryId?: string;
  initialData?: Category;
}

export function CategoryForm({ categoryId, initialData }: CategoryFormProps) {
  const router = useRouter();
  const queryClient = useQueryClient();

  // Form state
  const [name, setName] = useState(initialData?.name || "");
  const [description, setDescription] = useState(initialData?.description || "");
  const [slug, setSlug] = useState(initialData?.slug || "");
  const [parentId, setParentId] = useState(initialData?.parentId || "");
  const [displayOrder, setDisplayOrder] = useState(initialData?.displayOrder?.toString() || "0");
  const [isActive, setIsActive] = useState(initialData?.isActive ?? true);
  const [showInNavbar, setShowInNavbar] = useState(initialData?.showInNavbar ?? false);

  // Fetch categories for parent dropdown (all categories)
  const { data: categories } = useQuery({
    queryKey: ["categories", "all"],
    queryFn: () => catalogApi.getCategories({ all: true }),
  });

  // Auto-generate slug from name
  const generateSlug = (text: string) => {
    return text
      .toLowerCase()
      .replace(/[^a-z0-9\s-]/g, '')
      .replace(/\s+/g, '-')
      .replace(/-+/g, '-')
      .trim();
  };

  const handleNameChange = (value: string) => {
    setName(value);
    // Auto-generate slug if it's empty or hasn't been manually edited
    if (!slug || slug === generateSlug(name)) {
      setSlug(generateSlug(value));
    }
  };

  const createCategoryMutation = useMutation({
    mutationFn: (data: CreateCategoryDto) => catalogApi.createCategory(data),
    onSuccess: () => {
      toast.success("Category created successfully");
      router.push("/admin/categories");
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || "Failed to create category");
    },
  });

  const updateCategoryMutation = useMutation({
    mutationFn: (data: CreateCategoryDto) => catalogApi.updateCategory(categoryId!, data),
    onSuccess: () => {
      toast.success("Category updated successfully");
      queryClient.invalidateQueries({ queryKey: ["admin-categories"] });
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      queryClient.invalidateQueries({ queryKey: ["categories-tree"] });
      router.push("/admin/categories");
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || "Failed to update category");
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!name) {
      toast.error("Please enter a category name");
      return;
    }

    const categoryData: CreateCategoryDto = {
      name,
      description: description || undefined,
      slug: slug || undefined,
      parentId: parentId || undefined,
      displayOrder: parseInt(displayOrder) || 0,
      isActive,
      showInNavbar,
    };

    if (categoryId) {
      updateCategoryMutation.mutate(categoryData);
    } else {
      createCategoryMutation.mutate(categoryData);
    }
  };

  const isLoading = createCategoryMutation.isPending || updateCategoryMutation.isPending;

  // Filter out current category and its descendants from parent options
  const availableParentCategories = categories?.filter(
    (cat) => cat.id !== categoryId
  );

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Basic Information */}
      <Card>
        <CardHeader>
          <CardTitle>Category Information</CardTitle>
          <CardDescription>
            Basic category details
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Category Name *</Label>
            <Input
              id="name"
              value={name}
              onChange={(e) => handleNameChange(e.target.value)}
              placeholder="e.g., Power Tools"
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="slug">URL Slug</Label>
            <Input
              id="slug"
              value={slug}
              onChange={(e) => setSlug(e.target.value)}
              placeholder="e.g., power-tools"
            />
            <p className="text-xs text-muted-foreground">
              Auto-generated from name if left empty. Only lowercase letters, numbers, and hyphens.
            </p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Describe this category"
              rows={4}
            />
          </div>
        </CardContent>
      </Card>

      {/* Organization */}
      <Card>
        <CardHeader>
          <CardTitle>Organization</CardTitle>
          <CardDescription>
            Categorize and organize
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="parent">Parent Category</Label>
            <Select
              value={parentId || "none"} // Fallback to "none" if parentId is null/empty
              onValueChange={(value) => setParentId(value === "none" ? "" : value)}
            >
              <SelectTrigger id="parent">
                <SelectValue placeholder="None (Top Level)" />
              </SelectTrigger>
              <SelectContent>
                {/* Use "none" as a valid string value */}
                <SelectItem value="none">None (Top Level)</SelectItem>

                {availableParentCategories?.map((category) => (
                  <SelectItem key={category.id} value={category.id}>
                    {category.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <p className="text-xs text-muted-foreground">
              Select a parent category to create a subcategory
            </p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="displayOrder">Display Order</Label>
            <Input
              id="displayOrder"
              type="number"
              value={displayOrder}
              onChange={(e) => setDisplayOrder(e.target.value)}
              placeholder="0"
            />
            <p className="text-xs text-muted-foreground">
              Lower numbers appear first (0 = first)
            </p>
          </div>

          <div className="flex flex-col gap-3">
            <div className="flex items-center space-x-2">
              <input
                type="checkbox"
                id="isActive"
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300"
              />
              <Label htmlFor="isActive" className="cursor-pointer">
                Active (visible to customers)
              </Label>
            </div>
            <div className="flex items-center space-x-2">
              <input
                type="checkbox"
                id="showInNavbar"
                checked={showInNavbar}
                onChange={(e) => setShowInNavbar(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300"
              />
              <Label htmlFor="showInNavbar" className="cursor-pointer">
                Show in navbar (main category navigation)
              </Label>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Form Actions */}
      <div className="flex gap-4">
        <Button type="submit" disabled={isLoading}>
          {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {categoryId ? "Update Category" : "Create Category"}
        </Button>
        <Button
          type="button"
          variant="outline"
          onClick={() => router.push("/admin/categories")}
        >
          Cancel
        </Button>
      </div>
    </form>
  );
}



