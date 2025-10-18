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
    public ProductImageDto? PrimaryImage { get; set; }
}

public class ProductDetailDto : ProductDto
{
    public string? Description { get; set; }
    public List<ProductImageDto> Images { get; set; } = [];
}