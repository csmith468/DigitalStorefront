using API.Database;

namespace API.Models.DboTables;

[DbTable("dbo.subcategory")]
public class Subcategory
{
    [DbPrimaryKey] public int SubcategoryId { get; set; }

    [DbColumn] public int CategoryId { get; set; }
    
    [DbColumn] public string Name { get; set; } = "";
    
    [DbColumn] public string Slug { get; set; } = "";

    [DbColumn] public int DisplayOrder { get; set; } = 0;
    
    [DbColumn] public string? ImageUrl { get; set; }

    [DbColumn] public bool IsActive { get; set; } = true;
    
    [DbColumn] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [DbColumn] public DateTime? UpdatedAt { get; set; }
    
    [DbColumn] public int CreatedBy { get; set; }
    
    [DbColumn] public int? UpdatedBy { get; set; }
}