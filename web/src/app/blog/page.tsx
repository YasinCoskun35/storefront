import { contentApi } from "@/lib/api";
import Image from "next/image";
import Link from "next/link";
import { getImageUrl } from "@/lib/utils";
import { Calendar, User } from "lucide-react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

interface BlogPageProps {
  searchParams: Promise<{
    category?: string;
    pageNumber?: string;
  }>;
}

export default async function BlogPage({ searchParams }: BlogPageProps) {
  const params = await searchParams;
  const pageNumber = parseInt(params.pageNumber || "1");

  const blogPosts = await contentApi.getBlogPosts({
    category: params.category,
    pageNumber,
    pageSize: 9,
  });

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2">Blog</h1>
        <p className="text-muted-foreground">
          Tips, guides, and news from the hardware world
        </p>
      </div>

      {blogPosts.items.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-lg text-muted-foreground">
            No blog posts found. Check back soon!
          </p>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {blogPosts.items.map((post) => (
              <Link key={post.id} href={`/blog/${post.slug}`}>
                <Card className="overflow-hidden hover:shadow-lg transition-shadow h-full">
                  {post.featuredImage && (
                    <div className="relative aspect-video overflow-hidden bg-muted">
                      <Image
                        src={getImageUrl(post.featuredImage)}
                        alt={post.title}
                        fill
                        className="object-cover transition-transform hover:scale-105"
                      />
                    </div>
                  )}
                  <CardHeader>
                    <CardTitle className="line-clamp-2">{post.title}</CardTitle>
                    {post.summary && (
                      <CardDescription className="line-clamp-3">
                        {post.summary}
                      </CardDescription>
                    )}
                  </CardHeader>
                  <CardContent>
                    <div className="flex items-center gap-4 text-sm text-muted-foreground">
                      {post.author && (
                        <div className="flex items-center gap-1">
                          <User className="h-4 w-4" />
                          <span>{post.author}</span>
                        </div>
                      )}
                      {post.publishedAt && (
                        <div className="flex items-center gap-1">
                          <Calendar className="h-4 w-4" />
                          <span>
                            {new Date(post.publishedAt).toLocaleDateString()}
                          </span>
                        </div>
                      )}
                    </div>
                  </CardContent>
                </Card>
              </Link>
            ))}
          </div>

          {/* Simple Pagination Info */}
          {blogPosts.totalPages > 1 && (
            <div className="mt-8 text-center text-sm text-muted-foreground">
              Page {blogPosts.pageNumber} of {blogPosts.totalPages}
            </div>
          )}
        </>
      )}
    </div>
  );
}

