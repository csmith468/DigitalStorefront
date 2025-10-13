using API.Database;

namespace API.Models.Dbo;

[DbTable("dbo.productType")]
public class ProductType
{
    [DbPrimaryKey] public int ProductTypeId { get; set; }

    [DbColumn] public string TypeName { get; set; } = "";
    
    [DbColumn] public string TypeCode { get; set; } = "";
    
    [DbColumn] public string? Description { get; set; }
}