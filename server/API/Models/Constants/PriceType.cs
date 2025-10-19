using API.Database;

namespace API.Models.Constants;

[DbTable("dbo.priceType")]
public class PriceType
{
    public int PriceTypeId { get; set; }

    public string PriceTypeName { get; set; } = "";
    
    public string PriceTypeCode { get; set; } = "";

    public string Icon { get; set; } = "";
}

public static class PriceTypes
{
    public static readonly List<PriceType> All =
    [
        new PriceType { PriceTypeId = 1, PriceTypeName = "Coins", Icon = "★" },
        new PriceType { PriceTypeId = 2, PriceTypeName = "USD", Icon = "$" }
    ];

    public static string GetIcon(int priceTypeId)
    {
        return All.FirstOrDefault(p => p.PriceTypeId == priceTypeId)?.Icon ?? "";
    }
}