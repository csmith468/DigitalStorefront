using API.Database;

namespace API.Models.Dbo;

[DbTable("dbo.product")]
public class Product
{
    [DbPrimaryKey] public int ProductId { get; set; }

    [DbColumn] public string Name { get; set; } = "";
    
    [DbColumn] public string Slug { get; set; } = "";
    
    [DbColumn] public string? Description { get; set; }
    
    [DbColumn] public int ProductTypeId { get; set; }

    [DbColumn] public bool IsTradeable { get; set; } = false;

    [DbColumn] public bool IsNew { get; set; } = false;

    [DbColumn] public bool IsPromotional { get; set; } = false;

    [DbColumn] public bool IsExclusive { get; set; } = false;
    
    [DbColumn] public string Sku { get; set; } = "";
    
    [DbColumn] public int PriceTypeId { get; set; }
    
    [DbColumn] public decimal Price { get; set; }
    
    [DbColumn] public decimal PremiumPrice { get; set; }
    
    [DbColumn] public string? ImageUrl { get; set; }

    [DbColumn] public DateTime CreatedAt { get; set; } = DateTime.Now;

    [DbColumn] public DateTime? UpdatedAt { get; set; }
}