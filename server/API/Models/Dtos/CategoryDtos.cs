namespace API.Models.Dtos;

public class CategoryDto
{
    public int? CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int DisplayOrder { get; set; } = 0;
    public List<SubcategoryDto> Subcategories { get; set; } = [];
}

public class SubcategoryDto
{
    public int? SubcategoryId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int DisplayOrder { get; set; } = 0;
    public string? ImageUrl { get; set; }
}