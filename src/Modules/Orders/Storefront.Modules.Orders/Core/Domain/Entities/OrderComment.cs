using Storefront.Modules.Orders.Core.Domain.Enums;

namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class OrderComment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Order Reference
    public string OrderId { get; set; } = string.Empty;
    public virtual Order Order { get; set; } = null!;
    
    // Comment Details
    public string Content { get; set; } = string.Empty;
    public CommentType Type { get; set; } = CommentType.General;
    
    // Author Information
    public string AuthorId { get; set; } = string.Empty; // User ID (admin or partner)
    public string AuthorName { get; set; } = string.Empty; // Display name
    public string AuthorType { get; set; } = string.Empty; // "Admin" or "Partner"
    
    // Visibility
    public bool IsInternal { get; set; } = false; // Only visible to admins
    public bool IsSystemGenerated { get; set; } = false; // Auto-generated (e.g., status changes)
    
    // Attachments
    public string? AttachmentUrl { get; set; } // Optional file attachment
    public string? AttachmentFileName { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
