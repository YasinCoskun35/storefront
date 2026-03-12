"use client";

import { useQuery } from "@tanstack/react-query";
import { api } from "@/lib/api";
import { BlogPostForm } from "@/components/admin/blog-post-form";
import { ArrowLeft, Loader2 } from "lucide-react";
import Link from "next/link";
import { use } from "react";

interface EditBlogPostPageProps {
  params: Promise<{ id: string }>;
}

async function fetchPost(id: string) {
  const res = await api.get(`/api/content/blog/admin/all`, {
    params: { pageSize: 100 },
  });
  return res.data.items.find((p: any) => p.id === id) ?? null;
}

export default function EditBlogPostPage({ params }: EditBlogPostPageProps) {
  const { id } = use(params);

  const { data: post, isLoading } = useQuery({
    queryKey: ["admin-blog-post", id],
    queryFn: () => fetchPost(id),
  });

  if (isLoading) {
    return (
      <div className="flex justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/admin/blog">
          <button className="flex h-10 w-10 items-center justify-center rounded-lg border bg-background hover:bg-muted">
            <ArrowLeft className="h-5 w-5" />
          </button>
        </Link>
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary">Edit Blog Post</h1>
          {post && <p className="text-muted-foreground">{post.title}</p>}
        </div>
      </div>

      <BlogPostForm postId={id} initialData={post} />
    </div>
  );
}
