"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Plus, Pencil, Trash2, Loader2, FileText, Eye } from "lucide-react";
import Link from "next/link";
import { toast } from "sonner";

interface BlogPost {
  id: string;
  title: string;
  slug: string;
  author?: string;
  isPublished: boolean;
  publishedAt?: string;
  viewCount: number;
  category?: string;
}

async function fetchAllPosts(page: number): Promise<{ items: BlogPost[]; totalCount: number; totalPages: number }> {
  const res = await api.get("/api/content/blog/admin/all", {
    params: { pageNumber: page, pageSize: 20 },
  });
  return res.data;
}

async function deletePost(id: string) {
  await api.delete(`/api/content/blog/${id}`);
}

export default function AdminBlogPage() {
  const [page, setPage] = useState(1);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ["admin-blog-all", page],
    queryFn: () => fetchAllPosts(page),
  });

  const deleteMutation = useMutation({
    mutationFn: deletePost,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-blog-all"] });
      toast.success("Blog post deleted");
    },
    onError: () => toast.error("Failed to delete post"),
  });

  const handleDelete = (id: string, title: string) => {
    if (confirm(`Delete "${title}"? This cannot be undone.`)) {
      deleteMutation.mutate(id);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary flex items-center gap-2">
            <FileText className="h-8 w-8" />
            Blog Posts
          </h1>
          <p className="text-muted-foreground">Manage your blog content</p>
        </div>
        <Link href="/admin/blog/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Post
          </Button>
        </Link>
      </div>

      <div className="rounded-md border bg-card">
        {isLoading ? (
          <div className="flex justify-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-primary" />
          </div>
        ) : (
          <table className="w-full">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="px-4 py-3 text-left text-sm font-medium">Title</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Author</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Status</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Published</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Views</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Actions</th>
              </tr>
            </thead>
            <tbody>
              {data?.items.map((post) => (
                <tr key={post.id} className="border-b last:border-0 hover:bg-muted/50">
                  <td className="px-4 py-3">
                    <div className="font-medium text-sm">{post.title}</div>
                    <div className="text-xs text-muted-foreground font-mono">{post.slug}</div>
                  </td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">
                    {post.author || "—"}
                  </td>
                  <td className="px-4 py-3">
                    <Badge variant={post.isPublished ? "default" : "secondary"}>
                      {post.isPublished ? "Published" : "Draft"}
                    </Badge>
                  </td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">
                    {post.publishedAt
                      ? new Date(post.publishedAt).toLocaleDateString()
                      : "—"}
                  </td>
                  <td className="px-4 py-3 text-sm text-muted-foreground">{post.viewCount}</td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-1">
                      {post.isPublished && (
                        <Link href={`/blog/${post.slug}`} target="_blank">
                          <Button variant="ghost" size="sm" title="View live">
                            <Eye className="h-4 w-4" />
                          </Button>
                        </Link>
                      )}
                      <Link href={`/admin/blog/${post.id}`}>
                        <Button variant="ghost" size="sm" title="Edit">
                          <Pencil className="h-4 w-4" />
                        </Button>
                      </Link>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDelete(post.id, post.title)}
                        disabled={deleteMutation.isPending}
                        title="Delete"
                      >
                        <Trash2 className="h-4 w-4 text-destructive" />
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
              {data?.items.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-4 py-12 text-center text-muted-foreground">
                    No blog posts yet. Create your first post.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-center gap-2">
          <Button
            variant="outline"
            size="sm"
            disabled={page === 1}
            onClick={() => setPage((p) => p - 1)}
          >
            Previous
          </Button>
          <span className="text-sm text-muted-foreground">
            Page {page} of {data.totalPages}
          </span>
          <Button
            variant="outline"
            size="sm"
            disabled={page >= data.totalPages}
            onClick={() => setPage((p) => p + 1)}
          >
            Next
          </Button>
        </div>
      )}
    </div>
  );
}
