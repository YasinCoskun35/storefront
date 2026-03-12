namespace Storefront.Modules.Identity.Core.Domain.Enums;

public static class PartnerScope
{
    public const string ViewCatalog = "ViewCatalog";
    public const string ViewPrices = "ViewPrices";
    public const string ManageCart = "ManageCart";
    public const string PlaceOrders = "PlaceOrders";
    public const string ViewOrders = "ViewOrders";
    public const string ManageTeam = "ManageTeam";

    public static readonly string[] All = [ViewCatalog, ViewPrices, ManageCart, PlaceOrders, ViewOrders, ManageTeam];
}
