using API.Database;

namespace API.Models.DboTables;

[DbTable("dbo.productSubcategory")]
public class ProductSubcategory
{
    [DbPrimaryKey] public int ProductSubcategoryId { get; set; }
    
    [DbColumn] public int ProductId { get; set; }
    
    [DbColumn] public int SubcategoryId { get; set; }

    [DbColumn] public int DisplayOrder { get; set; } = 0;
}