"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery, useMutation } from "@tanstack/react-query";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useToast } from "@/hooks/use-toast";
import { catalogApi, CreateProductDto, ProductDetail } from "@/lib/api";
import { Loader2, Upload, X } from "lucide-react";

interface ProductFormProps {
  productId?: string;
  initialData?: ProductDetail;
}

export function ProductForm({ productId, initialData }: ProductFormProps) {
  const router = useRouter();
  const { toast } = useToast();
  
  // Form state
  const [name, setName] = useState(initialData?.name || "");
  const [sku, setSku] = useState(initialData?.sku || "");
  const [description, setDescription] = useState(initialData?.description || "");
  const [shortDescription, setShortDescription] = useState(initialData?.shortDescription || "");
  const [price] = useState(initialData?.price?.toString() || "");
  const [compareAtPrice] = useState(initialData?.compareAtPrice?.toString() || "");
  const [stockStatus] = useState(initialData?.stockStatus?.toString() || "1");
  const [quantity] = useState(initialData?.quantity?.toString() || "0");
  const [categoryId, setCategoryId] = useState(initialData?.categoryId || "");
  const [weight, setWeight] = useState(initialData?.weight?.toString() || "");
  const [length, setLength] = useState(initialData?.length?.toString() || "");
  const [width, setWidth] = useState(initialData?.width?.toString() || "");
  const [height, setHeight] = useState(initialData?.height?.toString() || "");
  const [isActive, setIsActive] = useState(initialData?.isActive ?? true);
  const [isFeatured, setIsFeatured] = useState(initialData?.isFeatured ?? false);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string>(initialData?.primaryImageUrl || "");

  // Fetch categories (all for dropdown)
  const { data: categories } = useQuery({
    queryKey: ["categories", "all"],
    queryFn: () => catalogApi.getCategories({ all: true }),
  });

  // Create product mutation
  const createProductMutation = useMutation({
    mutationFn: (data: CreateProductDto) => catalogApi.createProduct(data),
    onSuccess: async (response) => {
      if (imageFile) {
        try {
          await catalogApi.uploadProductImage(response.id, imageFile, true);
        } catch (error) {
          console.error("Failed to upload image:", error);
        }
      }
      toast({ title: "Product created", description: "The product has been created successfully." });
      router.push("/admin/products");
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to create product",
        variant: "destructive",
      });
    },
  });

  // Update product mutation
  const updateProductMutation = useMutation({
    mutationFn: (data: Parameters<typeof catalogApi.updateProduct>[1]) =>
      catalogApi.updateProduct(productId!, data),
    onSuccess: async () => {
      if (imageFile) {
        try {
          await catalogApi.uploadProductImage(productId!, imageFile, true);
        } catch (error) {
          console.error("Failed to upload image:", error);
        }
      }
      toast({ title: "Product updated", description: "Changes saved successfully." });
    },
    onError: (error: any) => {
      toast({
        title: "Error",
        description: error.response?.data?.message || "Failed to update product",
        variant: "destructive",
      });
    },
  });

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setImageFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setImagePreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleRemoveImage = () => {
    setImageFile(null);
    setImagePreview("");
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Validation
    if (!name || !sku || !categoryId) {
      toast({
        title: "Validation Error",
        description: "Please fill in all required fields (Name, SKU, Category)",
        variant: "destructive",
      });
      return;
    }

    if (productId) {
      updateProductMutation.mutate({
        name,
        sku,
        description: description || undefined,
        shortDescription: shortDescription || undefined,
        categoryId,
        weight: weight ? parseFloat(weight) : undefined,
        length: length ? parseFloat(length) : undefined,
        width: width ? parseFloat(width) : undefined,
        height: height ? parseFloat(height) : undefined,
        isActive,
        isFeatured,
      });
    } else {
      const productData: CreateProductDto = {
        name,
        sku,
        description: description || undefined,
        shortDescription: shortDescription || undefined,
        price: price ? parseFloat(price) : undefined,
        compareAtPrice: compareAtPrice ? parseFloat(compareAtPrice) : undefined,
        stockStatus: stockStatus || undefined,
        quantity: quantity ? parseInt(quantity) : undefined,
        categoryId,
        productType: "Simple",
        canBeSoldSeparately: true,
        weight: weight ? parseFloat(weight) : undefined,
        length: length ? parseFloat(length) : undefined,
        width: width ? parseFloat(width) : undefined,
        height: height ? parseFloat(height) : undefined,
        isActive,
        isFeatured,
      };
      createProductMutation.mutate(productData);
    }
  };

  const isLoading = createProductMutation.isPending || updateProductMutation.isPending;

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Basic Information */}
      <Card>
        <CardHeader>
          <CardTitle>Basic Information</CardTitle>
          <CardDescription>
            Essential product details
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="name">Product Name *</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g., DeWalt 20V MAX Drill"
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="sku">SKU *</Label>
              <Input
                id="sku"
                value={sku}
                onChange={(e) => setSku(e.target.value)}
                placeholder="e.g., DW-20V-DRILL"
                required
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="shortDescription">Short Description</Label>
            <Input
              id="shortDescription"
              value={shortDescription}
              onChange={(e) => setShortDescription(e.target.value)}
              placeholder="Brief product summary"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Full Description</Label>
            <Textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Detailed product description"
              rows={5}
            />
          </div>
        </CardContent>
      </Card>

      {/* Organization */}
      <Card>
        <CardHeader>
          <CardTitle>Organization</CardTitle>
          <CardDescription>
            Categorize and organize your product
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="category">Category *</Label>
            <Select value={categoryId} onValueChange={setCategoryId}>
              <SelectTrigger id="category">
                <SelectValue placeholder="Select a category" />
              </SelectTrigger>
              <SelectContent>
                {categories?.map((category) => (
                  <SelectItem key={category.id} value={category.id}>
                    {category.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex gap-4">
            <div className="flex items-center space-x-2">
              <input
                type="checkbox"
                id="isActive"
                checked={isActive}
                onChange={(e) => setIsActive(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300"
              />
              <Label htmlFor="isActive" className="cursor-pointer">
                Active
              </Label>
            </div>

            <div className="flex items-center space-x-2">
              <input
                type="checkbox"
                id="isFeatured"
                checked={isFeatured}
                onChange={(e) => setIsFeatured(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300"
              />
              <Label htmlFor="isFeatured" className="cursor-pointer">
                Featured Product
              </Label>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Shipping */}
      <Card>
        <CardHeader>
          <CardTitle>Shipping Information</CardTitle>
          <CardDescription>
            Product dimensions and weight
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-4">
            <div className="space-y-2">
              <Label htmlFor="weight">Weight (kg)</Label>
              <Input
                id="weight"
                type="number"
                step="0.01"
                value={weight}
                onChange={(e) => setWeight(e.target.value)}
                placeholder="0.00"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="length">Length (cm)</Label>
              <Input
                id="length"
                type="number"
                step="0.01"
                value={length}
                onChange={(e) => setLength(e.target.value)}
                placeholder="0.00"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="width">Width (cm)</Label>
              <Input
                id="width"
                type="number"
                step="0.01"
                value={width}
                onChange={(e) => setWidth(e.target.value)}
                placeholder="0.00"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="height">Height (cm)</Label>
              <Input
                id="height"
                type="number"
                step="0.01"
                value={height}
                onChange={(e) => setHeight(e.target.value)}
                placeholder="0.00"
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Product Image */}
      <Card>
        <CardHeader>
          <CardTitle>Product Image</CardTitle>
          <CardDescription>
            Upload a primary product image
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {imagePreview ? (
            <div className="relative inline-block">
              <img
                src={imagePreview}
                alt="Product preview"
                className="h-48 w-48 rounded-lg border object-cover"
              />
              <Button
                type="button"
                variant="destructive"
                size="icon"
                className="absolute -right-2 -top-2"
                onClick={handleRemoveImage}
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
          ) : (
            <div className="flex items-center justify-center w-full">
              <label
                htmlFor="image-upload"
                className="flex flex-col items-center justify-center w-full h-48 border-2 border-dashed rounded-lg cursor-pointer bg-muted hover:bg-muted/80"
              >
                <div className="flex flex-col items-center justify-center pt-5 pb-6">
                  <Upload className="w-10 h-10 mb-3 text-muted-foreground" />
                  <p className="mb-2 text-sm text-muted-foreground">
                    <span className="font-semibold">Click to upload</span> or drag and drop
                  </p>
                  <p className="text-xs text-muted-foreground">
                    PNG, JPG or WEBP (MAX. 5MB)
                  </p>
                </div>
                <input
                  id="image-upload"
                  type="file"
                  className="hidden"
                  accept="image/*"
                  onChange={handleImageChange}
                />
              </label>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Form Actions */}
      <div className="flex gap-4">
        <Button type="submit" disabled={isLoading}>
          {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {productId ? "Update Product" : "Create Product"}
        </Button>
        <Button
          type="button"
          variant="outline"
          onClick={() => router.push("/admin/products")}
        >
          Cancel
        </Button>
      </div>
    </form>
  );
}



