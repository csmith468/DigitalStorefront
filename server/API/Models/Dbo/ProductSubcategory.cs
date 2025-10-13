using API.Database;

namespace API.Models.Dbo;

[DbTable("dbo.productSubcategory")]
public class ProductSubcategory
{
    [DbColumn] public int ProductId { get; set; }
    
    [DbColumn] public int SubcategoryId { get; set; }

    [DbColumn] public int DisplayOrder { get; set; } = 0;
    
    [DbColumn] public DateTime CreatedAt { get; set; } = DateTime.Now;
}