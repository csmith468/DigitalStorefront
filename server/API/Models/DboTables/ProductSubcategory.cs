using API.Database;

namespace API.Models.DboTables;

[DbTable("dbo.productSubcategory")]
public class ProductSubcategory
{
    [DbPrimaryKey] public int ProductSubcategoryId { get; set; }
    
    [DbColumn] public int ProductId { get; set; }
    
    [DbColumn] public int SubcategoryId { get; set; }

    [DbColumn] public int DisplayOrder { get; set; } = 0;
    
    [DbColumn] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [DbColumn] public DateTime? UpdatedAt { get; set; }
    
    [DbColumn] public int CreatedBy { get; set; }
    
    [DbColumn] public int? UpdatedBy { get; set; }
}