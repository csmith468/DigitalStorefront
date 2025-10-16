namespace API.Models.Dtos;

public class ProductImageDto
{
    public int ProductImageId { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = "";
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }  // Computed: displayOrder == 0
    public int DisplayOrder { get; set; }
}

public class AddProductImageDto
{
    public IFormFile File { get; set; } = null!;
    public string? AltText { get; set; }
    public bool SetAsPrimary { get; set; }  // Renamed for clarity
}