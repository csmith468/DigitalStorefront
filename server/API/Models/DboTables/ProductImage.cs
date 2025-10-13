using API.Database;

namespace API.Models.DboTables;

[DbTable("dbo.productImage")]
public class ProductImage
{
    [DbPrimaryKey] public int ProductImageId { get; set; }

    [DbColumn] public int ProductId { get; set; }
    
    [DbColumn] public string ImageUrl { get; set; } = "";
    
    [DbColumn] public string? AltText { get; set; }
        
    [DbColumn] public int DisplayOrder { get; set; } = 0;

    [DbColumn] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [DbColumn] public DateTime? UpdatedAt { get; set; }
    
    [DbColumn] public int CreatedBy { get; set; }
    
    [DbColumn] public int? UpdatedBy { get; set; }

}