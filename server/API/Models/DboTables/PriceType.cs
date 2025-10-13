using API.Database;

namespace API.Models.DboTables;

[DbTable("dbo.priceType")]
public class PriceType
{
    [DbPrimaryKey] public int PriceTypeId { get; set; }

    [DbColumn] public string PriceTypeName { get; set; } = "";
    
    [DbColumn] public string PriceTypeCode { get; set; } = "";

    [DbColumn] public string Icon { get; set; } = "";
}