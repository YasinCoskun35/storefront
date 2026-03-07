namespace Storefront.Modules.Orders.Core.Domain.Entities;

public class CartItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string CartId { get; set; } = string.Empty;
    public virtual Cart Cart { get; set; } = null!;

    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }

    // JSON: [{"groupId":"...","groupName":"Fabric","optionId":"...","optionName":"Royal Blue","optionCode":"RB-001","hexColor":"#1E3A8A"}]
    public string? SelectedVariants { get; set; }

    public int Quantity { get; set; } = 1;

    public string? CustomizationNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
