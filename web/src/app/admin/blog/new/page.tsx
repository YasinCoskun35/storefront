"use client";

import { BlogPostForm } from "@/components/admin/blog-post-form";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";

export default function NewBlogPostPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/admin/blog">
          <button className="flex h-10 w-10 items-center justify-center rounded-lg border bg-background hover:bg-muted">
            <ArrowLeft className="h-5 w-5" />
          </button>
        </Link>
        <div>
          <h1 className="font-display text-3xl font-bold text-secondary">New Blog Post</h1>
          <p className="text-muted-foreground">Write and publish a new article</p>
        </div>
      </div>

      <BlogPostForm />
    </div>
  );
}
