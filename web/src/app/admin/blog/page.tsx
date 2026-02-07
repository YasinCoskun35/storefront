"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { contentApi, BlogPostSummary } from "@/lib/api";
import { DataTable } from "@/components/admin/data-table";
import { Button } from "@/components/ui/button";
import { Plus, Pencil, Trash2 } from "lucide-react";
import Link from "next/link";

export default function AdminBlogPage() {
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ["admin-blog", page],
    queryFn: () => contentApi.getBlogPosts({ pageNumber: page, pageSize: 10 }),
  });

  const columns = [
    {
      header: "Title",
      accessor: "title" as keyof BlogPostSummary,
      cell: (value: string) => <span className="font-medium">{value}</span>,
    },
    {
      header: "Slug",
      accessor: "slug" as keyof BlogPostSummary,
      cell: (value: string) => (
        <span className="text-xs text-muted-foreground font-mono">{value}</span>
      ),
    },
    {
      header: "Author",
      accessor: "author" as keyof BlogPostSummary,
    },
    {
      header: "Published",
      accessor: "publishedAt" as keyof BlogPostSummary,
      cell: (value: string) =>
        value ? new Date(value).toLocaleDateString() : "Draft",
    },
    {
      header: "Views",
      accessor: "viewCount" as keyof BlogPostSummary,
    },
    {
      header: "Actions",
      accessor: (row: BlogPostSummary) => (
        <div className="flex items-center gap-2">
          <Link href={`/admin/blog/${row.id}`}>
            <Button variant="ghost" size="sm">
              <Pencil className="h-4 w-4" />
            </Button>
          </Link>
          <Button variant="ghost" size="sm">
            <Trash2 className="h-4 w-4 text-destructive" />
          </Button>
        </div>
      ),
    },
  ];

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Blog Posts</h1>
          <p className="text-muted-foreground">Manage your blog content</p>
        </div>
        <Link href="/admin/blog/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Post
          </Button>
        </Link>
      </div>

      <DataTable
        columns={columns}
        data={data?.items || []}
        currentPage={page}
        totalPages={data?.totalPages || 1}
        onPageChange={setPage}
      />
    </div>
  );
}

