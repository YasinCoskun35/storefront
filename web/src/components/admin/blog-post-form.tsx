"use client";

import { useState, useEffect } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loader2, Save } from "lucide-react";
import { toast } from "sonner";

interface BlogPostFormProps {
  postId?: string;
  initialData?: {
    title: string;
    slug: string;
    summary?: string;
    body?: string;
    featuredImage?: string;
    author?: string;
    isPublished: boolean;
    tags?: string;
    category?: string;
  } | null;
}

function generateSlug(title: string): string {
  return title
    .toLowerCase()
    .replace(/ğ/g, "g").replace(/ü/g, "u").replace(/ş/g, "s")
    .replace(/ı/g, "i").replace(/ö/g, "o").replace(/ç/g, "c")
    .replace(/[^a-z0-9\s-]/g, "")
    .trim()
    .replace(/\s+/g, "-");
}

export function BlogPostForm({ postId, initialData }: BlogPostFormProps) {
  const router = useRouter();
  const queryClient = useQueryClient();

  const [form, setForm] = useState({
    title: initialData?.title || "",
    slug: initialData?.slug || "",
    summary: initialData?.summary || "",
    body: initialData?.body || "",
    featuredImage: initialData?.featuredImage || "",
    author: initialData?.author || "",
    isPublished: initialData?.isPublished ?? false,
    tags: initialData?.tags || "",
    category: initialData?.category || "",
  });

  useEffect(() => {
    if (initialData) {
      setForm({
        title: initialData.title || "",
        slug: initialData.slug || "",
        summary: initialData.summary || "",
        body: initialData.body || "",
        featuredImage: initialData.featuredImage || "",
        author: initialData.author || "",
        isPublished: initialData.isPublished ?? false,
        tags: initialData.tags || "",
        category: initialData.category || "",
      });
    }
  }, [initialData]);

  const handleTitleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const title = e.target.value;
    setForm((prev) => ({
      ...prev,
      title,
      slug: postId ? prev.slug : generateSlug(title),
    }));
  };

  const saveMutation = useMutation({
    mutationFn: async (data: typeof form) => {
      if (postId) {
        const res = await api.put(`/api/content/blog/${postId}`, {
          id: postId,
          ...data,
          metaTitle: data.title,
          metaDescription: data.summary,
        });
        return res.data;
      } else {
        const res = await api.post("/api/content/blog", {
          ...data,
          metaTitle: data.title,
          metaDescription: data.summary,
        });
        return res.data;
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-blog-all"] });
      toast.success(postId ? "Post updated" : "Post created");
      if (!postId) {
        router.push("/admin/blog");
      }
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || "Failed to save post");
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.title.trim()) {
      toast.error("Title is required");
      return;
    }
    if (!form.body.trim()) {
      toast.error("Body content is required");
      return;
    }
    saveMutation.mutate(form);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main content */}
        <div className="lg:col-span-2 space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Post Content</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="title">Title *</Label>
                <Input
                  id="title"
                  value={form.title}
                  onChange={handleTitleChange}
                  placeholder="Post title..."
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="slug">Slug</Label>
                <Input
                  id="slug"
                  value={form.slug}
                  onChange={(e) => setForm({ ...form, slug: e.target.value })}
                  placeholder="post-url-slug"
                  className="font-mono text-sm"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="summary">Summary</Label>
                <Textarea
                  id="summary"
                  value={form.summary}
                  onChange={(e) => setForm({ ...form, summary: e.target.value })}
                  placeholder="Brief summary of the post..."
                  rows={3}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="body">Content *</Label>
                <Textarea
                  id="body"
                  value={form.body}
                  onChange={(e) => setForm({ ...form, body: e.target.value })}
                  placeholder="Write your blog post content here (HTML supported)..."
                  rows={18}
                  className="font-mono text-sm"
                />
                <p className="text-xs text-muted-foreground">
                  HTML tags are supported. Use &lt;p&gt;, &lt;h2&gt;, &lt;ul&gt;, &lt;strong&gt;, etc.
                </p>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Settings</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <Label htmlFor="published">Published</Label>
                  <p className="text-xs text-muted-foreground">Make visible to readers</p>
                </div>
                <Switch
                  id="published"
                  checked={form.isPublished}
                  onCheckedChange={(v: boolean) => setForm({ ...form, isPublished: v })}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="author">Author</Label>
                <Input
                  id="author"
                  value={form.author}
                  onChange={(e) => setForm({ ...form, author: e.target.value })}
                  placeholder="Author name"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="category">Category</Label>
                <Input
                  id="category"
                  value={form.category}
                  onChange={(e) => setForm({ ...form, category: e.target.value })}
                  placeholder="e.g. News, Tips, Updates"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="tags">Tags</Label>
                <Input
                  id="tags"
                  value={form.tags}
                  onChange={(e) => setForm({ ...form, tags: e.target.value })}
                  placeholder="tag1, tag2, tag3"
                />
                <p className="text-xs text-muted-foreground">Comma separated</p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="featuredImage">Featured Image URL</Label>
                <Input
                  id="featuredImage"
                  value={form.featuredImage}
                  onChange={(e) => setForm({ ...form, featuredImage: e.target.value })}
                  placeholder="https://..."
                />
              </div>
            </CardContent>
          </Card>

          <Button type="submit" className="w-full" disabled={saveMutation.isPending}>
            {saveMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            <Save className="mr-2 h-4 w-4" />
            {postId ? "Save Changes" : "Create Post"}
          </Button>
        </div>
      </div>
    </form>
  );
}
