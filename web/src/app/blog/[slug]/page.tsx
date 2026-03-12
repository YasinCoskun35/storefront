import { getImageUrl } from "@/lib/utils";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";
import { Calendar, User, Tag, ArrowLeft } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { Metadata } from "next";

interface BlogPostPageProps {
  params: Promise<{ slug: string }>;
}

export async function generateMetadata({ params }: BlogPostPageProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const post = await fetchPost(slug);
    return {
      title: post.title,
      description: post.summary,
    };
  } catch {
    return { title: "Blog Post" };
  }
}

async function fetchPost(slug: string) {
  const res = await fetch(
    `${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080"}/api/content/blog/${slug}`,
    { next: { revalidate: 300 } }
  );
  if (!res.ok) throw new Error("Not found");
  return res.json();
}

export default async function BlogPostPage({ params }: BlogPostPageProps) {
  const { slug } = await params;

  let post: any;
  try {
    post = await fetchPost(slug);
  } catch {
    notFound();
  }

  const tags = post.tags
    ? post.tags.split(",").map((t: string) => t.trim()).filter(Boolean)
    : [];

  return (
    <div className="container mx-auto px-4 py-8 max-w-4xl">
      {/* Back */}
      <Link
        href="/blog"
        className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-primary mb-8 transition-colors"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Blog
      </Link>

      {/* Header */}
      <article>
        <header className="mb-8">
          {post.category && (
            <Badge variant="outline" className="mb-4">
              {post.category}
            </Badge>
          )}
          <h1 className="font-display text-4xl font-bold text-secondary mb-4 leading-tight">
            {post.title}
          </h1>

          {post.summary && (
            <p className="text-lg text-muted-foreground mb-6 leading-relaxed">
              {post.summary}
            </p>
          )}

          <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground pb-6 border-b">
            {post.author && (
              <div className="flex items-center gap-1.5">
                <User className="h-4 w-4" />
                <span>{post.author}</span>
              </div>
            )}
            {post.publishedAt && (
              <div className="flex items-center gap-1.5">
                <Calendar className="h-4 w-4" />
                <span>
                  {new Date(post.publishedAt).toLocaleDateString("en-US", {
                    year: "numeric",
                    month: "long",
                    day: "numeric",
                  })}
                </span>
              </div>
            )}
          </div>
        </header>

        {/* Featured Image */}
        {post.featuredImage && (
          <div className="relative aspect-video overflow-hidden rounded-xl mb-8 bg-muted">
            <Image
              src={getImageUrl(post.featuredImage)}
              alt={post.title}
              fill
              className="object-cover"
              unoptimized
            />
          </div>
        )}

        {/* Body */}
        <div
          className="prose prose-gray max-w-none prose-headings:font-display prose-headings:text-secondary prose-a:text-primary prose-img:rounded-lg"
          dangerouslySetInnerHTML={{ __html: post.body }}
        />

        {/* Tags */}
        {tags.length > 0 && (
          <div className="mt-10 pt-6 border-t">
            <div className="flex items-center gap-2 flex-wrap">
              <Tag className="h-4 w-4 text-muted-foreground" />
              {tags.map((tag: string) => (
                <Link
                  key={tag}
                  href={`/blog?tag=${encodeURIComponent(tag)}`}
                  className="text-sm text-muted-foreground hover:text-primary transition-colors"
                >
                  #{tag}
                </Link>
              ))}
            </div>
          </div>
        )}
      </article>
    </div>
  );
}
