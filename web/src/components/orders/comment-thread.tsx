"use client";

import { useState } from "react";
import { OrderComment, COMMENT_TYPE_LABELS, CommentType } from "@/lib/api/orders";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card } from "@/components/ui/card";
import { Send, Paperclip, Lock } from "lucide-react";
import { formatDistanceToNow } from "date-fns";

interface CommentThreadProps {
  comments: OrderComment[];
  onAddComment: (content: string, type: CommentType) => Promise<void>;
  isAdmin?: boolean;
}

export function CommentThread({
  comments,
  onAddComment,
  isAdmin = false,
}: CommentThreadProps) {
  const [newComment, setNewComment] = useState("");
  const [commentType, setCommentType] = useState<CommentType>(CommentType.General);
  const [isInternal, setIsInternal] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newComment.trim() || isSubmitting) return;

    setIsSubmitting(true);
    try {
      await onAddComment(newComment, commentType);
      setNewComment("");
      setCommentType(CommentType.General);
      setIsInternal(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Filter out internal comments if not admin
  const visibleComments = isAdmin 
    ? comments 
    : comments.filter(c => !c.isInternal);

  return (
    <div className="space-y-4">
      <h3 className="text-lg font-medium">Comments & Updates</h3>

      {/* Comments List */}
      <div className="space-y-3 max-h-96 overflow-y-auto">
        {visibleComments.map((comment) => (
          <Card key={comment.id} className="p-4">
            <div className="flex items-start justify-between mb-2">
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-gray-900">
                  {comment.authorName}
                </span>
                <span className={`text-xs px-2 py-0.5 rounded-full ${
                  comment.authorType === "Admin" 
                    ? "bg-purple-100 text-purple-800"
                    : comment.authorType === "Partner"
                    ? "bg-blue-100 text-blue-800"
                    : "bg-gray-100 text-gray-800"
                }`}>
                  {comment.authorType}
                </span>
                {comment.isInternal && (
                  <span className="text-xs px-2 py-0.5 rounded-full bg-yellow-100 text-yellow-800 flex items-center gap-1">
                    <Lock className="w-3 h-3" />
                    Internal
                  </span>
                )}
              </div>
              <span className="text-xs text-gray-500">
                {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
              </span>
            </div>

            <p className="text-sm text-gray-700 whitespace-pre-wrap">
              {comment.content}
            </p>

            {comment.attachmentUrl && (
              <div className="mt-2 flex items-center gap-2 text-sm text-blue-600">
                <Paperclip className="w-4 h-4" />
                <a 
                  href={comment.attachmentUrl} 
                  target="_blank" 
                  rel="noopener noreferrer"
                  className="hover:underline"
                >
                  {comment.attachmentFileName || "View attachment"}
                </a>
              </div>
            )}

            <div className="mt-2 text-xs text-gray-500">
              Type: {COMMENT_TYPE_LABELS[comment.type as unknown as CommentType] || comment.type}
            </div>
          </Card>
        ))}

        {visibleComments.length === 0 && (
          <div className="text-center py-8 text-gray-500">
            No comments yet. Be the first to add one!
          </div>
        )}
      </div>

      {/* Add Comment Form */}
      <form onSubmit={handleSubmit} className="space-y-3">
        <Textarea
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Add a comment..."
          rows={3}
          disabled={isSubmitting}
        />

        <div className="flex items-center gap-3">
          <select
            value={commentType}
            onChange={(e) => setCommentType(Number(e.target.value) as CommentType)}
            className="px-3 py-2 border rounded-md text-sm"
            disabled={isSubmitting}
          >
            <option value={CommentType.General}>General</option>
            <option value={CommentType.Quote}>Quote</option>
            <option value={CommentType.Payment}>Payment</option>
            <option value={CommentType.Shipping}>Shipping</option>
          </select>

          {isAdmin && (
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={isInternal}
                onChange={(e) => setIsInternal(e.target.checked)}
                disabled={isSubmitting}
                className="rounded"
              />
              Internal note (admins only)
            </label>
          )}

          <Button 
            type="submit" 
            disabled={!newComment.trim() || isSubmitting}
            className="ml-auto"
          >
            <Send className="w-4 h-4 mr-2" />
            {isSubmitting ? "Sending..." : "Send"}
          </Button>
        </div>
      </form>
    </div>
  );
}
