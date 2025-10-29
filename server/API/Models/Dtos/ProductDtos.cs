using API.Models.DboTables;

namespace API.Models.Dtos;

public class ProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int ProductTypeId { get; set; }
    public bool IsTradeable { get; set; }
    public bool IsNew { get; set; }
    public bool IsPromotional { get; set; }
    public bool IsExclusive { get; set; }
    public string Sku { get; set; } = "";
    public decimal Price { get; set; }
    public decimal PremiumPrice { get; set; }
    public string PriceIcon { get; set; } = "";
    public bool IsDemoProduct { get; set; }
    public ProductImageDto? PrimaryImage { get; set; }
}

public class ProductDetailDto : ProductDto
{
    public string? Description { get; set; }
    public int PriceTypeId { get; set; }
    public List<ProductImageDto> Images { get; set; } = [];
    public List<SubcategoryDto> Subcategories { get; set; } = [];
    public List<TagDto> Tags { get; set; } = [];
}

// FUTURE: IsActive 
public class ProductFormDto
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int ProductTypeId { get; set; }
    public string? Description { get; set; }
    public bool IsTradeable { get; set; }
    public bool IsNew { get; set; }
    public bool IsPromotional { get; set; }
    public bool IsExclusive { get; set; }
    public decimal Price { get; set; }
    public decimal PremiumPrice { get; set; }
    public int PriceTypeId { get; set; }
    public List<int> SubcategoryIds { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}

public class ProductTypeDto: ProductType;

public class ProductFilterParams : PaginationParams
{
    public string? Search { get; set; }
    public int? ProductTypeId { get; set; }
    public string? CategorySlug { get; set; }
    public string? SubcategorySlug { get; set; }
}