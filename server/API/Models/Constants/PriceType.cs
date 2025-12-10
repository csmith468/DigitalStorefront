using API.Database;

namespace API.Models.Constants;

public class PriceType
{
    public int PriceTypeId { get; set; }

    public string PriceTypeName { get; set; } = "";
    
    public string PriceTypeCode { get; set; } = "";

    public string Icon { get; set; } = "";
}

public static class PriceTypes
{
    public const int Coins = 1;
    public const int Usd = 2;
    
    public static readonly List<PriceType> All =
    [
        new PriceType { PriceTypeId = Coins, PriceTypeName = "Coins", Icon = "★" },
        new PriceType { PriceTypeId = Usd, PriceTypeName = "USD", Icon = "$" }
    ];

    public static string GetIcon(int priceTypeId)
    {
        return All.FirstOrDefault(p => p.PriceTypeId == priceTypeId)?.Icon ?? "";
    }
}