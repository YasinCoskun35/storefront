namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string OrderId { get; set; } = string.Empty;
    public virtual Order Order { get; set; } = null!;

    // Denormalized for historical record
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }

    // JSON: [{"groupId":"...","groupName":"Fabric","optionId":"...","optionName":"Royal Blue","optionCode":"RB-001","hexColor":"#1E3A8A"}]
    public string? SelectedVariants { get; set; }

    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TotalPrice { get; set; }

    public string? CustomizationNotes { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
